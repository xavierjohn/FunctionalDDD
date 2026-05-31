# Trellis.Asp

[![NuGet Package](https://img.shields.io/nuget/v/Trellis.Asp.svg)](https://www.nuget.org/packages/Trellis.Asp)

ASP.NET Core integration for Trellis results, scalar value validation, and clean HTTP responses.

## Installation
```bash
dotnet add package Trellis.Asp
```

## Quick Example
```csharp
using Trellis;
using Trellis.Asp;

builder.Services.AddTrellisAsp();

app.MapGet("/widgets/{id}", (string id) =>
    Result.Ok(id).ToHttpResponse());
```

## Key Features
- Convert `Result<T>` and `Error` values into consistent HTTP responses.
- Validate Trellis scalar values during model binding and JSON deserialization.
- Support controller and minimal API styles, including AOT-friendly setups.
- Emit [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) Problem Details with `instance` populated from the request path so clients can correlate failures with the originating request.
- Ship the canonical ProblemDetails recipe via `AddTrellisProblemDetails()` + `UseTrellisProblemDetails()` (trace id from `Activity.Current`, friendly 500 detail, `allow` array on 405). Composes with any consumer `CustomizeProblemDetails` callback so the application keeps the last word on collisions.
- Compose a worker/system actor outside HTTP via `services.AddTrellisWorkerActor(systemActor)`. Wraps the existing unkeyed `IActorProvider` so HTTP requests still resolve through it and background-worker scopes (no `HttpContext`) resolve to the supplied system actor.

## Documentation
- [Full documentation](https://xavierjohn.github.io/Trellis/articles/integration-aspnet.html)
- [API Reference](https://xavierjohn.github.io/Trellis/api/index.html)

## Part of Trellis
This package is part of the [Trellis](https://github.com/xavierjohn/Trellis) framework.
