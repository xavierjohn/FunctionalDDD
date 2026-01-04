# FunctionalDdd.Http

HTTP client extensions for Railway Oriented Programming with Result and Maybe monads.

## Overview

This library provides fluent extension methods for working with `HttpResponseMessage` in a functional style, integrating seamlessly with the Railway Oriented Programming patterns from `FunctionalDdd.RailwayOrientedProgramming`.

## Features

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

### HandleNotFound / HandleNotFoundAsync

Converts HTTP 404 responses to `NotFoundError`.

```csharp
Result<HttpResponseMessage> HandleNotFound(
    this HttpResponseMessage response, 
    NotFoundError notFoundError)

Task<Result<HttpResponseMessage>> HandleNotFoundAsync(
    this Task<HttpResponseMessage> responseTask, 
    NotFoundError notFoundError)
```

### HandleFailureAsync

Custom error handling for any non-success status code.

```csharp
Task<Result<HttpResponseMessage>> HandleFailureAsync<TContext>(
    this HttpResponseMessage response,
    Func<HttpResponseMessage, TContext, CancellationToken, Task<Error>> callbackFailedStatusCode,
    TContext context,
    CancellationToken cancellationToken)
```

### ReadResultFromJsonAsync

Deserialize JSON response to `Result<T>`. Returns error if response is null.

```csharp
Task<Result<TValue>> ReadResultFromJsonAsync<TValue>(
    this HttpResponseMessage response,
    JsonTypeInfo<TValue> jsonTypeInfo,
    CancellationToken cancellationToken)
```

### ReadResultMaybeFromJsonAsync

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

## Related Packages

- **FunctionalDdd.RailwayOrientedProgramming**: Core Result and Maybe monads
- **FunctionalDdd.Asp**: ASP.NET Core integration (converts Result to ActionResult/IResult)

## License

MIT

## Contributing

Contributions are welcome! Please see the main repository for contribution guidelines.
