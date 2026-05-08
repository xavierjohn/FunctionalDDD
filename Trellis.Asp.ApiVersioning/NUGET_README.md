# Trellis.Asp.ApiVersioning

[![NuGet Package](https://img.shields.io/nuget/v/Trellis.Asp.ApiVersioning.svg)](https://www.nuget.org/packages/Trellis.Asp.ApiVersioning)

API-versioning helpers for `Trellis.Asp` — auto-injects the `api-version` route value into `Location` headers so 201 Created responses round-trip the requested version under query/header API versioning.

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
    .CreatedAtVersionedRoute("Customers_GetById", c => c.Id.Value));
//   ↑ Location header includes ?api-version=<requested-version> automatically.
```

## Why
Under query/header API versioning, `Location` headers from `CreatedAtRoute(...)` silently omit the `api-version` parameter unless every author remembers to add it to the route values dictionary — a recurring source of dereference 404s that's invisible without integration tests. `CreatedAtVersionedRoute` injects the version at request time using the configured `IApiVersionReader` chain, with sensible fallbacks and explicit failures for ambiguous configurations.

## Key Features
- Three overloads for multi-key, single-id, and explicit-version pinning scenarios
- Per-request resolution via `httpContext.RequestedApiVersion` (the `Asp.Versioning.Http` extension property), falling back to declared and default versions
- Honours `[ApiVersionNeutral]` and URL-segment versioning by skipping injection
- Throws on degenerate configurations (multi-version action with no client-requested version and no `DefaultApiVersion`) instead of silently picking

## Documentation
- [API Reference](https://xavierjohn.github.io/Trellis/api/index.html)

## Part of Trellis
This package is part of the [Trellis](https://github.com/xavierjohn/Trellis) framework.
