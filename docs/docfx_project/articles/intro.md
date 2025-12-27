# Why Use This Library?

## Functional Programming

Railway Oriented Programming controls program execution flow using success or error tracks. This enables function chaining without explicit error checking at each step.

This approach leads to:
- **Explicit error handling** - No hidden null references or exceptions
- **Composable operations** - Chain functions together naturally
- **Testable code** - Pure functions are easier to test
- **Type safety** - Compiler-enforced error handling

## Domain-Driven Design

The library provides classes for creating Aggregates, Entities, and Value Objects. Integrate with FluentValidation to ensure domain properties remain in a valid state. Use ScalarValueObject for single-value objects, or RequiredString for non-null/non-empty string validation.

**Key DDD building blocks:**
- **Aggregates** - Consistency boundaries with domain events
- **Entities** - Objects with identity
- **Value Objects** - Immutable objects defined by their values
- **Scalar Value Objects** - Single-value wrappers with validation

## Error Types

The library provides common error types for domain operations with automatic mapping to HTTP status codes.

**Built-in error types:**
- `ValidationError` - Input validation failures with detailed error information
- `NotFoundError` - Resource not found (404)
- `ForbiddenError` - Access denied (403)
- `UnauthorizedError` - Authentication required (401)
- `ConflictError` - Resource conflict (409)
- `BadRequestError` - Invalid request (400)
- `UnexpectedError` - Unexpected system error or exception (500)
- `DomainError` - Business rule violation (422)
- `RateLimitError` - Rate limit exceeded (429)
- `ServiceUnavailableError` - Service temporarily unavailable (503)
- `AggregateError` - Multiple errors combined

### Discriminated Error Matching

Match on specific error types for precise error handling:

```csharp
var result = ProcessOrder(order)
    .MatchError(
        onValidation: validationErr => 
            Results.BadRequest(new 
            { 
                errors = validationErr.FieldErrors
                    .ToDictionary(f => f.FieldName, f => f.Details.ToArray()) 
            }),
        onNotFound: notFoundErr => 
            Results.NotFound(new { message = notFoundErr.Detail }),
        onConflict: conflictErr => 
            Results.Conflict(new { message = conflictErr.Detail }),
        onSuccess: order => 
            Results.Ok(order)
    );
```

## Reuse Domain Validation at the API Layer

Domain validation rules automatically translate to HTTP standard error responses. ValidationError becomes BadRequest with detailed errors, NotFoundError becomes HTTP 404. This creates a **single source of truth**, eliminating duplication between domain and API layers.

## Pagination Support

Automatic HTTP header management with proper status codes: 200 (OK) for complete results, 206 (Partial Content) for paginated responses per [RFC 9110](https://www.rfc-editor.org/rfc/rfc9110#field.content-range).

## Avoid Primitive Obsession

Use strongly-typed value objects instead of primitive types. RequiredString provides type-safe string properties with automatic source generation. Additional value object types are available for common scenarios.

**Benefits:**
- **Type safety** - Compiler prevents parameter mix-ups
- **Self-documenting** - `FirstName` vs `string` is clearer
- **Validation once** - Create validated objects, use everywhere
- **Source generation** - Minimal boilerplate

## Async & Cancellation Support

All async operations support `CancellationToken` for graceful shutdown and request timeouts:

```csharp
await GetCustomerByIdAsync(id, cancellationToken)
   .EnsureAsync(
      (customer, ct) => customer.CanBePromotedAsync(ct),
      Error.Validation("Cannot promote"),
      cancellationToken)
   .TapAsync(
      async (customer, ct) => await customer.PromoteAsync(ct),
      cancellationToken)
   .MatchAsync(ok => "Success", error => error.Detail);
```

## Parallel Execution

Fetch data from multiple sources in parallel while maintaining Railway Oriented Programming style:

```csharp
var result = await Task.WhenAll(
        GetUserAsync(userId),
        GetOrdersAsync(userId),
        GetPreferencesAsync(userId)
    )
    .ThenAsync(results => results[0]
        .Combine(results[1])
        .Combine(results[2])
    );
```

## Performance

The library adds only **~11-16 nanoseconds** of overhead compared to imperative code - less than 0.002% of typical I/O operations. You get cleaner, more maintainable code with virtually zero performance cost.
