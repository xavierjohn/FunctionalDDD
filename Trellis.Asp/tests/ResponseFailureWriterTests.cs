namespace Trellis.Asp.Tests;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Trellis;
using Trellis.Asp;

/// <summary>
/// Branch coverage for <see cref="ResponseFailureWriter"/>: companion headers (Allow, Retry-After,
/// Content-Range), validation problem vs problem path, status redaction for 5xx, and the
/// extensions builder (faultId, rules).
/// </summary>
public sealed class ResponseFailureWriterTests
{
    private static DefaultHttpContext NewContext()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IProblemDetailsService, NoopPds>();
        var ctx = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }

    /// <summary>
    /// Builds an HttpContext whose RequestServices include a stub <see cref="IAuthenticationSchemeProvider"/>
    /// reporting <paramref name="defaultChallengeScheme"/> as the default challenge scheme. Used to exercise
    /// the synthesis path in <see cref="ResponseFailureWriter"/> without spinning up the real
    /// ASP.NET Core authentication subsystem.
    /// </summary>
    private static DefaultHttpContext NewAuthContext(string defaultChallengeScheme)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IProblemDetailsService, NoopPds>();
        services.AddSingleton<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>(
            new StubAuthSchemeProvider(defaultChallengeScheme));
        var ctx = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }

    private sealed class StubAuthSchemeProvider(string defaultChallengeScheme)
        : Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider
    {
        private readonly Microsoft.AspNetCore.Authentication.AuthenticationScheme _scheme =
            new(defaultChallengeScheme, defaultChallengeScheme,
                typeof(Microsoft.AspNetCore.Authentication.IAuthenticationHandler));

        public void AddScheme(Microsoft.AspNetCore.Authentication.AuthenticationScheme scheme) { }
        public Task<IEnumerable<Microsoft.AspNetCore.Authentication.AuthenticationScheme>> GetAllSchemesAsync()
            => Task.FromResult<IEnumerable<Microsoft.AspNetCore.Authentication.AuthenticationScheme>>([_scheme]);
        public Task<Microsoft.AspNetCore.Authentication.AuthenticationScheme?> GetDefaultAuthenticateSchemeAsync()
            => Task.FromResult<Microsoft.AspNetCore.Authentication.AuthenticationScheme?>(_scheme);
        public Task<Microsoft.AspNetCore.Authentication.AuthenticationScheme?> GetDefaultChallengeSchemeAsync()
            => Task.FromResult<Microsoft.AspNetCore.Authentication.AuthenticationScheme?>(_scheme);
        public Task<Microsoft.AspNetCore.Authentication.AuthenticationScheme?> GetDefaultForbidSchemeAsync()
            => Task.FromResult<Microsoft.AspNetCore.Authentication.AuthenticationScheme?>(_scheme);
        public Task<Microsoft.AspNetCore.Authentication.AuthenticationScheme?> GetDefaultSignInSchemeAsync()
            => Task.FromResult<Microsoft.AspNetCore.Authentication.AuthenticationScheme?>(_scheme);
        public Task<Microsoft.AspNetCore.Authentication.AuthenticationScheme?> GetDefaultSignOutSchemeAsync()
            => Task.FromResult<Microsoft.AspNetCore.Authentication.AuthenticationScheme?>(_scheme);
        public Task<IEnumerable<Microsoft.AspNetCore.Authentication.AuthenticationScheme>> GetRequestHandlerSchemesAsync()
            => Task.FromResult<IEnumerable<Microsoft.AspNetCore.Authentication.AuthenticationScheme>>([]);
        public Task<Microsoft.AspNetCore.Authentication.AuthenticationScheme?> GetSchemeAsync(string name)
            => Task.FromResult<Microsoft.AspNetCore.Authentication.AuthenticationScheme?>(
                name == defaultChallengeScheme ? _scheme : null);
        public void RemoveScheme(string name) { }
    }

    private sealed class NoopPds : IProblemDetailsService
    {
        public ValueTask WriteAsync(ProblemDetailsContext c) => ValueTask.CompletedTask;
#pragma warning disable CA1822
        public bool TryWrite(ProblemDetailsContext c) => false;
#pragma warning restore CA1822
    }

    private sealed record T(int Id);

    [Fact]
    public async Task Unauthorized_with_single_Bearer_challenge_emits_WwwAuthenticate_header()
    {
        // RFC 9110 §11.6.1: a 401 response MUST include a WWW-Authenticate header listing
        // every challenge applicable to the target resource. Error.Unauthorized.Challenges
        // is the round-trip carrier; ResponseFailureWriter must emit the header.
        var ctx = NewContext();
        var challenge = new AuthChallenge(
            "Bearer",
            ImmutableDictionary<string, string>.Empty
                .Add("realm", "api")
                .Add("scope", "read"));
        var r = Result.Fail<T>(new Error.Unauthorized(EquatableArray.Create(challenge)));

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(401);
        var values = ctx.Response.Headers["WWW-Authenticate"];
        values.Count.Should().Be(1, "a single challenge should produce a single header value");
        var header = values.ToString();
        header.Should().StartWith("Bearer ", "the auth-scheme leads the challenge");
        header.Should().Contain("realm=\"api\"");
        header.Should().Contain("scope=\"read\"");
    }

    [Fact]
    public async Task Unauthorized_with_scheme_only_challenge_emits_bare_scheme()
    {
        // A scheme without parameters is valid (e.g. "Bearer" alone). No trailing space
        // or empty parameter section.
        var ctx = NewContext();
        var r = Result.Fail<T>(new Error.Unauthorized(
            EquatableArray.Create(new AuthChallenge("Bearer"))));

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(401);
        var values = ctx.Response.Headers["WWW-Authenticate"];
        values.Count.Should().Be(1);
        values.ToString().Should().Be("Bearer", "scheme-only challenges must not have a trailing space or = sign");
    }

    [Fact]
    public async Task Unauthorized_with_multiple_challenges_emits_one_header_value_per_challenge()
    {
        // RFC 9110 §11.6.1: WWW-Authenticate is a list-valued header; multiple challenges
        // can be combined into one comma-separated value or repeated headers. ASP.NET Core
        // authentication handlers conventionally append one header per challenge so the
        // emitted shape is unambiguous to downstream middleware that doesn't parse list
        // syntax. ResponseFailureWriter follows the same convention.
        var ctx = NewContext();
        var r = Result.Fail<T>(new Error.Unauthorized(EquatableArray.Create(
            new AuthChallenge("Bearer", ImmutableDictionary<string, string>.Empty.Add("realm", "api")),
            new AuthChallenge("Basic", ImmutableDictionary<string, string>.Empty.Add("realm", "legacy")))));

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(401);
        var values = ctx.Response.Headers["WWW-Authenticate"];
        values.Count.Should().Be(2, "two challenges should produce two header values");
        values.Should().Contain(v => v!.StartsWith("Bearer", System.StringComparison.Ordinal)
                                      && v.Contains("realm=\"api\"", System.StringComparison.Ordinal));
        values.Should().Contain(v => v!.StartsWith("Basic", System.StringComparison.Ordinal)
                                      && v.Contains("realm=\"legacy\"", System.StringComparison.Ordinal));
    }

    [Fact]
    public async Task Unauthorized_without_challenges_and_no_auth_configured_emits_no_WwwAuthenticate_header()
    {
        // Service without ASP.NET Core authentication registered: no IAuthenticationSchemeProvider,
        // no default challenge scheme. Synthesizing "Bearer" against a service that does not use
        // Bearer (or any auth) would mislead clients. Skip synthesis and preserve the current
        // behavior for services that do not wire authentication.
        var ctx = NewContext();
        var r = Result.Fail<T>(new Error.Unauthorized());

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(401);
        ctx.Response.Headers.ContainsKey("WWW-Authenticate")
            .Should().BeFalse("no challenges + no auth configured → no synthesized header");
    }

    [Fact]
    public async Task Unauthorized_without_challenges_synthesizes_default_challenge_scheme_from_AuthenticationSchemeProvider()
    {
        // RFC 9110 §11.6.1: every 401 response MUST carry WWW-Authenticate. The mediator
        // authorization pipeline emits Error.Unauthorized with empty Challenges (it does not
        // know the configured scheme); when the ASP.NET Core authentication subsystem is
        // configured, ResponseFailureWriter resolves the default challenge scheme via
        // IAuthenticationSchemeProvider and synthesizes a scheme-only challenge. Without this,
        // a mediator-emitted 401 on an anonymous-tolerant route (where the auth handler is
        // never invoked) would ship out with no challenge header — strictly RFC-non-compliant.
        var ctx = NewAuthContext(defaultChallengeScheme: "Bearer");
        var r = Result.Fail<T>(new Error.Unauthorized());

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(401);
        var values = ctx.Response.Headers["WWW-Authenticate"];
        values.Count.Should().Be(1);
        values.ToString().Should().Be("Bearer",
            "synthesized challenge must use the default challenge scheme registered with ASP.NET Core authentication");
    }

    [Fact]
    public async Task Unauthorized_without_challenges_synthesizes_using_configured_challenge_scheme_name()
    {
        // The synthesized challenge uses the scheme NAME registered with AddAuthentication.
        // Consumers who call AddJwtBearer("ApiJwt", ...) get "ApiJwt" rather than the wire token
        // "Bearer". This matches the registered name and is documented as the behavior. Consumers
        // needing a different wire token (e.g., a policy scheme that forwards to Bearer but should
        // emit "Bearer" on the wire) should populate Error.Unauthorized.Challenges explicitly —
        // the writer treats supplied Challenges as authoritative and skips synthesis.
        var ctx = NewAuthContext(defaultChallengeScheme: "ApiJwt");
        var r = Result.Fail<T>(new Error.Unauthorized());

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        var values = ctx.Response.Headers["WWW-Authenticate"];
        values.ToString().Should().Be("ApiJwt",
            "synthesized challenge uses the registered default-challenge scheme name verbatim");
    }

    [Fact]
    public async Task Unauthorized_with_explicit_challenges_does_not_synthesize()
    {
        // When the caller populates Error.Unauthorized.Challenges, that takes precedence —
        // the writer emits exactly what was provided and does not also synthesize. The
        // explicit shape (with realm/scope/error_description parameters) is the authoritative
        // one; synthesis is only a fallback for the empty-Challenges mediator-emitted path.
        var ctx = NewAuthContext(defaultChallengeScheme: "Bearer");
        var challenge = new AuthChallenge(
            "Bearer",
            ImmutableDictionary<string, string>.Empty.Add("realm", "explicit"));
        var r = Result.Fail<T>(new Error.Unauthorized(EquatableArray.Create(challenge)));

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        var values = ctx.Response.Headers["WWW-Authenticate"];
        values.Count.Should().Be(1);
        values.ToString().Should().Contain("realm=\"explicit\"",
            "supplied Challenges are authoritative; synthesis must not double-emit");
    }

    [Fact]
    public async Task Unauthorized_synthesis_skipped_when_response_already_carries_WwwAuthenticate()
    {
        // Defense against double-emission: if middleware or upstream code already set a
        // WWW-Authenticate header on the response (e.g., a custom auth pipeline already wrote
        // its challenge), the synthesized scheme-only fallback would be redundant and could
        // confuse clients that parse only the first header. Preserve what is already there.
        var ctx = NewAuthContext(defaultChallengeScheme: "Bearer");
        ctx.Response.Headers["WWW-Authenticate"] = "Bearer realm=\"upstream\"";
        var r = Result.Fail<T>(new Error.Unauthorized());

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        var values = ctx.Response.Headers["WWW-Authenticate"];
        values.Count.Should().Be(1);
        values.ToString().Should().Be("Bearer realm=\"upstream\"",
            "existing header must not be overwritten or appended-to by synthesis");
    }

    [Fact]
    public async Task Unauthorized_without_challenges_and_non_401_mapped_status_does_not_synthesize()
    {
        // Same status-aware gating as the explicit-challenges path: WWW-Authenticate is bound
        // to 401 per RFC 9110 §11.6.1. If WithErrorMapping promotes Error.Unauthorized to a
        // non-401 status, suppress synthesis along with the explicit-challenge emission.
        var ctx = NewAuthContext(defaultChallengeScheme: "Bearer");
        var r = Result.Fail<T>(new Error.Unauthorized());

        await r.ToHttpResponse(t => t, o => o.WithErrorMapping<Error.Unauthorized>(500))
            .ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(500);
        ctx.Response.Headers.ContainsKey("WWW-Authenticate")
            .Should().BeFalse("WWW-Authenticate is bound to 401; no synthesis on remapped statuses");
    }

    [Fact]
    public async Task Unauthorized_param_value_with_quote_and_backslash_is_escaped_per_RFC_9110()
    {
        // RFC 9110 §5.6.4 quoted-string: DQUOTE and backslash inside a quoted-string MUST
        // be escaped with a preceding backslash. A real-world case is challenge param
        // values like error_description carrying user-supplied phrases that may contain
        // quotes or backslashes.
        var ctx = NewContext();
        var challenge = new AuthChallenge(
            "Bearer",
            ImmutableDictionary<string, string>.Empty.Add("error_description", "bad \"token\" \\path"));
        var r = Result.Fail<T>(new Error.Unauthorized(EquatableArray.Create(challenge)));

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(401);
        var header = ctx.Response.Headers["WWW-Authenticate"].ToString();
        header.Should().Contain("error_description=\"bad \\\"token\\\" \\\\path\"",
            "quote and backslash inside a quoted-string MUST be backslash-escaped per RFC 9110 §5.6.4");
    }

    [Fact]
    public async Task Unauthorized_with_challenges_but_non_401_mapped_status_does_not_emit_WwwAuthenticate()
    {
        // Mirrors the m-13 design (ValidationProblem_with_5xx_status_scrubs_detail): when
        // WithErrorMapping promotes Error.Unauthorized to a non-401 status, companion
        // headers tied to the original 401 semantics must not be emitted. RFC 9110 §11.6.1
        // ties WWW-Authenticate to 401 specifically; emitting it on a 500 (or any other
        // mapped status) would mislead clients into attempting re-authentication.
        var ctx = NewContext();
        var error = new Error.Unauthorized(EquatableArray.Create(new AuthChallenge("Bearer")));
        var r = Result.Fail<T>(error);

        await r.ToHttpResponse(t => t, o => o.WithErrorMapping<Error.Unauthorized>(500))
            .ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(500);
        ctx.Response.Headers.ContainsKey("WWW-Authenticate")
            .Should().BeFalse("WWW-Authenticate is meaningful only on 401 responses per RFC 9110 §11.6.1");
    }

    [Fact]
    public async Task ServiceUnavailable_with_RetryAfter_emits_RetryAfter_header()
    {
        var ctx = NewContext();
        var r = Result.Fail<T>(new Error.ServiceUnavailable(RetryAfterValue.FromSeconds(60)));

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(503);
        ctx.Response.Headers["Retry-After"].ToString().Should().Be("60");
    }

    [Fact]
    public async Task RangeNotSatisfiable_emits_ContentRange_header()
    {
        var ctx = NewContext();
        var r = Result.Fail<T>(new Error.RangeNotSatisfiable(1234));

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(416);
        ctx.Response.Headers["Content-Range"].ToString().Should().Be("bytes */1234");
    }

    [Fact]
    public async Task UnprocessableContent_with_field_violations_writes_validation_problem()
    {
        var ctx = NewContext();
        var fields = EquatableArray.Create(
            new FieldViolation(new InputPointer("/email"), "format", null, "must be email"),
            new FieldViolation(new InputPointer("/email"), "required", null, "required"));
        var r = Result.Fail<T>(new Error.UnprocessableContent(fields));

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(422);
    }

    [Fact]
    public async Task UnprocessableContent_with_only_rules_writes_validation_problem()
    {
        var ctx = NewContext();
        var rules = EquatableArray.Create(
            new RuleViolation("must_have_items",
                EquatableArray.Create(new InputPointer("/items")),
                null, "Order must have items."));
        var r = Result.Fail<T>(new Error.UnprocessableContent(default, rules));

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(422);
    }

    [Fact]
    public async Task UnprocessableContent_empty_falls_back_to_plain_problem()
    {
        var ctx = NewContext();
        // No fields, no rules: skips ValidationProblem and writes plain Problem.
        var r = Result.Fail<T>(new Error.UnprocessableContent(default));

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(422);
    }

    [Fact]
    public async Task InternalServerError_writes_500_problem_response()
    {
        var ctx = NewContext();
        var r = Result.Fail<T>(new Error.InternalServerError("FAULT-7") { Detail = "stack trace leak" });

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task TooManyRequests_without_RetryAfter_does_not_emit_header()
    {
        var ctx = NewContext();
        var r = Result.Fail<T>(new Error.TooManyRequests());

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(429);
        ctx.Response.Headers.ContainsKey("Retry-After").Should().BeFalse();
    }

    [Fact]
    public async Task ValidationProblem_with_5xx_status_scrubs_detail()
    {
        // Regression for m-13: when a custom WithErrorMapping promotes UnprocessableContent
        // to a 5xx status, the validation-branch detail must be scrubbed identically to
        // the plain Problem branch. Previously the validation branch leaked unprocessable.Detail.
        var ctx = NewContext();
        var fields = EquatableArray.Create(
            new FieldViolation(new InputPointer("/email"), "format", null, "must be email"));
        var error = new Error.UnprocessableContent(fields)
        {
            Detail = "Sensitive internal context that must not leak.",
        };
        var r = Result.Fail<T>(error);

        await r.ToHttpResponse(t => t, o => o.WithErrorMapping<Error.UnprocessableContent>(500))
            .ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(500);
        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync(TestContext.Current.CancellationToken);
        body.Should().Contain("An internal error occurred.");
        body.Should().NotContain("Sensitive internal context");
    }

    [Fact]
    public async Task ValidationProblem_with_4xx_status_keeps_detail()
    {
        var ctx = NewContext();
        var fields = EquatableArray.Create(
            new FieldViolation(new InputPointer("/email"), "format", null, "must be email"));
        var error = new Error.UnprocessableContent(fields)
        {
            Detail = "One or more validation errors occurred.",
        };
        var r = Result.Fail<T>(error);

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(422);
        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync(TestContext.Current.CancellationToken);
        body.Should().Contain("One or more validation errors occurred.");
    }

    // ---------------------------------------------------------------------
    // Bundle C / m-9 (#33): JSON Pointer field paths translated to MVC
    // dot+bracket convention on the wire `errors` keys, matching ASP.NET
    // Core's default ValidationProblemDetails shape (so OpenAPI codegen and
    // React form libraries like react-hook-form / Formik can lookup
    // setError(key, ...) directly without a slash→dot translation shim).
    // RFC 6901 escapes (~1, ~0) are decoded so segments containing literal
    // '/' or '~' appear correctly in the wire key.
    // ---------------------------------------------------------------------

    [Theory]
    [InlineData("/email", "email")]                       // single segment: bare (regression guard)
    [InlineData("/customer/email", "customer.email")]     // nested object: dot
    [InlineData("/items/0", "items[0]")]                  // object → array index: brackets
    [InlineData("/items/0/name", "items[0].name")]        // object → array → object
    [InlineData("/items/0/tags/3", "items[0].tags[3]")]   // mixed nesting
    [InlineData("/0/name", "[0].name")]                   // root array index
    [InlineData("/foo~1bar", "foo/bar")]                  // RFC 6901 ~1 unescape
    [InlineData("/foo~0bar", "foo~bar")]                  // RFC 6901 ~0 unescape
    public async Task UnprocessableContent_translates_pointer_to_MVC_dot_bracket(string pointerPath, string expectedKey)
    {
        var ctx = NewContext();
        var fields = EquatableArray.Create(
            new FieldViolation(new InputPointer(pointerPath), "format", null, "must be valid"));
        var r = Result.Fail<T>(new Error.UnprocessableContent(fields));

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.Body.Position = 0;
        using var body = await JsonDocument.ParseAsync(ctx.Response.Body, cancellationToken: TestContext.Current.CancellationToken);
        body.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.TryGetProperty(expectedKey, out _)
            .Should().BeTrue($"expected MVC convention key '{expectedKey}' for pointer '{pointerPath}'");
    }

    [Fact]
    public async Task UnprocessableContent_does_not_emit_JSON_Pointer_slash_form_on_the_wire()
    {
        var ctx = NewContext();
        var fields = EquatableArray.Create(
            new FieldViolation(new InputPointer("/items/0/name"), "format", null, "must be valid"));
        var r = Result.Fail<T>(new Error.UnprocessableContent(fields));

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.Body.Position = 0;
        var raw = await new StreamReader(ctx.Response.Body).ReadToEndAsync(TestContext.Current.CancellationToken);
        raw.Should().Contain("items[0].name");
        raw.Should().NotContain("items/0/name", "JSON Pointer slash form must not appear in the wire `errors` keys");
    }

    [Fact]
    public async Task UnprocessableContent_aggregates_multiple_violations_for_same_pointer_under_one_MVC_key()
    {
        // Regression guard: two FieldViolations with the same pointer must aggregate into ONE
        // wire `errors` key with an array of two messages — not two separate keys.
        var ctx = NewContext();
        var fields = EquatableArray.Create(
            new FieldViolation(new InputPointer("/items/0/name"), "required", null, "is required"),
            new FieldViolation(new InputPointer("/items/0/name"), "format", null, "must be valid"));
        var r = Result.Fail<T>(new Error.UnprocessableContent(fields));

        await r.ToHttpResponse(t => t).ExecuteAsync(ctx);

        ctx.Response.Body.Position = 0;
        using var body = await JsonDocument.ParseAsync(ctx.Response.Body, cancellationToken: TestContext.Current.CancellationToken);
        var errors = body.RootElement.GetProperty("errors");
        var messages = errors.GetProperty("items[0].name").EnumerateArray();
        messages.Should().HaveCount(2);
    }
}