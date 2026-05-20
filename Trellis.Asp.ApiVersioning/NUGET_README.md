# Trellis.Asp.ApiVersioning

[![NuGet Package](https://img.shields.io/nuget/v/Trellis.Asp.ApiVersioning.svg)](https://www.nuget.org/packages/Trellis.Asp.ApiVersioning)

API-versioning helpers for `Trellis.Asp` — auto-injects the `api-version` route value into `Location` headers so responses round-trip the requested version under query/header API versioning.

## Installation
```bash
dotnet add package Trellis.Asp.ApiVersioning
```

## Quick Example
```csharp
using Trellis;
using Trellis.Asp;
using Trellis.Asp.ApiVersioning;

result.ToHttpResponse(opts => opts
    .CreatedAtRoute("Customers_GetById", c => c.Id.Value)
    .WithVersionedRoute());
//   ↑ Location header includes ?api-version=<requested-version> automatically.
```

`WithVersionedRoute()` chains after any builder method that emits a builder-generated `Location` header — including `CreatedAtRoute(...)` / `CreatedAtAction(...)` (201 Created) and `WithLocation(...)` (2xx state-transition responses on existing resources).

## Why
Under query/header API versioning, `Location` headers from `CreatedAtRoute(...)` / `CreatedAtAction(...)` / `WithLocation(...)` silently omit the `api-version` parameter unless every author remembers to add it to the route values dictionary — a recurring source of dereference 404s that's invisible without integration tests. `WithVersionedRoute()` injects the version at request time using the configured `IApiVersionReader` chain, with sensible fallbacks and explicit failures for ambiguous configurations.

## Key Features
- Composes with `CreatedAtRoute(...)`, `CreatedAtAction(...)`, `WithLocation(...)`, and any other builder-generated Location method
- Two overloads: per-request resolution (default) and explicit-version pinning (`WithVersionedRoute(ApiVersion)`)
- Per-request resolution via `httpContext.RequestedApiVersion` (the `Asp.Versioning.Http` extension property), falling back to declared and default versions
- Honours `[ApiVersionNeutral]` and URL-segment versioning by skipping injection (applies to both overloads — even explicit pinning won't inject a version into a neutral-endpoint Location or duplicate a path-segment version)
- Throws on degenerate configurations (multi-version action with no client-requested version and no `DefaultApiVersion`) instead of silently picking

## Documentation
- [API Reference](https://xavierjohn.github.io/Trellis/api/index.html)

## Part of Trellis
This package is part of the [Trellis](https://github.com/xavierjohn/Trellis) framework.
