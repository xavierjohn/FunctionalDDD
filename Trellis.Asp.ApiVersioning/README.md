# Trellis.Asp.ApiVersioning

API-versioning helpers for [Trellis.Asp](../Trellis.Asp/README.md). Adds a `WithVersionedRoute(...)` extension on `HttpResponseOptionsBuilder<TDomain>` that auto-injects the `api-version` route value into `Location` headers, so responses round-trip the requested version under query/header API versioning. Chain after any builder method that emits a `Location` header — `CreatedAtRoute(...)` for 201 Created, `WithLocation(...)` for state-transition 2xx responses on existing resources.

## Why this package exists

Under query-string or header API versioning (`Asp.Versioning.QueryStringApiVersionReader`, `HeaderApiVersionReader`, or composite readers), `Location` headers from `HttpResponseOptionsBuilder<T>.CreatedAtRoute(routeName, routeValues)` silently omit the `api-version` parameter unless every author remembers to add it manually. The result: a `POST /api/orders?api-version=2026-12-01` returns `Location: /api/orders/42` (no version), and the follow-up `GET /api/orders/42` 404s because the controller is registered under the versioned route group.

The bug is invisible without integration tests that assert on the full Location URL, easy to miss in code review, and silently regresses across cycles. `.WithVersionedRoute()` removes the trap by injecting the version at request time using the configured `IApiVersionReader` chain.

## Resolution order

The resolver runs per request inside the `LinkGenerator` callback:

1. **`HttpContext.RequestedApiVersion`** — primary signal; reflects whatever the configured `IApiVersionReader` parsed (query, header, media-type, URL segment, composite).
2. **Endpoint metadata `ApiVersionMetadata.Map(ApiVersionMapping.Implicit).DeclaredApiVersions`** — fallback when (1) is null and exactly one declared version exists.
3. **`ApiVersioningOptions.DefaultApiVersion`** — final fallback, configured via `services.AddApiVersioning(o => o.DefaultApiVersion = …)`.

The resolver short-circuits to a no-op (no `api-version` route value injected) when:

- The endpoint is decorated with `[ApiVersionNeutral]`.
- The endpoint participates in URL-segment versioning (route template contains `:apiVersion`).

## Usage

```csharp
using Trellis.Asp;
using Trellis.Asp.ApiVersioning;

[ApiController]
[ApiVersion("2026-12-01")]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    [HttpGet("{id:int}", Name = "Orders_GetById")]
    public IActionResult Get(int id) => Ok(...);

    [HttpPost]
    public ActionResult<Order> Create([FromBody] CreateOrderRequest req) =>
        _mediator.Send(new CreateOrderCommand(req))
            .ToHttpResponse(opts => opts
                .CreatedAtRoute(
                    "Orders_GetById",
                    o => new RouteValueDictionary { ["id"] = o.Id })
                .WithVersionedRoute())
            .AsActionResult<Order>();
}
```

The resulting `Location` header is `/api/orders/42?api-version=2026-12-01` (or whatever version the client requested).

To pin a specific version regardless of what the client requested (cross-version `Location` redirects on deprecated endpoints), use the explicit overload: `.WithVersionedRoute(new ApiVersion(new DateOnly(2026, 12, 1)))`.

## Related diagnostics

- **TRLS023** (`Trellis.Analyzers`) warns on `HttpResponseOptionsBuilder<T>.CreatedAtRoute(...)` or `WithLocation(...)` calls inside `[ApiVersion]`-decorated controllers when the chain is not followed by `.WithVersionedRoute(...)` and the route values dictionary literal does not include an `"api-version"` key. The code fix appends `.WithVersionedRoute()` and adds `using Trellis.Asp.ApiVersioning;` when missing.

## Configuration

Register API versioning in the host as you would normally; this package does not require its own `services.AddXxx(...)` call:

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(new DateOnly(2026, 12, 1));
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("api-version"));
});
```

The package depends on `Asp.Versioning.Http` (for the `HttpContext.RequestedApiVersion` extension property), `Asp.Versioning.Mvc`, and `Asp.Versioning.Mvc.ApiExplorer`.

## Documentation

- [API Reference](https://xavierjohn.github.io/Trellis/api/index.html)
- [`trellis-api-asp-apiversioning.md`](../docs/docfx_project/api_reference/trellis-api-asp-apiversioning.md) — LLM-targeted API reference

## Part of Trellis

This package is part of the [Trellis](https://github.com/xavierjohn/Trellis) framework.
