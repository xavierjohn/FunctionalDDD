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

## Domain → HTTP boundary

Default mappings (overridable per call via `.WithErrorMapping(...)` or globally via `TrellisAspOptions.ErrorStatusCodeMap`):

| Domain `Error` case | Status | Wire `kind` | Synthesized header |
|---|---|---|---|
| `InvalidInput` | 422 | `unprocessable-content` | – |
| `InvariantViolation` | 422 | `unprocessable-content` | – |
| `AuthenticationRequired` | 401 | `unauthorized` | `WWW-Authenticate` |
| `Forbidden` | 403 | `forbidden` | – |
| `NotFound` | 404 | `not-found` | – |
| `Conflict` | 409 | `conflict` | – |
| `Gone` | 410 | `gone` | – |
| `RateLimited` | 429 | `too-many-requests` | `Retry-After` (from `RetryAdvice`) |
| `Unavailable` | 503 | `service-unavailable` | `Retry-After` (from `RetryAdvice`) |
| `Unexpected` (default) | 500 | `internal-server-error` | – |
| `Unexpected { ReasonCode == "not_implemented" }` | 501 | `not-implemented` | – |
| `Aggregate` | inner-case status | inner kinds | per inner |
| `TransportFault(HttpError.X)` | 405 / 406 / 412 / 413 / 415 / 416 / 428 | inner wire kind | `Allow` / `Content-Range` per inner |

The domain `Kind` slug and the on-wire `kind` extension are intentionally distinct for the renamed cases (`invalid-input`, `invariant-violation`, `authentication-required`, `rate-limited`, `unavailable`, `unexpected`). The wire token is preserved at the boundary at the historical RFC-9110-aligned value, so external problem-details consumers see no change.

## Documentation
- [Full documentation](https://xavierjohn.github.io/Trellis/articles/integration-aspnet.html)
- [API Reference](https://xavierjohn.github.io/Trellis/api/index.html)

## Part of Trellis
This package is part of the [Trellis](https://github.com/xavierjohn/Trellis) framework.
