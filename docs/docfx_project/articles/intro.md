# Introduction

Learn why functional domain modeling with Railway-Oriented Programming makes your code cleaner, safer, and more maintainable.

## Why Use This Library?

Building robust applications requires explicit error handling, type safety, and clean code. This library combines **Railway-Oriented Programming** with **Domain-Driven Design** to achieve all three—without sacrificing performance or readability.

## Functional Programming

Railway Oriented Programming controls program execution flow using success or error tracks. This enables function chaining without explicit error checking at each step.

This approach leads to:
- **Explicit error handling** - No hidden null references or exceptions
- **Composable operations** - Chain functions together naturally
- **Testable code** - Pure functions are easier to test
- **Type safety** - Compiler-enforced error handling

**Quick example:**
```csharp
var result = FirstName.TryCreate("John")
    .Combine(LastName.TryCreate("Smith"))
    .Combine(EmailAddress.TryCreate("john@example.com"))
    .Bind((first, last, email) => User.TryCreate(first, last, email))
    .Tap(user => _repository.Save(user))
    .Match(
        onSuccess: user => $"Created user: {user.Id}",
        onFailure: error => $"Failed: {error.Detail}"
    );
```

See [Basics](basics.md) for a complete tutorial on Railway-Oriented Programming.

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

Learn more about error handling patterns in [Error Handling](error-handling.md).

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

**Example:**
```csharp
// ❌ Easy to swap parameters
Person CreatePerson(string firstName, string lastName);

// ✅ Compiler catches mistakes
Person CreatePerson(FirstName firstName, LastName lastName);
```

See [Basics](basics.md) to learn how to create type-safe value objects.

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
   .MatchAsync(
      onSuccess: ok => "Success", 
      onFailure: error => error.Detail,
      cancellationToken: cancellationToken);
```

Learn about async patterns and cancellation in [Async & Cancellation](async-cancellation.md).

## Parallel Execution

Fetch data from multiple sources in parallel while maintaining Railway Oriented Programming style:

```csharp
// Execute multiple async operations in parallel using ParallelAsync
var result = await GetUserAsync(userId, cancellationToken)
    .ParallelAsync(GetOrdersAsync(userId, cancellationToken))
    .ParallelAsync(GetPreferencesAsync(userId, cancellationToken))
    .AwaitAsync()
    .Bind((user, orders, preferences) => 
        Result.Success(new UserProfile(user, orders, preferences)));
```

See [Advanced Features](advanced-features.md) for parallel operations, LINQ syntax, and more.

## Performance

The library adds only **~11-16 nanoseconds** of overhead compared to imperative code—less than 0.002% of typical I/O operations. You get cleaner, more maintainable code with virtually zero performance cost.

**Typical operation costs:**
- Database query: **1-10 milliseconds** (1,000,000-10,000,000 ns)
- HTTP request: **10-100 milliseconds** (10,000,000-100,000,000 ns)
- ROP overhead: **11-16 nanoseconds**

The overhead is **negligible** compared to real-world I/O operations. See [BENCHMARKS.md](BENCHMARKS.md) for detailed performance analysis.

## Next Steps

Ready to get started? Choose your learning path:

### 🎓 Beginner Path (Start Here!)
**Time:** 2-3 hours | **Goal:** Understand ROP basics and build your first features

1. 📖 **[Basics](basics.md)** - Learn Railway-Oriented Programming fundamentals
   - Result type, Combine, Bind, Map, Tap, Match
   - Safe error handling patterns
   - Complete working examples

2. 💡 **[Examples](examples.md)** - See real-world patterns and code snippets
   - User registration, form validation
   - HTTP response handling
   - Common patterns library

3. 🔗 **[ASP.NET Core Integration](integration-aspnet.md)** - Connect to your API
   - ToActionResult, ToHttpResult
   - Automatic error-to-HTTP mapping
   - MVC and Minimal API examples

### 📚 Intermediate Path
**Time:** 4-6 hours | **Prerequisites:** Basics | **Goal:** Master error handling and async patterns

1. 🚨 **[Error Handling](error-handling.md)** - Discriminated unions, error aggregation
   - Custom error types
   - MatchError patterns
   - ValidationError fluent API

2. ⚡ **[Async & Cancellation](async-cancellation.md)** - CancellationToken patterns, timeouts
   - Async operation chains
   - Parallel execution
   - Timeout and retry patterns

3. ✅ **[FluentValidation Integration](integration-fluentvalidation.md)** - Domain validation
   - InlineValidator
   - Async validation rules
   - Reuse domain validation at API layer

4. 🔍 **[Debugging](debugging.md)** - Tools and techniques for debugging ROP chains
   - Built-in debug extensions
   - OpenTelemetry tracing
   - Common pitfalls and solutions

### 🚀 Advanced Path
**Time:** 2-3 hours | **Prerequisites:** Intermediate | **Goal:** Expert-level patterns and optimization

1. 🎯 **[Advanced Features](advanced-features.md)** - LINQ, parallel operations, Maybe type
   - LINQ query syntax
   - Parallel async operations
   - Pattern matching
   - Exception capture

2. 🏗️ **[Entity Framework Core](integration-ef.md)** - Repository patterns
   - Result-based repositories
   - Async database operations
   - Transaction handling

3. 📊 **[OpenTelemetry Integration](integration-observability.md)** - Observability
   - Automatic ROP tracing
   - Distributed tracing
   - Performance monitoring

4. ⚡ **[Performance](performance.md)** - Optimization and benchmarks
   - Performance characteristics
   - Benchmarking results
   - Optimization tips

### 📚 Reference Materials (Jump to as Needed)

- **[Error Handling Reference](error-handling.md)** - Complete error type catalog
- **[Debugging Guide](debugging.md)** - Troubleshooting and tools
- **[Performance & Benchmarks](performance.md)** - Detailed performance analysis
- **[Integration Guides](integration.md)** - ASP.NET, EF Core, FluentValidation, OpenTelemetry

---

## Quick Links by Experience Level

**Never used functional programming?** Start with [Introduction](intro.md) then [Basics](basics.md)

**Coming from F# or Haskell?** Jump to [Advanced Features](advanced-features.md) and [Examples](examples.md)

**Need to integrate with existing code?** See [Integration](integration.md) and [FluentValidation](integration-fluentvalidation.md)

**Looking for specific patterns?** Check [Examples](examples.md) and [Error Handling](error-handling.md)
