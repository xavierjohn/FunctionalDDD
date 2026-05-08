---
package: Trellis.Asp.ApiVersioning
namespaces: [Trellis.Asp.ApiVersioning]
types: [HttpResponseOptionsBuilderApiVersioningExtensions]
version: v1
last_verified: 2026-05-08
audience: [llm]
---
# Trellis.Asp.ApiVersioning — API Reference

## Header

- **Package:** `Trellis.Asp.ApiVersioning`
- **Namespace:** `Trellis.Asp.ApiVersioning`
- **Purpose:** API-versioning helpers that auto-inject the `api-version` route value into `Location` headers produced by `HttpResponseOptionsBuilder<T>.CreatedAtRoute(...)`, so 201 Created responses round-trip the requested version under query/header API versioning. Skips injection for `[ApiVersionNeutral]` and URL-segment-versioned endpoints.

See also: [trellis-api-asp.md](trellis-api-asp.md) — the underlying `HttpResponseOptionsBuilder<T>` and `WithRouteValueResolver` hook this package builds on. [trellis-api-analyzers.md](trellis-api-analyzers.md) — `TRLS023` warns on `CreatedAtRoute` calls in versioned controllers that omit the `api-version` route value, and offers a code fix that rewrites them to `CreatedAtVersionedRoute`.

## Use this file when

- You return `Result<T>` / `WriteOutcome<T>` 201 Created responses from versioned controllers (`[ApiVersion("…")]`) and need the `Location` header to round-trip the client's requested `api-version`.
- You configured `Asp.Versioning` with `QueryStringApiVersionReader`, `HeaderApiVersionReader`, or a composite reader (anything that is *not* URL-segment versioning) and need link generation to preserve the version.
- You want a per-request hook to inject any other route value into `Location` (the `WithRouteValueResolver` mechanism `Trellis.Asp` exposes; this package consumes it).

## Patterns Index

| Goal | Canonical API / pattern | See |
|---|---|---|
| Return 201 Created with versioned Location | Replace `CreatedAtRoute(...)` with `CreatedAtVersionedRoute(...)` | [`HttpResponseOptionsBuilderApiVersioningExtensions`](#httpresponseoptionsbuilderapiversioningextensions) |
| Single id route value | `CreatedAtVersionedRoute(routeName, x => x.Id)` (single-id overload) | [Single-id overload](#single-id-overload) |
| Multi-key route values | `CreatedAtVersionedRoute(routeName, x => new RouteValueDictionary { ["tenantId"] = x.TenantId, ["id"] = x.Id })` | [Multi-key overload](#multi-key-overload) |
| Pin Location to a specific version (rare) | `CreatedAtVersionedRoute(routeName, selector, explicitVersion: new ApiVersion(new DateOnly(2026, 12, 1)))` | [Explicit-version overload](#explicit-version-overload) |
| `[ApiVersionNeutral]` controller | Use `CreatedAtRoute` (no version injected); `CreatedAtVersionedRoute` short-circuits the resolver to a no-op when the endpoint is neutral | [Behavioral notes](#behavioral-notes) |
| URL-segment versioning (`v{version:apiVersion}` in template) | Continue to use `CreatedAtRoute`; the segment is filled by the route template, not a query route value | [Behavioral notes](#behavioral-notes) |

## Common traps

- Do **not** add `["api-version"] = …` manually inside the route values dictionary when you call `CreatedAtVersionedRoute`. The resolver runs *after* the route-values selector and **overwrites** any pre-existing `api-version` entry, so manual entries are silently discarded. Either trust the resolver, or use the `WithRouteValueResolver` hook directly with your own logic.
- Do not assume `CreatedAtVersionedRoute` works for URL-segment versioning. URL-segment templates resolve the version from the route template parameter, not from a query/header route value; the helper intentionally skips injection in that case.
- The route values selector should return a fresh `RouteValueDictionary` per call. The runtime clones the dictionary defensively before applying resolvers, but selectors that return a shared instance still cost an unnecessary allocation per request.
- Configure `Asp.Versioning` with an explicit `DefaultApiVersion` if you support multiple declared versions on the same controller. With `AllowMultiple = true` `[ApiVersion]` and no client-supplied version, the resolver throws rather than silently picking one.

## Types

### `HttpResponseOptionsBuilderApiVersioningExtensions`

**Declaration**

```csharp
public static class HttpResponseOptionsBuilderApiVersioningExtensions
```

**Constructors**

- None. This is a static class.

**Properties**

| Name | Type | Description |
| --- | --- | --- |
| None | — | This static class exposes no public properties. |

**Methods**

| Signature | Returns | Behavior |
| --- | --- | --- |
| `CreatedAtVersionedRoute<TDomain>(this HttpResponseOptionsBuilder<TDomain> builder, string routeName, Func<TDomain, RouteValueDictionary> routeValues)` | `HttpResponseOptionsBuilder<TDomain>` | Multi-key overload. Equivalent to `CreatedAtRoute(routeName, routeValues).WithRouteValueResolver("api-version", ResolveApiVersion)`. |
| `CreatedAtVersionedRoute<TDomain>(this HttpResponseOptionsBuilder<TDomain> builder, string routeName, Func<TDomain, object> idSelector, string idRouteKey = "id")` | `HttpResponseOptionsBuilder<TDomain>` | Single-id overload. Sugar for the common case where the new resource has a single `id` route parameter. |
| `CreatedAtVersionedRoute<TDomain>(this HttpResponseOptionsBuilder<TDomain> builder, string routeName, Func<TDomain, RouteValueDictionary> routeValues, ApiVersion explicitVersion)` | `HttpResponseOptionsBuilder<TDomain>` | Explicit-version overload. Pins the `api-version` route value to a specific `ApiVersion` instance (typically constructed as `new ApiVersion(new DateOnly(...))` for date-format versioning) instead of resolving from the request. Useful for redirects to a fixed version. |

#### Behavioral notes

The api-version resolver runs per request inside the `LinkGenerator` callback (after `_routeValuesSelector` produces the route values dictionary, before link generation). Resolution order:

1. **`HttpContext.RequestedApiVersion`** — primary signal; reflects whatever the configured `IApiVersionReader` parsed (query, header, media-type, URL segment, composite). This is the C# extension property from `Asp.Versioning.Http`.
2. **Endpoint metadata `ApiVersionMetadata.Map(ApiVersionMapping.Implicit).DeclaredApiVersions`** — fallback when (1) is null and exactly one declared version exists. Throws if the endpoint declares multiple versions and the client supplied none.
3. **`ApiVersioningOptions.DefaultApiVersion`** — final fallback, configured via `services.AddApiVersioning(o => o.DefaultApiVersion = …)`.

The resolver short-circuits to a no-op (no `api-version` route value injected) when:

- The endpoint is decorated with `[ApiVersionNeutral]` — version-neutral endpoints must not carry a version in their `Location` header.
- The endpoint participates in URL-segment versioning (the route template contains a `{version:apiVersion}` segment) — the segment is filled by the route template binder, not a query/header route value.

The route-value key is fixed at `"api-version"`, matching the default for `QueryStringApiVersionReader` and the conventional header name. Hosts using a non-default reader parameter name should register a custom resolver via `WithRouteValueResolver(<key>, …)` directly instead of using this package.

#### Single-id overload

```csharp
return result.ToHttpResponse(opts => opts
    .CreatedAtVersionedRoute("Customers_GetById", c => c.Id));
```

Generates `Location: /customers/42?api-version=2026-12-01` when the request specified `?api-version=2026-12-01`. The `idRouteKey` parameter defaults to `"id"`; supply a different key when the route template uses a different parameter name (e.g. `"orderId"`).

#### Multi-key overload

```csharp
return result.ToHttpResponse(opts => opts
    .CreatedAtVersionedRoute(
        "Orders_GetById",
        o => new RouteValueDictionary
        {
            ["tenantId"] = o.TenantId,
            ["id"] = o.Id,
        }));
```

#### Explicit-version overload

```csharp
return result.ToHttpResponse(opts => opts
    .CreatedAtVersionedRoute(
        "Orders_GetById",
        o => new RouteValueDictionary { ["id"] = o.Id },
        explicitVersion: new ApiVersion(new DateOnly(2026, 12, 1))));
```

Pins the `Location` to `?api-version=2026-12-01` regardless of what the client requested. Use this only for redirects to a fixed version (deprecation flows, version migration). For the common case, prefer the request-driven overloads.

### Configuration

Register API versioning in the host as you would normally; `Trellis.Asp.ApiVersioning` does not require its own `services.AddXxx(...)` call:

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

The package depends on `Asp.Versioning.Http` (for `HttpContext.RequestedApiVersion`), `Asp.Versioning.Mvc`, and `Asp.Versioning.Mvc.ApiExplorer`.

## Related diagnostics

- **TRLS023** (`Trellis.Analyzers`) — warns on `HttpResponseOptionsBuilder<T>.CreatedAtRoute(...)` calls inside `[ApiVersion]`-decorated controllers when the route values dictionary literal does not include an `"api-version"` key. Code fix rewrites the call to `CreatedAtVersionedRoute(...)` and adds `using Trellis.Asp.ApiVersioning;` when missing.
