namespace Trellis.Asp.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Trellis;
using Trellis.Asp;
using Trellis.Asp.Authorization;
using Trellis.Authorization;

/// <summary>
/// Tests for <c>VaryForActor()</c> on <see cref="HttpResponseOptionsBuilder{TDomain}"/>:
/// the new helper that emits the request header(s) contributing to actor identity into
/// the response <c>Vary</c> header, so intermediate HTTP caches don't serve actor A's
/// response to actor B. Pins the fail-closed behavior — the builder throws rather than
/// silently emit an incorrect or incomplete <c>Vary</c> header when the registered
/// provider does not implement <see cref="IProvideActorVaryHeaders"/>.
/// </summary>
public sealed class VaryForActorTests
{
    private sealed record Thing(int Id, string Name);

    private static DefaultHttpContext NewContext(IActorProvider? provider)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IProblemDetailsService, NoopPds>();
        if (provider is not null)
            services.AddSingleton(provider);
        var ctx = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }

    private sealed class NoopPds : IProblemDetailsService
    {
        public ValueTask WriteAsync(ProblemDetailsContext c) => ValueTask.CompletedTask;
#pragma warning disable CA1822
        public bool TryWrite(ProblemDetailsContext c) => false;
#pragma warning restore CA1822
    }

    private sealed class TestActorProvider(IReadOnlyCollection<string>? vary)
        : IActorProvider, IProvideActorVaryHeaders
    {
        private static readonly System.Collections.Frozen.FrozenSet<string> EmptyPermissions =
            System.Collections.Frozen.FrozenSet<string>.Empty;

        public IReadOnlyCollection<string> VaryByHeaders { get; } = vary ?? [];
        public Task<Maybe<Actor>> GetCurrentActorAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Maybe.From(Actor.Create("test-actor", EmptyPermissions)));
    }

    private sealed class ProviderWithoutVaryCapability : IActorProvider
    {
        private static readonly System.Collections.Frozen.FrozenSet<string> EmptyPermissions =
            System.Collections.Frozen.FrozenSet<string>.Empty;

        public Task<Maybe<Actor>> GetCurrentActorAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Maybe.From(Actor.Create("test-actor", EmptyPermissions)));
    }

    [Fact]
    public async Task VaryForActor_emits_Authorization_for_bearer_style_provider()
    {
        // Bearer-style provider declares Authorization as the request header that contributes
        // to actor identity. VaryForActor() emits exactly that header.
        var ctx = NewContext(new TestActorProvider(["Authorization"]));
        var r = Result.Ok(new Thing(1, "x"));

        await r.ToHttpResponse(t => t, o => o.VaryForActor()).ExecuteAsync(ctx);

        var vary = string.Join("|", ctx.Response.Headers.Vary.ToArray()!);
        vary.Should().Contain("Authorization",
            "VaryForActor() must emit each header named by the provider's VaryByHeaders");
    }

    [Fact]
    public async Task VaryForActor_emits_test_header_for_DevelopmentActorProvider_style_provider()
    {
        // A non-bearer provider (e.g. DevelopmentActorProvider) varies by a different header.
        // VaryForActor() emits the provider's declared header verbatim — not a hard-coded
        // "Authorization" default. This is the defense against cache-poisoning for non-bearer
        // auth schemes.
        var ctx = NewContext(new TestActorProvider(["X-Test-Actor"]));
        var r = Result.Ok(new Thing(1, "x"));

        await r.ToHttpResponse(t => t, o => o.VaryForActor()).ExecuteAsync(ctx);

        var vary = string.Join("|", ctx.Response.Headers.Vary.ToArray()!);
        vary.Should().Contain("X-Test-Actor");
    }

    [Fact]
    public async Task VaryForActor_emits_multiple_headers_when_provider_declares_them()
    {
        // A custom provider may legitimately vary by multiple headers (e.g. Authorization +
        // X-Forwarded-Tenant for tenant-scoped caching). VaryForActor() emits every header
        // the provider declares.
        var ctx = NewContext(new TestActorProvider(["Authorization", "X-Forwarded-Tenant"]));
        var r = Result.Ok(new Thing(1, "x"));

        await r.ToHttpResponse(t => t, o => o.VaryForActor()).ExecuteAsync(ctx);

        var vary = string.Join("|", ctx.Response.Headers.Vary.ToArray()!);
        vary.Should().Contain("Authorization").And.Contain("X-Forwarded-Tenant");
    }

    [Fact]
    public async Task VaryForActor_deduplicates_against_existing_Vary_entries()
    {
        // Composes with explicit .Vary(...) calls: when a header is already in Vary
        // (whether from earlier middleware or from .Vary("Authorization") on the same chain),
        // VaryForActor() does not append a duplicate. Same idempotence semantics as the
        // existing AppendVaryUnique helper.
        var ctx = NewContext(new TestActorProvider(["Authorization"]));
        ctx.Response.Headers["Vary"] = "Authorization";
        var r = Result.Ok(new Thing(1, "x"));

        await r.ToHttpResponse(t => t, o => o.VaryForActor()).ExecuteAsync(ctx);

        // Combine all values and split by comma to count occurrences, since ASP.NET Core may
        // store Vary entries as one combined string or multiple header values.
        var combined = string.Join(",", ctx.Response.Headers.Vary.ToArray()!);
        var occurrences = combined
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Count(p => string.Equals(p, "Authorization", StringComparison.OrdinalIgnoreCase));
        occurrences.Should().Be(1, "VaryForActor() must dedupe against existing Vary entries (case-insensitive)");
    }

    [Fact]
    public async Task VaryForActor_throws_when_no_IActorProvider_registered()
    {
        // Fail-closed: calling VaryForActor() on a request that has no IActorProvider
        // registered is a configuration error. Silently emitting nothing would still let
        // an intermediate cache pollute responses across actors; throwing surfaces the
        // bug at the first request rather than via a security incident.
        var ctx = NewContext(provider: null);
        var r = Result.Ok(new Thing(1, "x"));

        var act = async () => await r.ToHttpResponse(t => t, o => o.VaryForActor()).ExecuteAsync(ctx);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*IActorProvider*");
    }

    [Fact]
    public async Task VaryForActor_throws_when_provider_does_not_implement_IProvideActorVaryHeaders()
    {
        // Fail-closed: a custom IActorProvider implementation that has not opted into
        // IProvideActorVaryHeaders cannot tell us which headers to emit. Silently falling
        // back to "Authorization" would be unsafe (cookie/mTLS/forwarded-header auth would
        // still cache-poison). The exception names the offending provider type and points
        // to the remedy.
        var ctx = NewContext(new ProviderWithoutVaryCapability());
        var r = Result.Ok(new Thing(1, "x"));

        var act = async () => await r.ToHttpResponse(t => t, o => o.VaryForActor()).ExecuteAsync(ctx);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*IProvideActorVaryHeaders*");
    }

    [Fact]
    public async Task VaryForActor_throws_when_provider_declares_empty_VaryByHeaders()
    {
        // A provider that explicitly declares an empty VaryByHeaders set is saying "I derive
        // actor identity from request data that can't be cleanly named by an HTTP header"
        // (mTLS, IP-based, etc.). The provider has opted into IProvideActorVaryHeaders but
        // is signaling that VaryForActor() is the wrong tool for this endpoint — endpoints
        // backed by that provider should use Cache-Control: private, no-store instead.
        var ctx = NewContext(new TestActorProvider(vary: []));
        var r = Result.Ok(new Thing(1, "x"));

        var act = async () => await r.ToHttpResponse(t => t, o => o.VaryForActor()).ExecuteAsync(ctx);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*empty*");
    }

    [Fact]
    public async Task VaryForActor_composes_with_explicit_Vary_call()
    {
        // Mixing .VaryForActor() and .Vary("X") is valid: the response carries every header
        // either path requested. Common shape for endpoints that vary by both actor and
        // content negotiation (Accept, Accept-Language).
        var ctx = NewContext(new TestActorProvider(["Authorization"]));
        var r = Result.Ok(new Thing(1, "x"));

        await r.ToHttpResponse(t => t, o => o.Vary("Accept-Language").VaryForActor())
            .ExecuteAsync(ctx);

        var vary = string.Join("|", ctx.Response.Headers.Vary.ToArray()!);
        vary.Should().Contain("Authorization").And.Contain("Accept-Language");
    }

    [Fact]
    public async Task VaryForActor_applies_to_WriteOutcome_path()
    {
        // The success path for Result<WriteOutcome<T>> goes through TrellisWriteOutcomeResult
        // rather than TrellisHttpResult. VaryForActor() must plug into both code paths,
        // otherwise endpoints returning WriteOutcome (Created/Updated/Accepted) would
        // silently miss the Vary header and cache-poison across actors.
        var ctx = NewContext(new TestActorProvider(["Authorization"]));
        var outcome = new WriteOutcome<Thing>.Updated(new Thing(1, "x"));
        var r = Result.Ok<WriteOutcome<Thing>>(outcome);

        await r.ToHttpResponse<Thing>(o => o.VaryForActor()).ExecuteAsync(ctx);

        var vary = string.Join("|", ctx.Response.Headers.Vary.ToArray()!);
        vary.Should().Contain("Authorization", "VaryForActor() must apply to the WriteOutcome success path too");
    }

    [Fact]
    public void ClaimsActorProvider_default_VaryByHeaders_is_Authorization()
    {
        // Pins the JWT-bearer default that the bundled ClaimsActorProvider /
        // EntraActorProvider rely on. If this changes, callers using AddClaimsActorProvider /
        // AddEntraActorProvider + VaryForActor() get a different shape on the wire — that
        // is a breaking change that should require a code review and CHANGELOG note.
        var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var options = Microsoft.Extensions.Options.Options.Create(new ClaimsActorOptions());
        var provider = new ClaimsActorProvider(httpContextAccessor, options);

        provider.Should().BeAssignableTo<IProvideActorVaryHeaders>();
        ((IProvideActorVaryHeaders)provider).VaryByHeaders.Should().BeEquivalentTo(["Authorization"]);
    }

    [Fact]
    public void CachingActorProvider_delegates_VaryByHeaders_to_inner_provider()
    {
        // CachingActorProvider wraps another provider. Its VaryByHeaders must surface the
        // inner provider's declaration, otherwise wrapping a bearer-style provider in
        // AddCachingActorProvider<T>() would silently strip the Vary capability.
        var inner = new TestActorProvider(["X-Test-Actor"]);
        var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var cache = new CachingActorProvider(inner, httpContextAccessor);

        ((IProvideActorVaryHeaders)cache).VaryByHeaders.Should().BeEquivalentTo(["X-Test-Actor"]);
    }

    [Fact]
    public void CachingActorProvider_returns_empty_VaryByHeaders_when_inner_does_not_implement_interface()
    {
        // Inner provider has not opted into IProvideActorVaryHeaders. CachingActorProvider
        // cannot guess on its behalf — surface an empty collection so VaryForActor() throws
        // fail-closed with a message that points at the wrapped (custom) provider as the
        // remediation site.
        var inner = new ProviderWithoutVaryCapability();
        var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var cache = new CachingActorProvider(inner, httpContextAccessor);

        ((IProvideActorVaryHeaders)cache).VaryByHeaders.Should().BeEmpty();
    }
}
