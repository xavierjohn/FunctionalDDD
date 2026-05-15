namespace Trellis.Asp.Tests;

using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Trellis;
using Trellis.Asp;

/// <summary>
/// Tests for <c>WithCacheControl()</c> on <see cref="HttpResponseOptionsBuilder{TDomain}"/>
/// and the non-generic <see cref="HttpResponseOptionsBuilder"/>. Pins the contract:
/// the configured <c>Cache-Control</c> header is emitted on success (200/201/204/206),
/// on conditional-not-modified (304) responses where ETag/Last-Modified short-circuit
/// the body, AND on failure responses when configured via the static-value overload —
/// so `WithCacheControl(CacheControl.NoStore())` actually protects 404s from being
/// cached by intermediate proxies, not just the success-path representation.
/// </summary>
public sealed class WithCacheControlTests
{
    private sealed record Todo(int Id, string Title, string ETag);

    private static DefaultHttpContext NewContext()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRouting();
        services.AddSingleton<IProblemDetailsService, NoopPds>();
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

    // ---------- Builder argument validation ----------

    [Fact]
    public void WithCacheControl_value_throws_on_null()
    {
        var b = new HttpResponseOptionsBuilder<Todo>();
        FluentActions.Invoking(() => b.WithCacheControl((CacheControlHeaderValue)null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithCacheControl_selector_throws_on_null()
    {
        var b = new HttpResponseOptionsBuilder<Todo>();
        FluentActions.Invoking(() => b.WithCacheControl((Func<Todo, CacheControlHeaderValue?>)null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithCacheControl_nongeneric_value_throws_on_null()
    {
        var b = new HttpResponseOptionsBuilder();
        FluentActions.Invoking(() => b.WithCacheControl(null!))
            .Should().Throw<ArgumentNullException>();
    }

    // ---------- Success path: TrellisHttpResult ----------

    [Fact]
    public async Task Static_value_emits_Cache_Control_header_on_200()
    {
        var ctx = NewContext();
        var r = Result.Ok(new Todo(1, "hi", "abc"));

        await r.ToHttpResponse(t => t,
            o => o.WithCacheControl(new CacheControlHeaderValue { Private = true, MaxAge = TimeSpan.FromMinutes(5) }))
            .ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(200);
        var cc = ctx.Response.Headers.CacheControl.ToString();
        cc.Should().Contain("private").And.Contain("max-age=300",
            "WithCacheControl(value) emits both directives on 200 responses");
    }

    [Fact]
    public async Task Selector_receives_domain_value_and_emits_header()
    {
        var ctx = NewContext();
        var r = Result.Ok(new Todo(42, "ttl", "abc"));

        await r.ToHttpResponse(t => t,
            o => o.WithCacheControl(t => new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromSeconds(t.Id), // proves the selector saw the domain
            }))
            .ExecuteAsync(ctx);

        ctx.Response.Headers.CacheControl.ToString().Should().Be("public, max-age=42");
    }

    [Fact]
    public async Task Selector_returning_null_omits_header()
    {
        var ctx = NewContext();
        var r = Result.Ok(new Todo(1, "x", "abc"));

        await r.ToHttpResponse(t => t,
            o => o.WithCacheControl(_ => null))
            .ExecuteAsync(ctx);

        ctx.Response.Headers.CacheControl.ToString().Should().BeEmpty(
            "a null selector return means 'this response has no per-domain Cache-Control'");
    }

    [Fact]
    public async Task Selector_overrides_static_value_when_both_configured()
    {
        var ctx = NewContext();
        var r = Result.Ok(new Todo(7, "x", "abc"));

        await r.ToHttpResponse(t => t, o => o
            .WithCacheControl(new CacheControlHeaderValue { Private = true, MaxAge = TimeSpan.FromMinutes(1) })
            .WithCacheControl(t => new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromSeconds(t.Id) }))
            .ExecuteAsync(ctx);

        ctx.Response.Headers.CacheControl.ToString().Should().Be("public, max-age=7",
            "the per-domain selector is the more specific directive and wins");
    }

    [Fact]
    public async Task Static_value_composes_with_ETag_and_Vary_without_interference()
    {
        var ctx = NewContext();
        var r = Result.Ok(new Todo(1, "hi", "abc"));

        await r.ToHttpResponse(t => t, o => o
                .WithETag(t => t.ETag)
                .Vary("Accept-Encoding")
                .WithCacheControl(new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromMinutes(10) }))
            .ExecuteAsync(ctx);

        ctx.Response.Headers.ETag.ToString().Should().Be("\"abc\"");
        ctx.Response.Headers.Vary.ToString().Should().Contain("Accept-Encoding");
        ctx.Response.Headers.CacheControl.ToString().Should().Be("public, max-age=600");
    }

    [Fact]
    public async Task Static_value_preserved_on_304_NotModified()
    {
        // RFC 9111 §4.3.4: 304 responses carry cache-relevant headers. Trellis must keep
        // Cache-Control on the 304 short-circuit so revalidation responses do not weaken
        // the cache policy declared on the 200.
        var ctx = NewContext();
        ctx.Request.Method = HttpMethods.Get;
        ctx.Request.Headers.IfNoneMatch = "\"abc\"";
        var r = Result.Ok(new Todo(1, "hi", "abc"));

        await r.ToHttpResponse(t => t, o => o
                .WithETag(t => t.ETag)
                .EvaluatePreconditions()
                .WithCacheControl(new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromMinutes(5) }))
            .ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(304);
        ctx.Response.Headers.CacheControl.ToString().Should().Be("public, max-age=300");
    }

    // ---------- Failure path ----------

    [Fact]
    public async Task Static_value_emits_Cache_Control_on_failure_response()
    {
        // The whole point of `WithCacheControl(CacheControl.NoStore())` on a sensitive
        // endpoint is that 404/403 responses do NOT leak through intermediate caches.
        // The static-value overload therefore applies to failures as well as successes.
        var ctx = NewContext();
        var r = Result.Fail<Todo>(new Error.NotFound(new ResourceRef("Todo", "1")));

        await r.ToHttpResponse(t => t,
            o => o.WithCacheControl(CacheControl.NoStore()))
            .ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(404);
        ctx.Response.Headers.CacheControl.ToString().Should().Be("no-store");
    }

    [Fact]
    public async Task Selector_only_does_not_emit_Cache_Control_on_failure_response()
    {
        // The selector has no domain value on the failure path. Only the static-value
        // overload propagates to failures; a selector-only configuration is success-only.
        var ctx = NewContext();
        var r = Result.Fail<Todo>(new Error.NotFound(new ResourceRef("Todo", "1")));

        await r.ToHttpResponse(t => t,
            o => o.WithCacheControl(_ => new CacheControlHeaderValue { NoStore = true }))
            .ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(404);
        ctx.Response.Headers.CacheControl.ToString().Should().BeEmpty(
            "selector-based Cache-Control is success-only because failures carry no domain value");
    }

    // ---------- WriteOutcome path ----------

    [Fact]
    public async Task Static_value_emits_on_Created_WriteOutcome()
    {
        var ctx = NewContext();
        var outcome = new WriteOutcome<Todo>.Created(new Todo(1, "hi", "abc"), "/todos/1");
        var r = Result.Ok<WriteOutcome<Todo>>(outcome);

        await r.ToHttpResponse<Todo>(
            o => o.WithCacheControl(CacheControl.Private(TimeSpan.FromMinutes(5))))
            .ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(201);
        var cc = ctx.Response.Headers.CacheControl.ToString();
        cc.Should().Contain("private").And.Contain("max-age=300");
    }

    [Fact]
    public async Task Selector_emits_on_Updated_WriteOutcome()
    {
        var ctx = NewContext();
        var outcome = new WriteOutcome<Todo>.Updated(new Todo(9, "hi", "abc"));
        var r = Result.Ok<WriteOutcome<Todo>>(outcome);

        await r.ToHttpResponse<Todo>(
            o => o.WithCacheControl(t => new CacheControlHeaderValue
            {
                Private = true,
                MaxAge = TimeSpan.FromSeconds(t.Id * 10),
            }))
            .ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(200);
        var cc = ctx.Response.Headers.CacheControl.ToString();
        cc.Should().Contain("private").And.Contain("max-age=90");
    }

    // ---------- Page<T> path ----------

    [Fact]
    public async Task Static_value_emits_on_paged_success()
    {
        var ctx = NewContext();
        var page = new Page<Todo>(
            Items: [new Todo(1, "a", "e1"), new Todo(2, "b", "e2")],
            Next: null,
            Previous: null,
            RequestedLimit: 50,
            AppliedLimit: 50);
        var r = Result.Ok(page);

        await r.ToHttpResponse(
                (_, _) => "/todos?cursor=next",
                t => t,
                o => o.WithCacheControl(CacheControl.Public(TimeSpan.FromMinutes(1))))
            .ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(200);
        ctx.Response.Headers.CacheControl.ToString().Should().Be("public, max-age=60");
    }

    // ---------- Non-generic builder ----------

    [Fact]
    public async Task Nongeneric_builder_emits_static_value_on_204()
    {
        var ctx = NewContext();
        var r = Result.Ok();

        await r.ToHttpResponse(
                o => o.WithCacheControl(CacheControl.NoStore()))
            .ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(204);
        ctx.Response.Headers.CacheControl.ToString().Should().Be("no-store");
    }

    [Fact]
    public async Task Nongeneric_builder_emits_static_value_on_failure()
    {
        var ctx = NewContext();
        var r = Result.Fail(new Error.Forbidden("documents.edit"));

        await r.ToHttpResponse(
                o => o.WithCacheControl(CacheControl.NoStore()))
            .ExecuteAsync(ctx);

        ctx.Response.StatusCode.Should().Be(403);
        ctx.Response.Headers.CacheControl.ToString().Should().Be("no-store");
    }

    // ---------- CacheControl preset helpers ----------

    [Fact]
    public void CacheControl_presets_return_fresh_instances()
    {
        // The presets must return a NEW CacheControlHeaderValue per call so a consumer
        // mutating one preset (e.g. setting MaxAge after the fact) cannot corrupt later
        // calls or other consumers sharing the same preset.
        var a = CacheControl.NoStore();
        var b = CacheControl.NoStore();
        a.Should().NotBeSameAs(b);

        a.MaxAge = TimeSpan.FromHours(1);
        b.MaxAge.Should().BeNull("mutating one preset instance must not leak into the next call");
    }

    [Fact]
    public void CacheControl_NoStore_formats_as_no_store() =>
        CacheControl.NoStore().ToString().Should().Be("no-store");

    [Fact]
    public void CacheControl_NoCache_formats_as_no_cache() =>
        CacheControl.NoCache().ToString().Should().Be("no-cache");

    [Fact]
    public void CacheControl_Public_formats_as_public_max_age() =>
        CacheControl.Public(TimeSpan.FromMinutes(5)).ToString().Should().Be("public, max-age=300");

    [Fact]
    public void CacheControl_Private_formats_as_private_max_age()
    {
        var s = CacheControl.Private(TimeSpan.FromMinutes(5)).ToString();
        s.Should().Contain("private").And.Contain("max-age=300");
    }

    [Fact]
    public void CacheControl_Immutable_includes_immutable_directive()
    {
        var s = CacheControl.Immutable(TimeSpan.FromHours(1)).ToString();
        s.Should().Contain("public").And.Contain("max-age=3600").And.Contain("immutable");
    }

    [Fact]
    public void CacheControl_Immutable_returns_fresh_instances_each_call()
    {
        // Immutable() builds via Extensions; the BCL collection is mutable, so callers
        // mutating one returned value must not affect a subsequent call.
        var a = CacheControl.Immutable(TimeSpan.FromHours(1));
        a.Extensions.Clear();
        var b = CacheControl.Immutable(TimeSpan.FromHours(1));
        b.Extensions.Select(e => e.Name).Should().Contain("immutable");
    }
}
