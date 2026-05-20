---
package: Trellis.Asp.ApiVersioning
namespaces: [Trellis.Asp.ApiVersioning]
types: [HttpResponseOptionsBuilderApiVersioningExtensions]
version: v1
last_verified: 2026-05-19
audience: [llm]
---
# Trellis.Asp.ApiVersioning — API Reference

## Header

- **Package:** `Trellis.Asp.ApiVersioning`
- **Namespace:** `Trellis.Asp.ApiVersioning`
- **Purpose:** API-versioning helper that auto-injects the `api-version` route value into `Location` headers emitted by `HttpResponseOptionsBuilder<T>.CreatedAtRoute(...)` / `CreatedAtAction(...)` (201 Created) and `HttpResponseOptionsBuilder<T>.WithLocation(...)` (200 OK on existing resources), so responses round-trip the requested version under query/header API versioning. Skips injection for `[ApiVersionNeutral]` and URL-segment-versioned endpoints.

See also: [trellis-api-asp.md](trellis-api-asp.md) — the underlying `HttpResponseOptionsBuilder<T>`, `CreatedAtRoute`, `CreatedAtAction`, `WithLocation`, and `WithRouteValueResolver` hook this package builds on. [trellis-api-analyzers.md](trellis-api-analyzers.md) — `TRLS023` warns on `CreatedAtRoute` / `CreatedAtAction` / `WithLocation` calls in versioned controllers that omit the `api-version` route value, and offers a code fix that chains `.WithVersionedRoute()`.

## Use this file when

- You return `Result<T>` responses from versioned controllers (`[ApiVersion("…")]`) and need a builder-generated `Location` header to round-trip the client's requested `api-version`.
- You configured `Asp.Versioning` with `QueryStringApiVersionReader`, `HeaderApiVersionReader`, or a composite reader (anything that is *not* URL-segment versioning) and need link generation to preserve the version.
- You want a per-request hook to inject any other route value into `Location` (the `WithRouteValueResolver` mechanism `Trellis.Asp` exposes; this package consumes it).

## Patterns Index

| Goal | Canonical API / pattern | See |
|---|---|---|
| Return 201 Created with versioned Location | Chain `.WithVersionedRoute()` after `CreatedAtRoute(...)` or `CreatedAtAction(...)` | [`HttpResponseOptionsBuilderApiVersioningExtensions`](#httpresponseoptionsbuilderapiversioningextensions) |
| Return 200 OK with versioned Location on an existing resource | Chain `.WithVersionedRoute()` after `WithLocation(...)` | [`WithLocation` composition](#withlocation-composition) |
| Single id route value | `CreatedAtRoute(routeName, x => x.Id).WithVersionedRoute()` (uses the single-id overload from `Trellis.Asp`) | [Composition examples](#composition-examples) |
| Multi-key route values | `CreatedAtRoute(routeName, x => new RouteValueDictionary { ["tenantId"] = x.TenantId, ["id"] = x.Id }).WithVersionedRoute()` | [Composition examples](#composition-examples) |
| Pin Location to a specific version (rare) | `CreatedAtRoute(...).WithVersionedRoute(new ApiVersion(new DateOnly(2026, 12, 1)))` | [Explicit-version overload](#explicit-version-overload) |
| `[ApiVersionNeutral]` controller | Use `CreatedAtRoute` (no version injected); `.WithVersionedRoute()` short-circuits the resolver to a no-op when the endpoint is neutral | [Behavioral notes](#behavioral-notes) |
| URL-segment versioning (`v{version:apiVersion}` in template) | Continue to use `CreatedAtRoute`; the segment is filled by the route template, not a query route value | [Behavioral notes](#behavioral-notes) |

## Common traps

- Do **not** add `["api-version"] = …` manually inside the route values dictionary when you chain `.WithVersionedRoute()`. The resolver runs *after* the route-values selector and **overwrites** any pre-existing `api-version` entry, so manual entries are silently discarded. Either trust the resolver, or use the `WithRouteValueResolver` hook directly with your own logic.
- Do not assume `.WithVersionedRoute()` works for URL-segment versioning. URL-segment templates resolve the version from the route template parameter, not from a query/header route value; the helper intentionally skips injection in that case.
- `WithLocation(...)` produces a `Location` header **without** changing the status code (typically 200 OK on `Result<T>` responses). Use it on state-transition endpoints that mutate an existing resource and want to point clients at the canonical URL (e.g., `POST /orders/{id}/return` returning 200 OK). For new-resource creation, use `CreatedAtRoute(...)` / `CreatedAtAction(...)` (201 Created). `Result<WriteOutcome<T>>` uses the `WriteOutcome` location/monitor URI instead; builder `WithLocation(...)` and route-value resolvers do not rewrite those outcome-owned URIs.
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
| `WithVersionedRoute<TDomain>(this HttpResponseOptionsBuilder<TDomain> builder)` | `HttpResponseOptionsBuilder<TDomain>` | Injects the configured `api-version` route value into the `Location` header emitted by a preceding `CreatedAtRoute(...)`, `CreatedAtAction(...)`, or `WithLocation(...)` call. The version is resolved per-request from `HttpContext`. Equivalent to `builder.WithRouteValueResolver("api-version", ResolveApiVersion)`. |
| `WithVersionedRoute<TDomain>(this HttpResponseOptionsBuilder<TDomain> builder, ApiVersion explicitVersion)` | `HttpResponseOptionsBuilder<TDomain>` | Escape hatch: pin the `Location` header to a specific `ApiVersion` regardless of what the client requested. Useful for cross-version `Location` redirects on deprecated endpoints. |

#### Behavioral notes

The api-version resolver runs per request inside the `LinkGenerator` callback (after the route-values selector produces the route values dictionary, before link generation). Resolution order:

1. **`HttpContext.RequestedApiVersion`** — primary signal; reflects whatever the configured `IApiVersionReader` parsed (query, header, media-type, URL segment, composite). This is the C# extension property from `Asp.Versioning.Http`.
2. **Endpoint metadata `ApiVersionMetadata.Map(ApiVersionMapping.Implicit).DeclaredApiVersions`** — fallback when (1) is null and exactly one declared version exists. Throws if the endpoint declares multiple versions and the client supplied none.
3. **`ApiVersioningOptions.DefaultApiVersion`** — final fallback, configured via `services.AddApiVersioning(o => o.DefaultApiVersion = …)`.

The resolver short-circuits to a no-op (no `api-version` route value injected) when:

- The endpoint is decorated with `[ApiVersionNeutral]` — version-neutral endpoints must not carry a version in their `Location` header.
- The endpoint participates in URL-segment versioning (the route template contains a `{version:apiVersion}` segment) — the segment is filled by the route template binder, not a query/header route value.

The route-value key is fixed at `"api-version"`, matching the default for `QueryStringApiVersionReader` and the conventional header name. Hosts using a non-default reader parameter name should register a custom resolver via `WithRouteValueResolver(<key>, …)` directly instead of using this package.

#### Composition examples

Single-id overload (sugar for the common `{ ["id"] = order.Id }` shape):

```csharp
return result.ToHttpResponse(opts => opts
    .CreatedAtRoute("Customers_GetById", c => c.Id)
    .WithVersionedRoute());
```

Generates `Location: /customers/42?api-version=2026-12-01` when the request specified `?api-version=2026-12-01`. The `idRouteKey` parameter on `CreatedAtRoute` defaults to `"id"`; supply a different key when the route template uses a different parameter name (e.g. `"orderId"`).

Multi-key route values:

```csharp
return result.ToHttpResponse(opts => opts
    .CreatedAtRoute(
        "Orders_GetById",
        o => new RouteValueDictionary
        {
            ["tenantId"] = o.TenantId,
            ["id"] = o.Id,
        })
    .WithVersionedRoute());
```

#### `WithLocation` composition

For state-transition endpoints that return 200 OK (or another 2xx) but want a `Location` header pointing to the canonical URL of the mutated resource:

```csharp
return result.ToHttpResponse(opts => opts
    .WithLocation("Orders_GetById", o => o.Id)
    .WithVersionedRoute());
```

Unlike `CreatedAtRoute`, `WithLocation` does **not** force the status code to 201 — the `Result<T>` response's natural 200 OK status is preserved. It does not affect `Result<WriteOutcome<T>>` responses, where `WriteOutcome.Created`, `WriteOutcome.Accepted`, and `WriteOutcome.AcceptedNoContent` own their literal `Location` / monitor URI values.

#### Explicit-version overload

```csharp
return result.ToHttpResponse(opts => opts
    .CreatedAtRoute("Orders_GetById", o => new RouteValueDictionary { ["id"] = o.Id })
    .WithVersionedRoute(new ApiVersion(new DateOnly(2026, 12, 1))));
```

Pins the `Location` to `?api-version=2026-12-01` regardless of what the client requested. Use this only for redirects to a fixed version (deprecation flows, version migration). For the common case, prefer the parameterless overload.

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

- **TRLS023** (`Trellis.Analyzers`) — warns on `HttpResponseOptionsBuilder<T>.CreatedAtRoute(...)`, `CreatedAtAction(...)`, or `WithLocation(...)` calls inside `[ApiVersion]`-decorated controllers when the chain is not followed by `.WithVersionedRoute(...)` and the route values dictionary literal does not include an `"api-version"` key. Code fix appends `.WithVersionedRoute()` and adds `using Trellis.Asp.ApiVersioning;` when missing.
