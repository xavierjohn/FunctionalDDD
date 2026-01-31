# HTTP Client Integration

**Level:** Beginner | **Time:** 20-30 minutes

Learn how to use HttpClient with Railway-Oriented Programming for functional HTTP communication with automatic error handling.

## Overview

The `FunctionalDdd.Http` package provides extension methods for `HttpResponseMessage` that integrate seamlessly with Railway-Oriented Programming patterns. Instead of dealing with exceptions and manual status code checks, you get clean, composable operations that return `Result<T>`.

**What you'll learn:**
- ✅ Handle specific HTTP status codes (401, 403, 404, 409) functionally
- ✅ Handle error ranges (all 4xx or 5xx) with custom factories
- ✅ Deserialize JSON responses to `Result<T>` or `Result<Maybe<T>>`
- ✅ Chain HTTP operations with other ROP operations
- ✅ Work with CancellationToken for proper cancellation support

## Installation

```bash
dotnet add package FunctionalDdd.Http
```

## Quick Start

### Basic JSON Deserialization

```csharp
using FunctionalDdd;
using System.Net.Http.Json;

// Define your JSON context for AOT compatibility
[JsonSerializable(typeof(User))]
internal partial class UserJsonContext : JsonSerializerContext { }

public async Task<Result<User>> GetUserAsync(string userId, CancellationToken ct)
{
    return await _httpClient.GetAsync($"api/users/{userId}", ct)
        .HandleNotFoundAsync(Error.NotFound($"User {userId} not found"))
        .ReadResultFromJsonAsync(UserJsonContext.Default.User, ct);
}
```

### Handle Multiple Status Codes

```csharp
public async Task<Result<Order>> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct)
{
    return await _httpClient.PostAsJsonAsync("api/orders", request, ct)
        .HandleUnauthorizedAsync(Error.Unauthorized("Please login to create orders"))
        .HandleForbiddenAsync(Error.Forbidden("You don't have permission to create orders"))
        .HandleConflictAsync(Error.Conflict("Order already exists"))
        .ReadResultFromJsonAsync(OrderJsonContext.Default.Order, ct);
}
```

## Status Code Handlers

### Specific Status Code Handlers

The library provides handlers for common HTTP status codes that map to specific error types:

| Handler | Status Code | Error Type | Use Case |
|---------|-------------|------------|----------|
| `HandleNotFound` | 404 | `NotFoundError` | Resource doesn't exist |
| `HandleUnauthorized` | 401 | `UnauthorizedError` | Authentication required |
| `HandleForbidden` | 403 | `ForbiddenError` | Insufficient permissions |
| `HandleConflict` | 409 | `ConflictError` | Resource already exists or state conflict |

**Example:**

```csharp
var result = await _httpClient.GetAsync($"api/products/{productId}", ct)
    .HandleNotFoundAsync(Error.NotFound("Product", productId))
    .HandleUnauthorizedAsync(Error.Unauthorized("Please login"))
    .ReadResultFromJsonAsync(ProductJsonContext.Default.Product, ct);

// Result will be:
// - Success<Product> if status is 200 and JSON deserializes
// - Failure<NotFoundError> if status is 404
// - Failure<UnauthorizedError> if status is 401
// - Success with response if other status codes (passes through)
```

### Range-Based Handlers

Handle entire ranges of status codes with custom error factories:

#### HandleClientError (4xx)

Handles all client error responses (400-499):

```csharp
var result = await _httpClient.PostAsJsonAsync("api/orders", order, ct)
    .HandleClientErrorAsync(statusCode => statusCode switch
    {
        HttpStatusCode.BadRequest => Error.BadRequest("Invalid order data"),
        HttpStatusCode.NotFound => Error.NotFound("Endpoint not found"),
        HttpStatusCode.Conflict => Error.Conflict("Order already exists"),
        _ => Error.Unexpected($"Client error: {statusCode}")
    })
    .ReadResultFromJsonAsync(OrderJsonContext.Default.Order, ct);
```

#### HandleServerError (5xx)

Handles all server error responses (500+):

```csharp
var result = await _httpClient.GetAsync("api/data", ct)
    .HandleServerErrorAsync(statusCode => 
        Error.ServiceUnavailable($"API is experiencing issues: {statusCode}"))
    .ReadResultFromJsonAsync(DataJsonContext.Default.Data, ct);
```

### EnsureSuccess

Functional alternative to `HttpResponseMessage.EnsureSuccessStatusCode()` that returns a `Result` instead of throwing an exception:

```csharp
// Default error for non-success status codes
var result = await _httpClient.DeleteAsync($"api/items/{id}", ct)
    .EnsureSuccessAsync()
    .TapAsync(response => _logger.LogInformation("Deleted item {Id}", id));

// Custom error factory
var result = await _httpClient.PutAsJsonAsync($"api/users/{userId}", updateData, ct)
    .EnsureSuccessAsync(statusCode => 
        Error.Unexpected($"Update failed with status {statusCode}"))
    .ReadResultFromJsonAsync(UserJsonContext.Default.User, ct);
```

## JSON Deserialization

### ReadResultFromJsonAsync

Deserializes JSON to `Result<T>`. Returns an error if the response body is null:

```csharp
public async Task<Result<User>> GetUserAsync(string userId, CancellationToken ct)
{
    return await _httpClient.GetAsync($"api/users/{userId}", ct)
        .ReadResultFromJsonAsync(UserJsonContext.Default.User, ct);
}

// If response is 200 with JSON body → Success<User>
// If response is 200 with null body → Failure<UnexpectedError>
// If response is non-success (4xx, 5xx) → Failure<UnexpectedError>
```

### ReadResultMaybeFromJsonAsync

Deserializes JSON to `Result<Maybe<T>>`. Null responses become `Maybe.None` instead of errors:

```csharp
public async Task<Result<Maybe<Profile>>> GetOptionalProfileAsync(string userId, CancellationToken ct)
{
    return await _httpClient.GetAsync($"api/users/{userId}/profile", ct)
        .ReadResultMaybeFromJsonAsync(ProfileJsonContext.Default.Profile, ct)
        .TapAsync(maybe =>
        {
            if (maybe.HasValue)
                _logger.LogInformation("Profile found: {Name}", maybe.Value.Name);
            else
                _logger.LogInformation("No profile available");
        });
}

// If response is 200 with JSON body → Success<Maybe<Profile>> with value
// If response is 200 with null body → Success<Maybe<Profile>> with no value
// If response is non-success (4xx, 5xx) → Failure<UnexpectedError>
```

## Composing HTTP Calls

### Chaining Multiple Status Handlers

```csharp
public async Task<Result<Order>> PlaceOrderAsync(
    CreateOrderRequest request,
    CancellationToken ct)
{
    return await _httpClient.PostAsJsonAsync("api/orders", request, ct)
        .HandleUnauthorizedAsync(Error.Unauthorized("Please login to place orders"))
        .HandleForbiddenAsync(Error.Forbidden("Your account cannot place orders"))
        .HandleConflictAsync(Error.Conflict("Order already exists"))
        .HandleClientErrorAsync(code => Error.BadRequest($"Invalid order data: {code}"))
        .HandleServerErrorAsync(code => Error.ServiceUnavailable($"Order service unavailable: {code}"))
        .ReadResultFromJsonAsync(OrderJsonContext.Default.Order, ct)
        .TapAsync(order => _logger.LogInformation("Order {OrderId} created", order.Id));
}

// Handlers are evaluated in order - first match wins
// Remaining handlers are skipped once a status code matches
```

### Integration with Railway-Oriented Programming

HTTP calls compose naturally with other ROP operations:

```csharp
public async Task<Result<OrderConfirmation>> ProcessOrderWorkflowAsync(
    string orderId,
    CancellationToken ct)
{
    return await _httpClient.GetAsync($"api/orders/{orderId}", ct)
        .HandleNotFoundAsync(Error.NotFound("Order", orderId))
        .HandleUnauthorizedAsync(Error.Unauthorized("Please login"))
        .ReadResultFromJsonAsync(OrderJsonContext.Default.Order, ct)
        .EnsureAsync(order => order.Status == "Pending",
            Error.Validation("Only pending orders can be processed"))
        .BindAsync((order, token) => ValidateInventoryAsync(order, token), ct)
        .BindAsync((order, token) => ProcessPaymentAsync(order, token), ct)
        .TapAsync((order, token) => SendConfirmationEmailAsync(order, token), ct)
        .MapAsync(order => new OrderConfirmation(order.Id, order.Total));
}
```

### Parallel HTTP Calls

Fetch data from multiple endpoints in parallel:

```csharp
public async Task<Result<Dashboard>> GetDashboardAsync(string userId, CancellationToken ct)
{
    var userTask = _httpClient.GetAsync($"api/users/{userId}", ct)
        .ReadResultFromJsonAsync(UserJsonContext.Default.User, ct);

    var ordersTask = _httpClient.GetAsync($"api/users/{userId}/orders", ct)
        .ReadResultFromJsonAsync(OrderListJsonContext.Default.OrderList, ct);

    var preferencesTask = _httpClient.GetAsync($"api/users/{userId}/preferences", ct)
        .ReadResultMaybeFromJsonAsync(PreferencesJsonContext.Default.Preferences, ct);

    return await userTask
        .ParallelAsync(ordersTask)
        .ParallelAsync(preferencesTask)
        .AwaitAsync()
        .MapAsync((user, orders, preferencesAsync) => new Dashboard(
            user,
            orders,
            preferences.GetValueOrDefault(Preferences.Default)));
}
```

## Custom Error Handling

### HandleFailureAsync

For complex error handling scenarios where you need to inspect the response:

```csharp
public async Task<Result<Order>> CreateOrderWithCustomErrorsAsync(
    CreateOrderRequest request,
    CancellationToken ct)
{
    return await _httpClient.PostAsJsonAsync("api/orders", request, ct)
        .HandleFailureAsync(
            async (response, context, cancellationToken) =>
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var statusCode = response.StatusCode;

                return statusCode switch
                {
                    HttpStatusCode.BadRequest => Error.Validation($"Invalid order: {errorBody}"),
                    HttpStatusCode.Conflict => Error.Conflict("Duplicate order detected"),
                    HttpStatusCode.ServiceUnavailable => Error.ServiceUnavailable("Order service is down"),
                    _ => Error.Unexpected($"Order creation failed ({statusCode}): {errorBody}")
                };
            },
            context: null,
            ct)
        .ReadResultFromJsonAsync(OrderJsonContext.Default.Order, ct);
}
```

## Best Practices

### 1. Use JSON Source Generators

Always use JSON source generators for AOT compatibility and better performance:

```csharp
// Define once per assembly
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(Order))]
[JsonSerializable(typeof(Product))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class AppJsonContext : JsonSerializerContext { }

// Use everywhere
var user = await _httpClient.GetAsync("api/users/123", ct)
    .ReadResultFromJsonAsync(AppJsonContext.Default.User, ct);
```

### 2. Handle Expected Errors Explicitly

Use specific handlers for expected error scenarios:

```csharp
// ✅ Good - Explicit handling of expected errors
var result = await _httpClient.GetAsync($"api/users/{userId}", ct)
    .HandleNotFoundAsync(Error.NotFound("User not found"))
    .HandleUnauthorizedAsync(Error.Unauthorized("Please login"))
    .ReadResultFromJsonAsync(UserJsonContext.Default.User, ct);

// ❌ Avoid - Generic catch-all for expected scenarios
var result = await _httpClient.GetAsync($"api/users/{userId}", ct)
    .EnsureSuccessAsync()  // Too broad
    .ReadResultFromJsonAsync(UserJsonContext.Default.User, ct);
```

### 3. Always Pass CancellationToken

Support graceful cancellation and timeouts:

```csharp
public async Task<Result<Data>> FetchDataAsync(string id, CancellationToken ct)
{
    return await _httpClient.GetAsync($"api/data/{id}", ct)
        .HandleNotFoundAsync(Error.NotFound("Data", id))
        .ReadResultFromJsonAsync(DataJsonContext.Default.Data, ct)
        .TapAsync((data, token) => CacheDataAsync(data, token), ct);  // Pass through
}
```

### 4. Use Range Handlers for Fallbacks

Catch unexpected client/server errors after specific handlers:

```csharp
var result = await _httpClient.PostAsJsonAsync("api/orders", order, ct)
    .HandleConflictAsync(Error.Conflict("Order exists"))  // Specific
    .HandleUnauthorizedAsync(Error.Unauthorized("Login required"))  // Specific
    .HandleClientErrorAsync(code => Error.BadRequest($"Client error: {code}"))  // Catch-all 4xx
    .HandleServerErrorAsync(code => Error.ServiceUnavailable($"Server error: {code}"))  // Catch-all 5xx
    .ReadResultFromJsonAsync(OrderJsonContext.Default.Order, ct);
```

### 5. Combine with Retry Policies

Use Polly for retry logic (don't reinvent it):

```csharp
using Polly;
using Polly.Extensions.Http;

// Configure HttpClient with Polly
services.AddHttpClient<IOrderService, OrderService>()
    .AddPolicyHandler(GetRetryPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

// Then use FunctionalDDD for functional error handling
public async Task<Result<Order>> GetOrderAsync(string orderId, CancellationToken ct)
{
    return await _httpClient.GetAsync($"api/orders/{orderId}", ct)  // Polly handles retries
        .HandleNotFoundAsync(Error.NotFound("Order", orderId))  // FunctionalDDD handles errors
        .ReadResultFromJsonAsync(OrderJsonContext.Default.Order, ct);
}
```

## Complete Example

Here's a complete service that demonstrates all HTTP integration patterns:

```csharp
using FunctionalDdd;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(Order))]
[JsonSerializable(typeof(OrderList))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class ApiJsonContext : JsonSerializerContext { }

public class OrderApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderApiClient> _logger;

    public OrderApiClient(HttpClient httpClient, ILogger<OrderApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Get user with specific error handling
    /// </summary>
    public async Task<Result<User>> GetUserAsync(string userId, CancellationToken ct)
    {
        return await _httpClient.GetAsync($"api/users/{userId}", ct)
            .HandleNotFoundAsync(Error.NotFound($"User {userId} not found"))
            .HandleUnauthorizedAsync(Error.Unauthorized("Authentication required"))
            .ReadResultFromJsonAsync(ApiJsonContext.Default.User, ct)
            .TapAsync(user => _logger.LogInformation("Retrieved user: {UserId}", user.Id));
    }

    /// <summary>
    /// Create order with comprehensive error handling
    /// </summary>
    public async Task<Result<Order>> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct)
    {
        return await _httpClient.PostAsJsonAsync("api/orders", request, ApiJsonContext.Default.CreateOrderRequest, ct)
            .HandleUnauthorizedAsync(Error.Unauthorized("Please login to create orders"))
            .HandleForbiddenAsync(Error.Forbidden("Your account cannot place orders"))
            .HandleConflictAsync(Error.Conflict("Order already exists"))
            .HandleClientErrorAsync(code => Error.BadRequest($"Invalid order data: {code}"))
            .HandleServerErrorAsync(code => Error.ServiceUnavailable($"Order service unavailable: {code}"))
            .ReadResultFromJsonAsync(ApiJsonContext.Default.Order, ct)
            .TapAsync(order => _logger.LogInformation("Order {OrderId} created", order.Id));
    }

    /// <summary>
    /// Get optional profile using Maybe pattern
    /// </summary>
    public async Task<Result<Maybe<UserProfile>>> GetOptionalProfileAsync(string userId, CancellationToken ct)
    {
        return await _httpClient.GetAsync($"api/users/{userId}/profile", ct)
            .ReadResultMaybeFromJsonAsync(ApiJsonContext.Default.UserProfile, ct)
            .TapAsync(maybe =>
            {
                if (maybe.HasValue)
                    _logger.LogInformation("Profile found for user {UserId}", userId);
                else
                    _logger.LogInformation("No profile for user {UserId}", userId);
            });
    }

    /// <summary>
    /// Complex workflow with multiple operations
    /// </summary>
    public async Task<Result<OrderConfirmation>> ProcessOrderWorkflowAsync(
        string orderId,
        CancellationToken ct)
    {
        return await GetOrderAsync(orderId, ct)
            .EnsureAsync(order => order.Status == "Pending",
                Error.Validation("Only pending orders can be processed"))
            .BindAsync((order, token) => ValidateInventoryAsync(order, token), ct)
            .BindAsync((order, token) => ProcessPaymentAsync(order, token), ct)
            .TapAsync((order, token) => SendConfirmationEmailAsync(order, token), ct)
            .MapAsync(order => new OrderConfirmation(order.Id, order.Total));
    }

    private async Task<Result<Order>> GetOrderAsync(string orderId, CancellationToken ct)
    {
        return await _httpClient.GetAsync($"api/orders/{orderId}", ct)
            .HandleNotFoundAsync(Error.NotFound("Order", orderId))
            .ReadResultFromJsonAsync(ApiJsonContext.Default.Order, ct);
    }

    private async Task<Result<Order>> ValidateInventoryAsync(Order order, CancellationToken ct)
    {
        return await _httpClient.PostAsJsonAsync($"api/orders/{order.Id}/validate-inventory", order, ct)
            .HandleConflictAsync(Error.Conflict("Insufficient inventory"))
            .ReadResultFromJsonAsync(ApiJsonContext.Default.Order, ct);
    }

    private async Task<Result<Order>> ProcessPaymentAsync(Order order, CancellationToken ct)
    {
        return await _httpClient.PostAsJsonAsync($"api/orders/{order.Id}/process-payment", order, ct)
            .HandleClientErrorAsync(code => Error.BadRequest("Payment failed"))
            .ReadResultFromJsonAsync(ApiJsonContext.Default.Order, ct);
    }

    private async Task SendConfirmationEmailAsync(Order order, CancellationToken ct)
    {
        await _httpClient.PostAsync($"api/notifications/order-confirmation/{order.Id}", null, ct);
    }
}
```

## Comparison with Traditional Approach

### Before (Traditional Exception-Based)

```csharp
public async Task<User> GetUserAsync(string userId, CancellationToken ct)
{
    try
    {
        var response = await _httpClient.GetAsync($"api/users/{userId}", ct);
        
        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException($"User {userId} not found");
            
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedException("Please login");
            
        response.EnsureSuccessStatusCode();  // Throws for other errors
        
        var user = await response.Content.ReadFromJsonAsync<User>(ct);
        
        if (user == null)
            throw new InvalidOperationException("Response was null");
            
        _logger.LogInformation("Retrieved user: {UserId}", user.Id);
        return user;
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "HTTP request failed");
        throw;
    }
    catch (JsonException ex)
    {
        _logger.LogError(ex, "JSON deserialization failed");
        throw;
    }
}
```

### After (Functional with Result)

```csharp
public async Task<Result<User>> GetUserAsync(string userId, CancellationToken ct)
{
    return await _httpClient.GetAsync($"api/users/{userId}", ct)
        .HandleNotFoundAsync(Error.NotFound($"User {userId} not found"))
        .HandleUnauthorizedAsync(Error.Unauthorized("Please login"))
        .ReadResultFromJsonAsync(UserJsonContext.Default.User, ct)
        .TapAsync(user => _logger.LogInformation("Retrieved user: {UserId}", user.Id));
}
```

**Benefits:**
- ✅ **No exceptions** - All errors are values in the type system
- ✅ **Composable** - Chain with other ROP operations
- ✅ **Explicit** - Return type shows this can fail
- ✅ **Concise** - 60% less code
- ✅ **Type-safe** - Compiler enforces error handling

## Next Steps

1. **Explore** [ASP.NET Core Integration](integration-aspnet.md) to convert Result back to HTTP responses
2. **Learn** [Error Handling](error-handling.md) for working with different error types
3. **Master** [Working with Async Operations](basics.md#working-with-async-operations) for proper cancellation support
4. **See** [Examples](examples.md) for more real-world patterns

## API Reference

For complete API documentation, see:
- [Package README](https://www.nuget.org/packages/FunctionalDdd.Http)
- Browse the API reference in the documentation site
