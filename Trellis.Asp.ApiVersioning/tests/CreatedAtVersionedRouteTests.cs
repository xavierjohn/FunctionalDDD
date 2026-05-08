namespace Trellis.Asp.ApiVersioning.Tests;

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using global::Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Trellis;
using Trellis.Asp;

/// <summary>
/// Pins the per-request resolution behaviour of
/// <see cref="HttpResponseOptionsBuilderApiVersioningExtensions.CreatedAtVersionedRoute{T}(HttpResponseOptionsBuilder{T}, string, Func{T, RouteValueDictionary})"/>.
/// Each test stands up a minimal ASP.NET Core host with a specific versioning configuration,
/// POSTs to a Created-returning controller action, and asserts the resulting <c>Location</c>
/// header carries (or correctly omits) the <c>api-version</c> route value.
/// </summary>
public sealed class CreatedAtVersionedRouteTests
{
    public const string ApiVersionV1 = "2026-11-12";
    public const string ApiVersionV2 = "2026-12-01";

    private static IHost CreateSingleVersionHost()
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(web => web
                .UseTestServer()
                .ConfigureServices(s =>
                {
                    s.AddTrellisAsp();
                    s.AddControllers().AddApplicationPart(typeof(SingleVersionController).Assembly);
                    s.AddApiVersioning(o =>
                    {
                        o.ReportApiVersions = true;
                        o.ApiVersionReader = new QueryStringApiVersionReader("api-version");
                        // Lets the no-client-version test reach the controller; the resolver
                        // then falls back to the single declared version.
                        o.AssumeDefaultVersionWhenUnspecified = true;
                        o.DefaultApiVersion = new ApiVersion(new DateOnly(2026, 11, 12));
                    }).AddMvc();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(e => e.MapControllers());
                }));
        return builder.Start();
    }

    private static IHost CreateMultiVersionHost(ApiVersion? defaultVersion = null)
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(web => web
                .UseTestServer()
                .ConfigureServices(s =>
                {
                    s.AddTrellisAsp();
                    s.AddControllers().AddApplicationPart(typeof(MultiVersionController).Assembly);
                    s.AddApiVersioning(o =>
                    {
                        o.ApiVersionReader = new QueryStringApiVersionReader("api-version");
                        if (defaultVersion is not null)
                        {
                            o.DefaultApiVersion = defaultVersion;
                            o.AssumeDefaultVersionWhenUnspecified = true;
                        }
                    }).AddMvc();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(e => e.MapControllers());
                }));
        return builder.Start();
    }

    [Fact]
    public async Task Single_declared_version_with_client_request_echoes_version_in_Location()
    {
        using var host = CreateSingleVersionHost();
        using var client = host.GetTestClient();

        var resp = await client.PostAsync($"/customers?api-version={ApiVersionV1}", JsonContent("{}"), TestContext.Current.CancellationToken);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        resp.Headers.Location!.OriginalString.Should().Contain($"api-version={ApiVersionV1}");
    }

    [Fact]
    public async Task Single_declared_version_with_no_client_request_falls_back_to_declared()
    {
        // Single declared version is unambiguous — the resolver should fall back to it
        // even when the client didn't specify a version.
        using var host = CreateSingleVersionHost();
        using var client = host.GetTestClient();

        // No api-version query param.
        var resp = await client.PostAsync("/customers", JsonContent("{}"), TestContext.Current.CancellationToken);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        resp.Headers.Location!.OriginalString.Should().Contain($"api-version={ApiVersionV1}");
    }

    [Fact]
    public async Task Multi_version_with_client_request_echoes_requested_version()
    {
        using var host = CreateMultiVersionHost();
        using var client = host.GetTestClient();

        var resp = await client.PostAsync($"/orders?api-version={ApiVersionV2}", JsonContent("{}"), TestContext.Current.CancellationToken);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        resp.Headers.Location!.OriginalString.Should().Contain($"api-version={ApiVersionV2}");
    }

    [Fact]
    public async Task Multi_version_with_no_client_request_uses_default_when_set()
    {
        using var host = CreateMultiVersionHost(defaultVersion: new ApiVersion(new DateOnly(2026, 11, 12)));
        using var client = host.GetTestClient();

        var resp = await client.PostAsync("/orders", JsonContent("{}"), TestContext.Current.CancellationToken);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        resp.Headers.Location!.OriginalString.Should().Contain($"api-version={ApiVersionV1}");
    }

    [Fact]
    public async Task Explicit_version_overload_pins_regardless_of_client_request()
    {
        using var host = CreateMultiVersionHost();
        using var client = host.GetTestClient();

        // Client requests v2 but the controller pinned the Location to v1 via the explicit overload.
        var resp = await client.PostAsync($"/orders/pinned?api-version={ApiVersionV2}", JsonContent("{}"), TestContext.Current.CancellationToken);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        resp.Headers.Location!.OriginalString.Should().Contain($"api-version={ApiVersionV1}");
        resp.Headers.Location.OriginalString.Should().NotContain($"api-version={ApiVersionV2}");
    }

    [Fact]
    public async Task Single_id_convenience_overload_injects_id_and_version()
    {
        using var host = CreateSingleVersionHost();
        using var client = host.GetTestClient();

        var resp = await client.PostAsync($"/customers/single-id?api-version={ApiVersionV1}", JsonContent("{}"), TestContext.Current.CancellationToken);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        // /customers/{id} with id from idSelector + api-version from the resolver.
        resp.Headers.Location!.OriginalString.Should().Contain("/customers/42");
        resp.Headers.Location.OriginalString.Should().Contain($"api-version={ApiVersionV1}");
    }

    private static StringContent JsonContent(string json) =>
        new(json, System.Text.Encoding.UTF8, "application/json");
}

#region Test fixtures

[ApiController]
[ApiVersion(CreatedAtVersionedRouteTests.ApiVersionV1)]
[Route("customers")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822", Justification = "Test fixture controllers don't need to be static.")]
public sealed class SingleVersionController : ControllerBase
{
    [HttpGet("{id:int}", Name = "Customers_GetById")]
    public IActionResult Get(int id) => Ok(new { id });

    [HttpPost]
    public ActionResult<CreatedCustomer> Post() =>
        Result.Ok(new CreatedCustomer(42))
            .ToHttpResponse(opts => opts.CreatedAtVersionedRoute(
                "Customers_GetById",
                c => new RouteValueDictionary { ["id"] = c.Id }))
            .AsActionResult<CreatedCustomer>();

    [HttpPost("single-id")]
    public ActionResult<CreatedCustomer> PostSingleId() =>
        Result.Ok(new CreatedCustomer(42))
            .ToHttpResponse(opts => opts.CreatedAtVersionedRoute("Customers_GetById", c => (object)c.Id))
            .AsActionResult<CreatedCustomer>();
}

[ApiController]
[ApiVersion(CreatedAtVersionedRouteTests.ApiVersionV1)]
[ApiVersion(CreatedAtVersionedRouteTests.ApiVersionV2)]
[Route("orders")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822", Justification = "Test fixture controllers don't need to be static.")]
public sealed class MultiVersionController : ControllerBase
{
    [HttpGet("{id:int}", Name = "Orders_GetById")]
    public IActionResult Get(int id) => Ok(new { id });

    [HttpPost]
    public ActionResult<CreatedOrder> Post() =>
        Result.Ok(new CreatedOrder(99))
            .ToHttpResponse(opts => opts.CreatedAtVersionedRoute(
                "Orders_GetById",
                o => new RouteValueDictionary { ["id"] = o.Id }))
            .AsActionResult<CreatedOrder>();

    [HttpPost("pinned")]
    public ActionResult<CreatedOrder> PostPinned() =>
        Result.Ok(new CreatedOrder(99))
            .ToHttpResponse(opts => opts.CreatedAtVersionedRoute(
                "Orders_GetById",
                o => new RouteValueDictionary { ["id"] = o.Id },
                explicitVersion: new ApiVersion(new DateOnly(2026, 11, 12))))
            .AsActionResult<CreatedOrder>();
}

public sealed record CreatedCustomer(int Id);

public sealed record CreatedOrder(int Id);

#endregion
