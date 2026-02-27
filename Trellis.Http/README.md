# Trellis.Http — HTTP Client Extensions

[![NuGet Package](https://img.shields.io/nuget/v/Trellis.Http.svg)](https://www.nuget.org/packages/Trellis.Http)

Fluent HTTP client extensions for Railway Oriented Programming — handle status codes, deserialize JSON, and compose error handling with `Result<T>` and `Maybe<T>`.

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [API Reference](#api-reference)
- [Best Practices](#best-practices)
- [Related Packages](#related-packages)

## Installation

```bash
dotnet add package Trellis.Http
```

## Quick Start

### Basic Error Handling

```csharp
using Trellis;

// Handle 404 Not Found
var result = await httpClient.GetAsync($"api/users/{userId}", ct)
    .HandleNotFoundAsync(Error.NotFound("User not found", userId))
    .ReadResultFromJsonAsync(UserJsonContext.Default.User, ct);

// Result will contain either:
// - Success with User object
// - Failure with NotFoundError
```

### Handle Specific HTTP Status Codes

```csharp
// Handle authentication/authorization errors
var response = await httpClient.PostAsync("api/admin/users", content, ct);
var result = await response
    .HandleUnauthorized(Error.Unauthorized("Please login"))
    .HandleForbidden(Error.Forbidden("Admin access required"))
    .HandleConflict(Error.Conflict("Username already exists"))
    .ReadResultFromJsonAsync(UserJsonContext.Default.User, ct);

// Each handler only intercepts its specific status code
// - 401 → UnauthorizedError
// - 403 → ForbiddenError
// - 409 → ConflictError
// - Other codes pass through to next handler
```

### Handle Error Ranges

```csharp
// Handle all client errors (4xx) or server errors (5xx) at once
var result = await httpClient.GetAsync("api/data", ct)
    .HandleClientErrorAsync(code => Error.BadRequest($"Client error: {code}"))
    .HandleServerErrorAsync(code => Error.ServiceUnavailable($"Server error: {code}"))
    .ReadResultFromJsonAsync(DataJsonContext.Default.Data, ct);

// Client errors (400-499) → Custom error via factory
// Server errors (500+) → Custom error via factory
// Success codes → Continue to JSON deserialization
```

### Ensure Success Status

```csharp
// Functional alternative to EnsureSuccessStatusCode()
var result = await httpClient.DeleteAsync($"api/items/{id}", ct)
    .EnsureSuccessAsync()  // Returns Result instead of throwing
    .TapAsync(response => _logger.LogInformation("Deleted item {Id}", id));

// With custom error factory
var result = await httpClient.GetAsync("api/data", ct)
    .EnsureSuccessAsync(code => Error.Unexpected($"API call failed with {code}"))
    .ReadResultFromJsonAsync(jsonContext, ct);
```

### Custom Error Handling

```csharp
var result = await httpClient.PostAsync("api/orders", content, ct)
    .HandleFailureAsync(
        async (response, context, ct) =>
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            return Error.BadRequest($"Order creation failed: {errorContent}");
        },
        context: null,
        ct);
```

### JSON Deserialization with Maybe

```csharp
// Returns Maybe<T> when the value might be null
var result = await httpClient.GetAsync($"api/users/{userId}/profile", ct)
    .ReadResultMaybeFromJsonAsync(ProfileJsonContext.Default.Profile, ct)
    .MapAsync(maybe => maybe.HasValue 
        ? $"Profile: {maybe.Value.Name}" 
        : "No profile available");
```

### Railway Oriented Programming Chain

```csharp
var result = await httpClient.GetAsync($"api/products/{productId}", ct)
    .HandleNotFoundAsync(Error.NotFound("Product", productId))
    .ReadResultFromJsonAsync(ProductJsonContext.Default.Product, ct)
    .EnsureAsync(
        p => p.IsAvailable, 
        Error.Conflict("Product is not available"))
    .TapAsync(p => _logger.LogInformation("Retrieved product: {Name}", p.Name))
    .MapAsync(p => new ProductViewModel(p));
```

### Composing Multiple Status Handlers

```csharp
// Chain multiple handlers for comprehensive error handling
var response = await httpClient.PostAsync("api/orders", orderContent, ct);
var result = await response
    .HandleUnauthorized(Error.Unauthorized("Please login to place orders"))
    .HandleForbidden(Error.Forbidden("Your account cannot place orders"))
    .HandleConflict(Error.Conflict("Order already exists"))
    .HandleClientError(code => Error.BadRequest($"Invalid order data: {code}"))
    .HandleServerError(code => Error.ServiceUnavailable($"Order service unavailable: {code}"))
    .ReadResultFromJsonAsync(OrderJsonContext.Default.Order, ct);

// Handlers are evaluated in order — first match wins
```

### Working with Result<HttpResponseMessage>

```csharp
Result<HttpResponseMessage> responseResult = await httpClient
    .GetAsync("api/data", ct)
    .HandleNotFoundAsync(Error.NotFound("Data not found"));

// Chain further operations on the Result
var data = await responseResult
    .ReadResultFromJsonAsync(DataJsonContext.Default.Data, ct)
    .MapAsync(d => d.Transform());
```

## API Reference

### Status Code Handlers

#### HandleNotFound / HandleNotFoundAsync

Converts HTTP 404 responses to `NotFoundError`.

```csharp
Result<HttpResponseMessage> HandleNotFound(
    this HttpResponseMessage response, 
    NotFoundError notFoundError)

Result<HttpResponseMessage> HandleNotFound(
    this Result<HttpResponseMessage> result, 
    NotFoundError notFoundError)

Task<Result<HttpResponseMessage>> HandleNotFoundAsync(
    this Task<HttpResponseMessage> responseTask, 
    NotFoundError notFoundError)

Task<Result<HttpResponseMessage>> HandleNotFoundAsync(
    this Task<Result<HttpResponseMessage>> resultTask, 
    NotFoundError notFoundError)
```

#### HandleUnauthorized / HandleUnauthorizedAsync

Converts HTTP 401 responses to `UnauthorizedError`.

```csharp
Result<HttpResponseMessage> HandleUnauthorized(
    this HttpResponseMessage response, 
    UnauthorizedError unauthorizedError)

Result<HttpResponseMessage> HandleUnauthorized(
    this Result<HttpResponseMessage> result, 
    UnauthorizedError unauthorizedError)

Task<Result<HttpResponseMessage>> HandleUnauthorizedAsync(
    this Task<HttpResponseMessage> responseTask, 
    UnauthorizedError unauthorizedError)

Task<Result<HttpResponseMessage>> HandleUnauthorizedAsync(
    this Task<Result<HttpResponseMessage>> resultTask, 
    UnauthorizedError unauthorizedError)
```

#### HandleForbidden / HandleForbiddenAsync

Converts HTTP 403 responses to `ForbiddenError`.

```csharp
Result<HttpResponseMessage> HandleForbidden(
    this HttpResponseMessage response, 
    ForbiddenError forbiddenError)

Result<HttpResponseMessage> HandleForbidden(
    this Result<HttpResponseMessage> result, 
    ForbiddenError forbiddenError)

Task<Result<HttpResponseMessage>> HandleForbiddenAsync(
    this Task<HttpResponseMessage> responseTask, 
    ForbiddenError forbiddenError)

Task<Result<HttpResponseMessage>> HandleForbiddenAsync(
    this Task<Result<HttpResponseMessage>> resultTask, 
    ForbiddenError forbiddenError)
```

#### HandleConflict / HandleConflictAsync

Converts HTTP 409 responses to `ConflictError`.

```csharp
Result<HttpResponseMessage> HandleConflict(
    this HttpResponseMessage response, 
    ConflictError conflictError)

Result<HttpResponseMessage> HandleConflict(
    this Result<HttpResponseMessage> result, 
    ConflictError conflictError)

Task<Result<HttpResponseMessage>> HandleConflictAsync(
    this Task<HttpResponseMessage> responseTask, 
    ConflictError conflictError)

Task<Result<HttpResponseMessage>> HandleConflictAsync(
    this Task<Result<HttpResponseMessage>> resultTask, 
    ConflictError conflictError)
```

### Range Handlers

#### HandleClientError / HandleClientErrorAsync

Handles any HTTP client error (4xx) status codes with a custom error factory.

```csharp
Result<HttpResponseMessage> HandleClientError(
    this HttpResponseMessage response,
    Func<HttpStatusCode, Error> errorFactory)

Result<HttpResponseMessage> HandleClientError(
    this Result<HttpResponseMessage> result,
    Func<HttpStatusCode, Error> errorFactory)

Task<Result<HttpResponseMessage>> HandleClientErrorAsync(
    this Task<HttpResponseMessage> responseTask,
    Func<HttpStatusCode, Error> errorFactory)

Task<Result<HttpResponseMessage>> HandleClientErrorAsync(
    this Task<Result<HttpResponseMessage>> resultTask,
    Func<HttpStatusCode, Error> errorFactory)
```

**Example:**
```csharp
var result = httpClient.GetAsync(url, ct)
    .HandleClientErrorAsync(code => code switch
    {
        HttpStatusCode.BadRequest => Error.BadRequest("Invalid request"),
        HttpStatusCode.NotFound => Error.NotFound("Resource not found"),
        _ => Error.Unexpected($"Client error: {code}")
    });
```

#### HandleServerError / HandleServerErrorAsync

Handles any HTTP server error (5xx) status codes with a custom error factory.

```csharp
Result<HttpResponseMessage> HandleServerError(
    this HttpResponseMessage response,
    Func<HttpStatusCode, Error> errorFactory)

Result<HttpResponseMessage> HandleServerError(
    this Result<HttpResponseMessage> result,
    Func<HttpStatusCode, Error> errorFactory)

Task<Result<HttpResponseMessage>> HandleServerErrorAsync(
    this Task<HttpResponseMessage> responseTask,
    Func<HttpStatusCode, Error> errorFactory)

Task<Result<HttpResponseMessage>> HandleServerErrorAsync(
    this Task<Result<HttpResponseMessage>> resultTask,
    Func<HttpStatusCode, Error> errorFactory)
```

**Example:**
```csharp
var result = httpClient.PostAsync(url, content, ct)
    .HandleServerErrorAsync(code => Error.ServiceUnavailable($"API error: {code}"));
```

### Success Validation

#### EnsureSuccess / EnsureSuccessAsync

Ensures the HTTP response has a success status code, otherwise returns an error.
This is a functional alternative to `HttpResponseMessage.EnsureSuccessStatusCode()`.

```csharp
Result<HttpResponseMessage> EnsureSuccess(
    this HttpResponseMessage response,
    Func<HttpStatusCode, Error>? errorFactory = null)

Result<HttpResponseMessage> EnsureSuccess(
    this Result<HttpResponseMessage> result,
    Func<HttpStatusCode, Error>? errorFactory = null)

Task<Result<HttpResponseMessage>> EnsureSuccessAsync(
    this Task<HttpResponseMessage> responseTask,
    Func<HttpStatusCode, Error>? errorFactory = null)

Task<Result<HttpResponseMessage>> EnsureSuccessAsync(
    this Task<Result<HttpResponseMessage>> resultTask,
    Func<HttpStatusCode, Error>? errorFactory = null)
```

**Example:**
```csharp
// Default error
var result = await httpClient.DeleteAsync($"api/items/{id}", ct)
    .EnsureSuccessAsync();

// Custom error
var result = await httpClient.PutAsync(url, content, ct)
    .EnsureSuccessAsync(code => Error.Unexpected($"Update failed: {code}"));
```

### Custom Error Handling

#### HandleFailureAsync

Custom error handling for any non-success status code.

```csharp
Task<Result<HttpResponseMessage>> HandleFailureAsync<TContext>(
    this HttpResponseMessage response,
    Func<HttpResponseMessage, TContext, CancellationToken, Task<Error>> callbackFailedStatusCode,
    TContext context,
    CancellationToken cancellationToken)
```

### JSON Deserialization

#### ReadResultFromJsonAsync

Deserialize JSON response to `Result<T>`. Returns error if response is null.

```csharp
Task<Result<TValue>> ReadResultFromJsonAsync<TValue>(
    this HttpResponseMessage response,
    JsonTypeInfo<TValue> jsonTypeInfo,
    CancellationToken cancellationToken)
```

#### ReadResultMaybeFromJsonAsync

Deserialize JSON response to `Result<Maybe<T>>`. Null responses become `Maybe.None`.

```csharp
Task<Result<Maybe<TValue>>> ReadResultMaybeFromJsonAsync<TValue>(
    this HttpResponseMessage response,
    JsonTypeInfo<TValue> jsonTypeInfo,
    CancellationToken cancellationToken)
```

## Best Practices

1. **Separate HTTP concerns from domain logic** — HTTP infrastructure concerns are separated from core functional programming patterns
2. **Use explicit error handling** — No hidden exceptions; all errors are represented in the type system
3. **Compose handlers fluently** — All methods integrate with Railway Oriented Programming patterns
4. **Don’t overlap with resilience** — Focused on status code handling, not resilience patterns (use the .NET resilience library for retry/circuit breaker)
5. **Use `CancellationToken` consistently** — Pass tokens through async chains

## Related Packages

- [Trellis.Results](https://www.nuget.org/packages/Trellis.Results) — Core `Result<T>` and `Maybe<T>` types
- [Trellis.Asp](https://www.nuget.org/packages/Trellis.Asp) — ASP.NET Core integration (Result → HTTP responses)

## License

MIT — see [LICENSE](../LICENSE) for details.
