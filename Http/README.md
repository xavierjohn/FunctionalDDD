# FunctionalDDD.Http

HTTP client extensions for Railway Oriented Programming with Result and Maybe monads.

## Overview

This library provides fluent extension methods for working with `HttpResponseMessage` in a functional style, integrating seamlessly with the Railway Oriented Programming patterns from `FunctionalDdd.RailwayOrientedProgramming`.

## Features

- **Specific Status Code Handling**: Handle 401 Unauthorized, 403 Forbidden, 409 Conflict
- **Range-based Error Handling**: Handle all 4xx client errors or 5xx server errors at once
- **EnsureSuccess**: Functional alternative to `EnsureSuccessStatusCode()` that returns Result
- **Error Handling for HTTP Status Codes**: Handle specific status codes (404 Not Found) functionally
- **Custom Error Handling**: Flexible callbacks for failed HTTP responses
- **JSON Deserialization**: Native support for deserializing to `Result<T>` and `Maybe<T>`
- **Fluent Composition**: Chain HTTP operations with Railway Oriented Programming patterns
- **Async-First**: All methods support asynchronous workflows with proper cancellation token support
- **AOT Compatible**: Fully compatible with Native AOT compilation

## Installation

```bash
dotnet add package FunctionalDdd.Http
```

## Usage

### Basic Error Handling

```csharp
using FunctionalDdd;

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
var result = await httpClient.PostAsync("api/admin/users", content, ct)
    .HandleUnauthorizedAsync(Error.Unauthorized("Please login"))
    .HandleForbiddenAsync(Error.Forbidden("Admin access required"))
    .HandleConflictAsync(Error.Conflict("Username already exists"))
    .ReadResultFromJsonAsync(UserJsonContext.Default.User, ct);

// Each handler only intercepts its specific status code
// - 401 ? UnauthorizedError
// - 403 ? ForbiddenError
// - 409 ? ConflictError
// - Other codes pass through to next handler
```

### Handle Error Ranges

```csharp
// Handle all client errors (4xx) or server errors (5xx) at once
var result = await httpClient.GetAsync("api/data", ct)
    .HandleClientErrorAsync(code => Error.BadRequest($"Client error: {code}"))
    .HandleServerErrorAsync(code => Error.ServiceUnavailable($"Server error: {code}"))
    .ReadResultFromJsonAsync(DataJsonContext.Default.Data, ct);

// Client errors (400-499) ? Custom error via factory
// Server errors (500+) ? Custom error via factory
// Success codes ? Continue to JSON deserialization
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
var result = await httpClient.PostAsync("api/orders", orderContent, ct)
    .HandleUnauthorizedAsync(Error.Unauthorized("Please login to place orders"))
    .HandleForbiddenAsync(Error.Forbidden("Your account cannot place orders"))
    .HandleConflictAsync(Error.Conflict("Order already exists"))
    .HandleClientErrorAsync(code => Error.BadRequest($"Invalid order data: {code}"))
    .HandleServerErrorAsync(code => Error.ServiceUnavailable($"Order service unavailable: {code}"))
    .ReadResultFromJsonAsync(OrderJsonContext.Default.Order, ct)
    .TapAsync(order => _logger.LogInformation("Order {OrderId} created", order.Id));

// Handlers are evaluated in order - first match wins
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

Task<Result<HttpResponseMessage>> HandleNotFoundAsync(
    this Task<HttpResponseMessage> responseTask, 
    NotFoundError notFoundError)
```

#### HandleUnauthorized / HandleUnauthorizedAsync

Converts HTTP 401 responses to `UnauthorizedError`.

```csharp
Result<HttpResponseMessage> HandleUnauthorized(
    this HttpResponseMessage response, 
    UnauthorizedError unauthorizedError)

Task<Result<HttpResponseMessage>> HandleUnauthorizedAsync(
    this Task<HttpResponseMessage> responseTask, 
    UnauthorizedError unauthorizedError)
```

#### HandleForbidden / HandleForbiddenAsync

Converts HTTP 403 responses to `ForbiddenError`.

```csharp
Result<HttpResponseMessage> HandleForbidden(
    this HttpResponseMessage response, 
    ForbiddenError forbiddenError)

Task<Result<HttpResponseMessage>> HandleForbiddenAsync(
    this Task<HttpResponseMessage> responseTask, 
    ForbiddenError forbiddenError)
```

#### HandleConflict / HandleConflictAsync

Converts HTTP 409 responses to `ConflictError`.

```csharp
Result<HttpResponseMessage> HandleConflict(
    this HttpResponseMessage response, 
    ConflictError conflictError)

Task<Result<HttpResponseMessage>> HandleConflictAsync(
    this Task<HttpResponseMessage> responseTask, 
    ConflictError conflictError)
```

### Range Handlers

#### HandleClientError / HandleClientErrorAsync

Handles any HTTP client error (4xx) status codes with a custom error factory.

```csharp
Result<HttpResponseMessage> HandleClientError(
    this HttpResponseMessage response,
    Func<HttpStatusCode, Error> errorFactory)

Task<Result<HttpResponseMessage>> HandleClientErrorAsync(
    this Task<HttpResponseMessage> responseTask,
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

Task<Result<HttpResponseMessage>> HandleServerErrorAsync(
    this Task<HttpResponseMessage> responseTask,
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

Task<Result<HttpResponseMessage>> EnsureSuccessAsync(
    this Task<HttpResponseMessage> responseTask,
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

## Design Principles

This library follows these design principles:

1. **Separation of Concerns**: HTTP infrastructure concerns are separated from core functional programming patterns
2. **Dependency Inversion**: Infrastructure depends on core abstractions, not vice versa
3. **Composability**: All methods integrate with Railway Oriented Programming patterns
4. **Explicit Error Handling**: No hidden exceptions; all errors are represented in the type system
5. **No Polly Overlap**: Focused on status code handling, not resilience patterns (use Polly for retry/circuit breaker)

## Related Packages

- **FunctionalDdd.RailwayOrientedProgramming**: Core Result and Maybe monads
- **FunctionalDdd.Asp**: ASP.NET Core integration (converts Result to ActionResult/IResult)

## License

MIT

## Contributing

Contributions are welcome! Please see the main repository for contribution guidelines.
