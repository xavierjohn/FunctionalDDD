# Error Handling

This guide covers error types, discriminated matching, and transformation patterns in Railway Oriented Programming.

```csharp
using FunctionalDdd;
using System.Collections.Immutable;
```

## Table of Contents

- [Error Types](#error-types)
- [Creating Errors](#creating-errors)
- [Discriminated Error Matching](#discriminated-error-matching)
- [Error Transformation](#error-transformation)
- [Aggregate Errors](#aggregate-errors)
- [Custom Error Types](#custom-error-types)

## Error Types

Built-in error types map to HTTP status codes and common business scenarios:

| Error Type | HTTP Status | Use Case | Example |
|------------|-------------|----------|---------|
| `ValidationError` | 400 Bad Request | Input validation failures | Invalid email format, required field missing |
| `NotFoundError` | 404 Not Found | Resource doesn't exist | User not found, order not found |
| `UnauthorizedError` | 401 Unauthorized | Authentication required | Missing token, invalid credentials |
| `ForbiddenError` | 403 Forbidden | Insufficient permissions | User cannot access resource |
| `ConflictError` | 409 Conflict | Resource state conflict | Duplicate email, concurrent update |
| `BadRequestError` | 400 Bad Request | General request errors | Malformed request |
| `UnexpectedError` | 500 Internal Server Error | System errors | Database connection failed |
| `DomainError` | 422 Unprocessable Entity | Business rule violation | Cannot withdraw more than balance |
| `RateLimitError` | 429 Too Many Requests | Rate limit exceeded | Too many login attempts |
| `ServiceUnavailableError` | 503 Service Unavailable | Service temporarily down | Service under maintenance |
| `AggregateError` | Varies | Multiple errors combined | Multiple validation failures |

### Error Structure

All errors share a common structure:

```csharp
public class Error
{
    public string Code { get; }
    public string Detail { get; }
    public string? Instance { get; }
}

// Factory methods create specific error types:
Error.Validation(...)     // ValidationError
Error.NotFound(...)       // NotFoundError
Error.Conflict(...)       // ConflictError
Error.BadRequest(...)     // BadRequestError
Error.Unauthorized(...)   // UnauthorizedError
Error.Forbidden(...)      // ForbiddenError
Error.Unexpected(...)     // UnexpectedError
Error.Domain(...)         // DomainError
Error.RateLimit(...)      // RateLimitError
Error.ServiceUnavailable(...) // ServiceUnavailableError
```

## Creating Errors

### Validation Errors

```csharp
// Simple validation error
var error = Error.Validation("Email is required", "email");

// With custom error code
var error = Error.Validation(
    fieldDetail: "Email format is invalid",
    fieldName: "email",
    detail: "Validation failed",
    instance: null
);

// Multiple field validation errors
var fieldErrors = ImmutableArray.Create(
    new FieldError("email", ImmutableArray.Create("Email is required", "Email format invalid")),
    new FieldError("age", ImmutableArray.Create("Must be 18 or older"))
);
var error = Error.Validation(fieldErrors, "Validation failed");
```

### Not Found Errors

```csharp
// Simple not found
var error = Error.NotFound($"User {userId} not found");

// With instance identifier
var error = Error.NotFound(
    $"User with ID {userId} does not exist",
    instance: userId.ToString()
);
```

### Authorization Errors

```csharp
// Unauthorized (authentication required)
var error = Error.Unauthorized("Authentication token missing");

// Forbidden (insufficient permissions)
var error = Error.Forbidden(
    "User does not have permission to delete orders"
);
```

### Conflict Errors

```csharp
// Resource conflict
var error = Error.Conflict(
    $"Email {email} is already registered"
);

// Concurrent update conflict
var error = Error.Conflict(
    "Resource was modified by another user"
);
```

### Unexpected Errors

```csharp
// System errors
var error = Error.Unexpected("Database connection failed");

// From exception
try
{
    // risky operation
}
catch (Exception ex)
{
    return Error.Unexpected(
        "SYSTEM_ERROR",
        "An unexpected error occurred",
        new Dictionary<string, object>
        {
            ["Exception"] = ex.GetType().Name,
            ["Message"] = ex.Message
        }
    );
}
```

## Discriminated Error Matching

The `MatchError` method allows you to handle different error types with specific logic:

### Basic Error Matching

```csharp
var httpResult = ProcessOrder(order)
    .MatchError(
        onValidation: validationErr => 
            Results.BadRequest(new { 
                errors = validationErr.FieldErrors.ToDictionary(f => f.FieldName, f => f.Details.ToArray())
            }),
        onNotFound: notFoundErr => 
            Results.NotFound(new { message = notFoundErr.Detail }),
        onConflict: conflictErr => 
            Results.Conflict(new { message = conflictErr.Detail }),
        onSuccess: order => 
            Results.Ok(order)
    );
```

### Complete Error Matching

```csharp
return await ProcessTransactionAsync(transaction)
    .MatchError(
        onValidation: err => 
            Results.BadRequest(new { 
                message = err.Detail,
                errors = err.FieldErrors.ToDictionary(f => f.FieldName, f => f.Details.ToArray())
            }),
        onNotFound: err => 
            Results.NotFound(new { message = err.Detail }),
        onUnauthorized: err => 
            Results.Unauthorized(),
        onForbidden: err => 
            Results.StatusCode(403),
        onConflict: err => 
            Results.Conflict(new { message = err.Detail }),
        onUnexpected: err => 
            Results.StatusCode(500),
        onSuccess: transaction => 
            Results.Ok(new { transactionId = transaction.Id })
    );
```

### Partial Error Matching

You don't need to handle every error type - only the ones relevant to your scenario:

```csharp
var result = CreateUser(userData)
    .MatchError(
        onValidation: err => Results.BadRequest(err.FieldErrors),
        onConflict: err => Results.Conflict(err.Detail),
        // All other errors handled by default
        onSuccess: user => Results.Created($"/users/{user.Id}", user)
    );
```

## Error Transformation

Transform errors as they flow through your pipeline:

### MapError - Transform Error Types

```csharp
var result = GetUserFromExternalApi(userId)
    .MapError(error => error switch
    {
        NotFoundError => Error.NotFound(
            "User not found in our system",
            "USER_NOT_IN_SYSTEM"
        ),
        UnexpectedError => Error.ServiceUnavailable(
            "External service is temporarily unavailable"
        ),
        _ => error
    });
```

### Add Context to Errors

```csharp
var result = ProcessPayment(order)
    .MapError(error => Error.Unexpected(
        $"Payment processing failed for order {order.Id}: {error.Detail}",
        $"order-{order.Id}"
    ));
```

### Compensate - Error Recovery

```csharp
var result = GetUserFromCache(userId)
    .Compensate(cacheError => 
        GetUserFromDatabase(userId)
            .MapError(dbError => Error.NotFound(
                $"User {userId} not found. Cache: {cacheError.Detail}, DB: {dbError.Detail}",
                $"user-{userId}"
            ))
    );
```

## Aggregate Errors

When combining multiple Results, validation errors are automatically aggregated:

### Automatic Aggregation

```csharp
var result = EmailAddress.TryCreate(email)
    .Combine(FirstName.TryCreate(firstName))
    .Combine(LastName.TryCreate(lastName));

// If multiple failures occur, result contains an AggregateError with all errors
if (result.IsFailure && result.Error is AggregateError aggregateError)
{
    foreach (var error in aggregateError.Errors)
    {
        Console.WriteLine($"- {error.Detail}");
    }
}
```

### Manual Error Aggregation

```csharp
var errors = new List<Error>();

if (string.IsNullOrEmpty(email))
    errors.Add(Error.Validation("Email is required"));

if (age < 18)
    errors.Add(Error.Validation("Must be 18 or older"));

if (errors.Any())
    return Result.Failure<User>(new AggregateError(errors));

return Result.Success(new User(email, age));
```

### Flattening Aggregate Errors

```csharp
public static Error FlattenErrors(Error error)
{
    if (error is not AggregateError aggregateError)
        return error;

    var allErrors = new List<Error>();

    foreach (var err in aggregateError.Errors)
    {
        if (err is AggregateError nested)
            allErrors.AddRange(((AggregateError)FlattenErrors(nested)).Errors);
        else
            allErrors.Add(err);
    }

    return new AggregateError(allErrors);
}
```

## Custom Error Types

While the built-in error types cover most scenarios, you can extend the system:

### Custom Error Factory Methods

```csharp
public static class CustomErrors
{
    public static RateLimitError RateLimitExceeded(int retryAfterSeconds)
    {
        return Error.RateLimit(
            $"Too many requests. Please try again after {retryAfterSeconds} seconds.",
            "RATE_LIMIT_EXCEEDED"
        );
    }

    public static DomainError PaymentDeclined(string reason)
    {
        return Error.Domain(
            $"Payment declined: {reason}",
            "PAYMENT_DECLINED"
        );
    }
}

// Usage
if (requestCount > limit)
    return Result.Failure<Response>(
        CustomErrors.RateLimitExceeded(retryAfterSeconds: 60)
    );
```

### Domain-Specific Errors

```csharp
public static class OrderErrors
{
    public static Error InsufficientInventory(ProductId productId, int requested, int available)
    {
        return Error.Conflict(
            "INSUFFICIENT_INVENTORY",
            $"Product {productId} has insufficient inventory",
            new Dictionary<string, object>
            {
                ["ProductId"] = productId.Value,
                ["Requested"] = requested,
                ["Available"] = available
            }
        );
    }

    public static Error OrderAlreadyShipped(OrderId orderId)
    {
        return Error.Conflict(
            "ORDER_ALREADY_SHIPPED",
            $"Order {orderId} has already been shipped and cannot be modified"
        );
    }
}
```

## Best Practices

1. **Use Specific Error Types**: Choose the most specific error type (NotFound vs Validation)
2. **Include Context**: Add metadata with relevant details for debugging
3. **Consistent Error Codes**: Use consistent, meaningful error codes across your app
4. **Handle Errors at Boundaries**: Use MatchError at API boundaries to convert to HTTP responses
5. **Don't Swallow Errors**: Always propagate or handle errors explicitly
6. **Use Aggregate for Multiple Errors**: Return all validation errors at once, not just the first one
7. **Add Tracing IDs**: Include correlation IDs in error metadata for distributed tracing

## Next Steps

- Learn about [Async & Cancellation](async-cancellation.md) for async error handling
- See [Integration](integration.md) for converting errors to HTTP responses
- Check [Debugging](debugging.md) for troubleshooting error scenarios
