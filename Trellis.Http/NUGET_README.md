# HTTP Client Extensions

[![NuGet Package](https://img.shields.io/nuget/v/Trellis.Http.svg)](https://www.nuget.org/packages/Trellis.Http)

Fluent HTTP client extensions for Railway Oriented Programming — handle status codes, deserialize JSON, and compose error handling with `Result<T>` and `Maybe<T>`.

## Installation

```bash
dotnet add package Trellis.Http
```

## Quick Start

```csharp
using Trellis;
using Trellis.Http;

var result = await httpClient.GetAsync($"api/users/{userId}", ct)
    .HandleNotFoundAsync(Error.NotFound("User not found", userId))
    .ReadResultFromJsonAsync(UserJsonContext.Default.User, ct);

// result is Result<User> — either Success with User or Failure with NotFoundError
```

## Status Code Handlers

```csharp
var response = await httpClient.PostAsync("api/orders", content, ct);
var result = await response
    .HandleUnauthorized(Error.Unauthorized("Please login"))
    .HandleForbidden(Error.Forbidden("Admin access required"))
    .HandleConflict(Error.Conflict("Order already exists"))
    .HandleClientError(code => Error.BadRequest($"Client error: {code}"))
    .HandleServerError(code => Error.ServiceUnavailable($"Server error: {code}"))
    .ReadResultFromJsonAsync(OrderJsonContext.Default.Order, ct);
```

| Handler | HTTP Status | Error Type |
|---------|-------------|------------|
| `HandleNotFoundAsync` | 404 | `NotFoundError` |
| `HandleUnauthorizedAsync` | 401 | `UnauthorizedError` |
| `HandleForbiddenAsync` | 403 | `ForbiddenError` |
| `HandleConflictAsync` | 409 | `ConflictError` |
| `HandleClientErrorAsync` | 4xx | Custom via factory |
| `HandleServerErrorAsync` | 5xx | Custom via factory |
| `EnsureSuccessAsync` | Any non-success | Custom via factory |

## ROP Pipeline Integration

```csharp
var result = await httpClient.GetAsync($"api/products/{productId}", ct)
    .HandleNotFoundAsync(Error.NotFound("Product", productId))
    .ReadResultFromJsonAsync(ProductJsonContext.Default.Product, ct)
    .EnsureAsync(p => p.IsAvailable, Error.Conflict("Product unavailable"))
    .TapAsync(p => _logger.LogInformation("Retrieved: {Name}", p.Name))
    .MapAsync(p => new ProductViewModel(p));
```

## JSON Deserialization

```csharp
// Required value — null response returns error
Task<Result<T>> ReadResultFromJsonAsync<T>(jsonTypeInfo, ct)

// Optional value — null response returns Maybe.None
Task<Result<Maybe<T>>> ReadResultMaybeFromJsonAsync<T>(jsonTypeInfo, ct)
```

## Related Packages

- [Trellis.Results](https://www.nuget.org/packages/Trellis.Results) — Core `Result<T>` and `Maybe<T>` types
- [Trellis.Asp](https://www.nuget.org/packages/Trellis.Asp) — ASP.NET Core integration (Result → HTTP responses)

## License

MIT — see [LICENSE](https://github.com/xavierjohn/Trellis/blob/main/LICENSE) for details.
