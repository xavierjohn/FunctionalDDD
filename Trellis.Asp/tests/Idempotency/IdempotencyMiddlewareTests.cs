namespace Trellis.Asp.Tests.Idempotency;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Trellis.Asp.Idempotency;

/// <summary>
/// Integration tests for the full <see cref="IdempotencyMiddleware"/> pipeline driven through
/// a <see cref="TestServer"/>. Pins the IETF Idempotency-Key contract end-to-end: opt-in via
/// <c>IdempotentAttribute</c>, replay verbatim, in-flight 409, body-mismatch
/// <c>idempotency.key_reused_with_different_body</c>, and request-body 413.
/// </summary>
public sealed class IdempotencyMiddlewareTests
{
    private const string KeyHeader = "Idempotency-Key";

    private static async Task<IHost> BuildHost(
        Action<IEndpointRouteBuilder>? configureEndpoints = null,
        Action<IdempotencyOptions>? configureOptions = null)
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(web => web
                .UseTestServer()
                .ConfigureServices(s =>
                {
                    s.AddRouting();
                    s.AddTrellisIdempotency(configureOptions);
                    s.AddInMemoryIdempotencyStore();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseTrellisIdempotency();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapPost("/idempotent", async ctx =>
                        {
                            ctx.Response.StatusCode = 201;
                            ctx.Response.ContentType = "application/json";
                            await ctx.Response.WriteAsync("{\"order\":\"created\"}", ctx.RequestAborted);
                        }).WithMetadata(new IdempotentAttribute());

                        endpoints.MapPost("/passthrough", async ctx =>
                        {
                            ctx.Response.StatusCode = 200;
                            await ctx.Response.WriteAsync("ok", ctx.RequestAborted);
                        });

                        endpoints.MapGet("/idempotent-get", async ctx =>
                        {
                            ctx.Response.StatusCode = 200;
                            await ctx.Response.WriteAsync("get-ok", ctx.RequestAborted);
                        }).WithMetadata(new IdempotentAttribute());

                        configureEndpoints?.Invoke(endpoints);
                    });
                }));

        var host = await builder.StartAsync();
        return host;
    }

    private static StringContent JsonBody(string json) =>
        new(json, Encoding.UTF8, "application/json");

    [Fact]
    public async Task Endpoint_without_attribute_is_pass_through()
    {
        using var host = await BuildHost();
        var client = host.GetTestClient();

        var content = JsonBody("{\"x\":1}");
        content.Headers.Add(KeyHeader, "abc");
        var response = await client.PostAsync("/passthrough", content, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains("Idempotent-Replayed").Should().BeFalse();
    }

    [Fact]
    public async Task Method_outside_options_set_is_pass_through()
    {
        using var host = await BuildHost();
        var client = host.GetTestClient();

        var req = new HttpRequestMessage(HttpMethod.Get, "/idempotent-get");
        req.Headers.Add(KeyHeader, "k1");
        var response = await client.SendAsync(req, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Be("get-ok");
    }

    [Fact]
    public async Task Missing_header_returns_400_when_RequireKey_default_true()
    {
        using var host = await BuildHost();
        var client = host.GetTestClient();

        var response = await client.PostAsync("/idempotent", JsonBody("{\"x\":1}"), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Contain("idempotency.key_required");
    }

    [Fact]
    public async Task Missing_header_is_pass_through_when_RequireKey_disabled()
    {
        using var host = await BuildHost(configureOptions: o => o.RequireKeyOnOptedInEndpoints = false);
        var client = host.GetTestClient();

        var response = await client.PostAsync("/idempotent", JsonBody("{\"x\":1}"), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Contains("Idempotent-Replayed").Should().BeFalse();
    }

    [Fact]
    public async Task First_request_executes_and_second_request_replays_verbatim()
    {
        using var host = await BuildHost();
        var client = host.GetTestClient();

        var key = Guid.NewGuid().ToString();

        var first = JsonBody("{\"x\":1}");
        first.Headers.Add(KeyHeader, key);
        var firstResp = await client.PostAsync("/idempotent", first, TestContext.Current.CancellationToken);

        firstResp.StatusCode.Should().Be(HttpStatusCode.Created);
        firstResp.Headers.Contains("Idempotent-Replayed").Should().BeFalse();
        (await firstResp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Be("{\"order\":\"created\"}");

        var second = JsonBody("{\"x\":1}");
        second.Headers.Add(KeyHeader, key);
        var secondResp = await client.PostAsync("/idempotent", second, TestContext.Current.CancellationToken);

        secondResp.StatusCode.Should().Be(HttpStatusCode.Created);
        secondResp.Headers.GetValues("Idempotent-Replayed").Should().Contain("true");
        (await secondResp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Be("{\"order\":\"created\"}");
    }

    [Fact]
    public async Task Reused_key_with_different_body_returns_mismatch_status()
    {
        using var host = await BuildHost();
        var client = host.GetTestClient();

        var key = Guid.NewGuid().ToString();

        var first = JsonBody("{\"x\":1}");
        first.Headers.Add(KeyHeader, key);
        await client.PostAsync("/idempotent", first, TestContext.Current.CancellationToken);

        var second = JsonBody("{\"x\":2}");
        second.Headers.Add(KeyHeader, key);
        var resp = await client.PostAsync("/idempotent", second, TestContext.Current.CancellationToken);

        resp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Contain("idempotency.key_reused_with_different_body");
    }

    [Fact]
    public async Task Invalid_key_returns_400_problem_details()
    {
        using var host = await BuildHost();
        var client = host.GetTestClient();

        var content = JsonBody("{}");
        content.Headers.TryAddWithoutValidation(KeyHeader, "bad key with space");
        var resp = await client.PostAsync("/idempotent", content, TestContext.Current.CancellationToken);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Contain("idempotency.key_invalid");
    }

    [Fact]
    public async Task Key_too_long_returns_400()
    {
        using var host = await BuildHost(configureOptions: o => o.MaxKeyLength = 10);
        var client = host.GetTestClient();

        var content = JsonBody("{}");
        content.Headers.Add(KeyHeader, new string('a', 20));
        var resp = await client.PostAsync("/idempotent", content, TestContext.Current.CancellationToken);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Contain("idempotency.key_too_long");
    }

    [Fact]
    public async Task Body_exceeding_max_returns_413()
    {
        using var host = await BuildHost(configureOptions: o => o.MaxRequestBodyBytes = 16);
        var client = host.GetTestClient();

        var content = JsonBody(new string('z', 100));
        content.Headers.Add(KeyHeader, "k1");
        var resp = await client.PostAsync("/idempotent", content, TestContext.Current.CancellationToken);

        resp.StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Contain("idempotency.request_body_too_large");
    }

    [Fact]
    public async Task UseTrellisIdempotency_throws_when_no_store_registered()
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(web => web
                .UseTestServer()
                .ConfigureServices(s =>
                {
                    s.AddRouting();
                    s.AddTrellisIdempotency();
                })
                .Configure(app =>
                {
                    var act = () => app.UseTrellisIdempotency();
                    act.Should().Throw<InvalidOperationException>()
                        .WithMessage("*IIdempotencyStore*");
                }));

        using var host = await builder.StartAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task UseTrellisIdempotency_throws_when_AddTrellisIdempotency_not_called()
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(web => web
                .UseTestServer()
                .ConfigureServices(s => s.AddRouting())
                .Configure(app =>
                {
                    var act = () => app.UseTrellisIdempotency();
                    act.Should().Throw<InvalidOperationException>()
                        .WithMessage("*AddTrellisIdempotency*");
                }));

        using var host = await builder.StartAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void AddTrellisIdempotency_registers_DefaultIdempotencyScopeResolver_by_default()
    {
        var services = new ServiceCollection();
        services.AddTrellisIdempotency();
        using var sp = services.BuildServiceProvider();

        var resolver = sp.GetRequiredService<IIdempotencyScopeResolver>();

        resolver.Should().BeOfType<DefaultIdempotencyScopeResolver>();
    }

    [Fact]
    public async Task SetCookie_header_is_filtered_from_snapshot_by_default()
    {
        using var host = await BuildHost(configureEndpoints: endpoints =>
            endpoints.MapPost("/with-cookie", async ctx =>
            {
                ctx.Response.StatusCode = 201;
                ctx.Response.Headers["Set-Cookie"] = "session=abc; Path=/";
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync("{\"ok\":true}", ctx.RequestAborted);
            }).WithMetadata(new IdempotentAttribute()));
        var client = host.GetTestClient();
        var key = Guid.NewGuid().ToString();

        var first = JsonBody("{}");
        first.Headers.Add(KeyHeader, key);
        var firstResp = await client.PostAsync("/with-cookie", first, TestContext.Current.CancellationToken);
        firstResp.Headers.Contains("Set-Cookie").Should().BeTrue("the live response should still carry Set-Cookie");

        var second = JsonBody("{}");
        second.Headers.Add(KeyHeader, key);
        var secondResp = await client.PostAsync("/with-cookie", second, TestContext.Current.CancellationToken);

        secondResp.Headers.GetValues("Idempotent-Replayed").Should().Contain("true");
        secondResp.Headers.Contains("Set-Cookie").Should().BeFalse("replayed response must not re-issue cookies");
    }

    [Fact]
    public async Task SetCookie_header_is_included_in_snapshot_when_option_enabled()
    {
        using var host = await BuildHost(
            configureEndpoints: endpoints =>
                endpoints.MapPost("/with-cookie", async ctx =>
                {
                    ctx.Response.StatusCode = 201;
                    ctx.Response.Headers["Set-Cookie"] = "session=abc; Path=/";
                    ctx.Response.ContentType = "application/json";
                    await ctx.Response.WriteAsync("{\"ok\":true}", ctx.RequestAborted);
                }).WithMetadata(new IdempotentAttribute()),
            configureOptions: o => o.IncludeSetCookieInSnapshot = true);
        var client = host.GetTestClient();
        var key = Guid.NewGuid().ToString();

        var first = JsonBody("{}");
        first.Headers.Add(KeyHeader, key);
        await client.PostAsync("/with-cookie", first, TestContext.Current.CancellationToken);

        var second = JsonBody("{}");
        second.Headers.Add(KeyHeader, key);
        var secondResp = await client.PostAsync("/with-cookie", second, TestContext.Current.CancellationToken);

        secondResp.Headers.GetValues("Idempotent-Replayed").Should().Contain("true");
        secondResp.Headers.Contains("Set-Cookie").Should().BeTrue();
    }

    [Fact]
    public async Task Bodyless_204_response_is_snapshotted_and_replayed()
    {
        using var host = await BuildHost(configureEndpoints: endpoints =>
            endpoints.MapPost("/no-content", ctx =>
            {
                ctx.Response.StatusCode = 204;
                return Task.CompletedTask;
            }).WithMetadata(new IdempotentAttribute()));
        var client = host.GetTestClient();
        var key = Guid.NewGuid().ToString();

        var first = JsonBody("{}");
        first.Headers.Add(KeyHeader, key);
        var firstResp = await client.PostAsync("/no-content", first, TestContext.Current.CancellationToken);
        firstResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        firstResp.Headers.Contains("Idempotent-Replayed").Should().BeFalse();

        var second = JsonBody("{}");
        second.Headers.Add(KeyHeader, key);
        var secondResp = await client.PostAsync("/no-content", second, TestContext.Current.CancellationToken);

        secondResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResp.Headers.GetValues("Idempotent-Replayed").Should().Contain("true");
    }

    [Fact]
    public async Task Bodyless_201_with_Location_only_is_snapshotted_and_replayed()
    {
        using var host = await BuildHost(configureEndpoints: endpoints =>
            endpoints.MapPost("/created-headers-only", ctx =>
            {
                ctx.Response.StatusCode = 201;
                ctx.Response.Headers["Location"] = "/orders/42";
                return Task.CompletedTask;
            }).WithMetadata(new IdempotentAttribute()));
        var client = host.GetTestClient();
        var key = Guid.NewGuid().ToString();

        var first = JsonBody("{}");
        first.Headers.Add(KeyHeader, key);
        var firstResp = await client.PostAsync("/created-headers-only", first, TestContext.Current.CancellationToken);
        firstResp.StatusCode.Should().Be(HttpStatusCode.Created);
        firstResp.Headers.Location?.ToString().Should().Be("/orders/42");

        var second = JsonBody("{}");
        second.Headers.Add(KeyHeader, key);
        var secondResp = await client.PostAsync("/created-headers-only", second, TestContext.Current.CancellationToken);

        secondResp.StatusCode.Should().Be(HttpStatusCode.Created);
        secondResp.Headers.Location?.ToString().Should().Be("/orders/42");
        secondResp.Headers.GetValues("Idempotent-Replayed").Should().Contain("true");
    }

    [Fact]
    public async Task ReplayHeaderName_option_is_honoured()
    {
        using var host = await BuildHost(configureOptions: o => o.ReplayHeaderName = "X-Trellis-Replayed");
        var client = host.GetTestClient();
        var key = Guid.NewGuid().ToString();

        var first = JsonBody("{\"x\":1}");
        first.Headers.Add(KeyHeader, key);
        await client.PostAsync("/idempotent", first, TestContext.Current.CancellationToken);

        var second = JsonBody("{\"x\":1}");
        second.Headers.Add(KeyHeader, key);
        var secondResp = await client.PostAsync("/idempotent", second, TestContext.Current.CancellationToken);

        secondResp.Headers.GetValues("X-Trellis-Replayed").Should().Contain("true");
        secondResp.Headers.Contains("Idempotent-Replayed").Should().BeFalse();
    }

    [Fact]
    public async Task Server_error_response_is_abandoned_and_retry_re_executes_handler()
    {
        var executions = 0;
        using var host = await BuildHost(configureEndpoints: endpoints =>
            endpoints.MapPost("/server-error", async ctx =>
            {
                Interlocked.Increment(ref executions);
                ctx.Response.StatusCode = 503;
                ctx.Response.ContentType = "application/problem+json";
                await ctx.Response.WriteAsync("{\"detail\":\"transient\"}", ctx.RequestAborted);
            }).WithMetadata(new IdempotentAttribute()));
        var client = host.GetTestClient();
        var key = Guid.NewGuid().ToString();

        var first = JsonBody("{\"x\":1}");
        first.Headers.Add(KeyHeader, key);
        var firstResp = await client.PostAsync("/server-error", first, TestContext.Current.CancellationToken);
        firstResp.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);

        var second = JsonBody("{\"x\":1}");
        second.Headers.Add(KeyHeader, key);
        var secondResp = await client.PostAsync("/server-error", second, TestContext.Current.CancellationToken);

        secondResp.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable,
            "transient 5xx responses must not be cached");
        secondResp.Headers.Contains("Idempotent-Replayed").Should().BeFalse(
            "the retry must hit the handler again, not replay the original 5xx");
        executions.Should().Be(2, "the handler must execute on each retry of a 5xx outcome");
    }

    [Fact]
    public async Task Bodyless_server_error_is_abandoned_and_retry_re_executes_handler()
    {
        var executions = 0;
        using var host = await BuildHost(configureEndpoints: endpoints =>
            endpoints.MapPost("/server-error-empty", ctx =>
            {
                Interlocked.Increment(ref executions);
                ctx.Response.StatusCode = 500;
                return Task.CompletedTask;
            }).WithMetadata(new IdempotentAttribute()));
        var client = host.GetTestClient();
        var key = Guid.NewGuid().ToString();

        var first = JsonBody("{}");
        first.Headers.Add(KeyHeader, key);
        var firstResp = await client.PostAsync("/server-error-empty", first, TestContext.Current.CancellationToken);
        firstResp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var second = JsonBody("{}");
        second.Headers.Add(KeyHeader, key);
        var secondResp = await client.PostAsync("/server-error-empty", second, TestContext.Current.CancellationToken);

        secondResp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        secondResp.Headers.Contains("Idempotent-Replayed").Should().BeFalse(
            "5xx detection must work even when the handler never flushed the response body");
        executions.Should().Be(2);
    }

    [Fact]
    public async Task Client_error_response_is_cached_and_replayed()
    {
        var executions = 0;
        using var host = await BuildHost(configureEndpoints: endpoints =>
            endpoints.MapPost("/client-error", async ctx =>
            {
                Interlocked.Increment(ref executions);
                ctx.Response.StatusCode = 422;
                ctx.Response.ContentType = "application/problem+json";
                await ctx.Response.WriteAsync("{\"detail\":\"invalid input\"}", ctx.RequestAborted);
            }).WithMetadata(new IdempotentAttribute()));
        var client = host.GetTestClient();
        var key = Guid.NewGuid().ToString();

        var first = JsonBody("{\"x\":1}");
        first.Headers.Add(KeyHeader, key);
        var firstResp = await client.PostAsync("/client-error", first, TestContext.Current.CancellationToken);
        firstResp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var second = JsonBody("{\"x\":1}");
        second.Headers.Add(KeyHeader, key);
        var secondResp = await client.PostAsync("/client-error", second, TestContext.Current.CancellationToken);

        secondResp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        secondResp.Headers.GetValues("Idempotent-Replayed").Should().Contain("true",
            "deterministic 4xx outcomes are still cached because retries will produce the same answer");
        executions.Should().Be(1, "the handler must run only once when a 4xx outcome was cached");
    }

    [Fact]
    public async Task Response_with_trailers_is_abandoned_and_retry_re_executes_handler()
    {
        var executions = 0;
        using var host = await BuildHost(configureEndpoints: endpoints =>
            endpoints.MapPost("/with-trailers", async ctx =>
            {
                Interlocked.Increment(ref executions);
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "application/json";
                var trailers = ctx.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseTrailersFeature>();
                if (trailers is not null)
                {
                    trailers.Trailers["X-Trace"] = "abc";
                }

                await ctx.Response.WriteAsync("{\"ok\":true}", ctx.RequestAborted);
            }).WithMetadata(new IdempotentAttribute()));
        var client = host.GetTestClient();
        var key = Guid.NewGuid().ToString();

        var first = JsonBody("{}");
        first.Headers.Add(KeyHeader, key);
        await client.PostAsync("/with-trailers", first, TestContext.Current.CancellationToken);

        var second = JsonBody("{}");
        second.Headers.Add(KeyHeader, key);
        var secondResp = await client.PostAsync("/with-trailers", second, TestContext.Current.CancellationToken);

        secondResp.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResp.Headers.Contains("Idempotent-Replayed").Should().BeFalse(
            "responses that wrote trailers cannot be replayed by the snapshot writer, so they must not be cached");
        executions.Should().Be(2, "the handler must execute on each retry when trailers were written");
    }

    [Fact]
    public async Task Scoped_store_registration_is_resolved_per_request()
    {
        var instanceCount = 0;
        var shared = new InMemoryIdempotencyStore(new IdempotencyOptions(), TimeProvider.System);

        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(web => web
                .UseTestServer()
                .UseDefaultServiceProvider(opts =>
                {
                    opts.ValidateScopes = true;
                    opts.ValidateOnBuild = true;
                })
                .ConfigureServices(s =>
                {
                    s.AddRouting();
                    s.AddTrellisIdempotency();
                    s.AddScoped<IIdempotencyStore>(_ =>
                    {
                        Interlocked.Increment(ref instanceCount);
                        return new DelegatingIdempotencyStore(shared);
                    });
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseTrellisIdempotency();
                    app.UseEndpoints(endpoints => endpoints.MapPost("/idempotent-scoped", async ctx =>
                    {
                        ctx.Response.StatusCode = 201;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.WriteAsync("{\"created\":true}", ctx.RequestAborted);
                    }).WithMetadata(new IdempotentAttribute()));
                }));

        using var host = await builder.StartAsync(TestContext.Current.CancellationToken);
        var client = host.GetTestClient();

        var first = JsonBody("{}");
        first.Headers.Add(KeyHeader, Guid.NewGuid().ToString());
        var firstResp = await client.PostAsync("/idempotent-scoped", first, TestContext.Current.CancellationToken);
        firstResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = JsonBody("{}");
        second.Headers.Add(KeyHeader, Guid.NewGuid().ToString());
        var secondResp = await client.PostAsync("/idempotent-scoped", second, TestContext.Current.CancellationToken);
        secondResp.StatusCode.Should().Be(HttpStatusCode.Created);

        instanceCount.Should().Be(2,
            "a scoped IIdempotencyStore registration must be resolved fresh per request via InvokeAsync parameter injection rather than being root-captured by the middleware constructor");
    }

    [Fact]
    public async Task Handler_writing_via_body_writer_without_flush_replays_full_body()
    {
        var payload = Encoding.UTF8.GetBytes("{\"hello\":\"world\"}");

        using var host = await BuildHost(configureEndpoints: endpoints =>
            endpoints.MapPost("/pipewriter", ctx =>
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "application/json";
                var mem = ctx.Response.BodyWriter.GetMemory(payload.Length);
                payload.CopyTo(mem);
                ctx.Response.BodyWriter.Advance(payload.Length);

                // Intentionally do NOT call FlushAsync on BodyWriter: the captured snapshot
                // would be empty unless the middleware flushes the cached PipeWriter before
                // reading the capture buffer.
                return Task.CompletedTask;
            }).WithMetadata(new IdempotentAttribute()));

        var client = host.GetTestClient();
        var key = Guid.NewGuid().ToString();

        var first = JsonBody("{}");
        first.Headers.Add(KeyHeader, key);
        var firstResp = await client.PostAsync("/pipewriter", first, TestContext.Current.CancellationToken);
        firstResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstBody = await firstResp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        firstBody.Should().Be("{\"hello\":\"world\"}");

        var second = JsonBody("{}");
        second.Headers.Add(KeyHeader, key);
        var secondResp = await client.PostAsync("/pipewriter", second, TestContext.Current.CancellationToken);
        secondResp.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResp.Headers.GetValues("Idempotent-Replayed").Should().Contain("true");
        var secondBody = await secondResp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        secondBody.Should().Be(
            "{\"hello\":\"world\"}",
            "responses written via Response.BodyWriter.GetMemory + Advance without explicit FlushAsync must still be captured into the snapshot for replay");
    }

    private sealed class DelegatingIdempotencyStore : IIdempotencyStore
    {
        private readonly IIdempotencyStore inner;

        public DelegatingIdempotencyStore(IIdempotencyStore inner) => this.inner = inner;

        public ValueTask<IdempotencyReservationOutcome> TryReserveAsync(string scope, string key, string fingerprint, CancellationToken cancellationToken) =>
            this.inner.TryReserveAsync(scope, key, fingerprint, cancellationToken);

        public ValueTask CompleteAsync(string scope, string key, string reservationId, IdempotencyResponseSnapshot snapshot, CancellationToken cancellationToken) =>
            this.inner.CompleteAsync(scope, key, reservationId, snapshot, cancellationToken);

        public ValueTask AbandonAsync(string scope, string key, string reservationId, CancellationToken cancellationToken) =>
            this.inner.AbandonAsync(scope, key, reservationId, cancellationToken);
    }
}
