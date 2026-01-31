# Railway Oriented Programming

[![NuGet Package](https://img.shields.io/nuget/v/FunctionalDdd.RailwayOrientedProgramming.svg)](https://www.nuget.org/packages/FunctionalDdd.RailwayOrientedProgramming)

Railway Oriented Programming (ROP) is a functional approach to error handling that treats your code like a railway track. Operations either succeed (staying on the success track) or fail (switching to the error track). This library provides the core types and extension methods to implement ROP in C#.

## Table of Contents

- [Installation](#installation)
- [Core Concepts](#core-concepts)
  - [Result Type](#result-type)
  - [Maybe Type](#maybe-type)
  - [Error Types](#error-types)
- [Getting Started](#getting-started)
- [Core Operations](#core-operations)
  - [Bind](#bind)
  - [Map](#map)
  - [Tap](#tap)
  - [Ensure](#ensure)
  - [RecoverOnFailure](#RecoverOnFailure)
  - [Combine](#combine)
- [Advanced Features](#advanced-features)
  - [LINQ Query Syntax](#linq-query-syntax)
  - [Pattern Matching](#pattern-matching)
  - [Exception Capture](#exception-capture)
  - [Parallel Operations](#parallel-operations)
  - [Error Transformation](#error-transformation)
- [Common Patterns](#common-patterns)
- [Debugging Railway Oriented Programming](#debugging-railway-oriented-programming)
  - [Understanding the Railway Track](#understanding-the-railway-track)
  - [Common Debugging Challenges](#common-debugging-challenges)
  - [Debugging Tools & Techniques](#debugging-tools--techniques)
  - [Best Practices for Debuggable ROP Code](#best-practices-for-debuggable-rop-code)
  - [Debugging Checklist](#debugging-checklist)
  - [Common Pitfalls](#common-pitfalls)
- [Best Practices](#best-practices)

## Installation

Install via NuGet:

```bash
dotnet add package FunctionalDdd.RailwayOrientedProgramming
```

## Core Concepts

### Result Type

The `Result<TValue>` type represents either a successful computation (with a value) or a failure (with an error).

```csharp
public readonly struct Result<TValue>
{
    public TValue Value { get; }        // Throws if IsFailure
    public Error Error { get; }         // Throws if IsSuccess
    
    public bool IsSuccess { get; }
    public bool IsFailure { get; }

    // Implicit conversions
    public static implicit operator Result<TValue>(TValue value);
    public static implicit operator Result<TValue>(Error error);
}
```

**Basic Usage:**

```csharp
using FunctionalDdd;

// Success result
Result<int> success = Result.Success(42);
Result<int> alsoSuccess = 42; // Implicit conversion

// Failure result
Result<int> failure = Result.Failure<int>(Error.NotFound("Item not found"));
Result<int> alsoFailure = Error.NotFound("Item not found"); // Implicit conversion

// Checking state
if (success.IsSuccess)
{
    var value = success.Value; // 42
}

if (failure.IsFailure)
{
    var error = failure.Error; // Error object
}
```

### Maybe Type

The `Maybe<T>` type represents an optional value that may or may not exist.

```csharp
public readonly struct Maybe<T> : IEquatable<T>, IEquatable<Maybe<T>>
    where T : notnull
{
    public T Value { get; }
    public bool HasValue { get; }
    public bool HasNoValue { get; }
}
```

**Basic Usage:**

```csharp
// Create Maybe with value
Maybe<string> some = Maybe.From("hello");
Maybe<string> alsoSome = "hello"; // Implicit conversion

// Create Maybe without value
Maybe<string> none = Maybe.None<string>();
Maybe<string> alsoNone = null; // For reference types

// Check and use
if (some.HasValue)
{
    Console.WriteLine(some.Value); // "hello"
}

// Get value or default
string result = none.GetValueOrDefault("default"); // "default"
```

### Error Types

The library provides several built-in error types, each with a specific purpose and default HTTP status code mapping:

| Error Type | Factory Method | Use When | HTTP Status | Code |
|------------|---------------|----------|-------------|------|
| `ValidationError` | `Error.Validation()` | Input data fails validation rules | 400 Bad Request | `validation.error` |
| `BadRequestError` | `Error.BadRequest()` | Request is malformed or syntactically invalid | 400 Bad Request | `bad.request.error` |
| `UnauthorizedError` | `Error.Unauthorized()` | User is not authenticated (not logged in) | 401 Unauthorized | `unauthorized.error` |
| `ForbiddenError` | `Error.Forbidden()` | User lacks permission (authenticated but forbidden) | 403 Forbidden | `forbidden.error` |
| `NotFoundError` | `Error.NotFound()` | Requested resource doesn't exist | 404 Not Found | `not.found.error` |
| `ConflictError` | `Error.Conflict()` | Operation conflicts with current state | 409 Conflict | `conflict.error` |
| `DomainError` | `Error.Domain()` | Business rule or domain logic violation | 422 Unprocessable Entity | `domain.error` |
| `RateLimitError` | `Error.RateLimit()` | Too many requests (quota exceeded) | 429 Too Many Requests | `rate.limit.error` |
| `UnexpectedError` | `Error.Unexpected()` | Unexpected system error or exception | 500 Internal Server Error | `unexpected.error` |
| `ServiceUnavailableError` | `Error.ServiceUnavailable()` | Service temporarily unavailable | 503 Service Unavailable | `service.unavailable.error` |
| `AggregateError` | (created via `Combine()`) | Multiple non-validation errors combined | Varies | `aggregate.error` |

**Common Usage Examples:**

```csharp
// ValidationError - field-level validation failures
var validation = Error.Validation("Email format is invalid", "email");
var multiField = Error.Validation("Password too short", "password")
    .And("email", "Email is required");

// BadRequestError - malformed requests
var badRequest = Error.BadRequest("Invalid JSON payload");

// NotFoundError - resource not found
var notFound = Error.NotFound($"User {userId} not found", userId);

// ConflictError - state conflicts
var conflict = Error.Conflict("Email address already in use");

// UnauthorizedError - authentication required
var unauthorized = Error.Unauthorized("Login required to access this resource");

// ForbiddenError - insufficient permissions
var forbidden = Error.Forbidden("Admin access required");

// DomainError - business rule violations
var domain = Error.Domain("Cannot withdraw more than account balance");

// RateLimitError - quota exceeded
var rateLimit = Error.RateLimit("API rate limit exceeded. Retry in 60 seconds");

// ServiceUnavailableError - temporary unavailability
var unavailable = Error.ServiceUnavailable("Service under maintenance");

// UnexpectedError - system errors
var unexpected = Error.Unexpected("Database connection failed");
```

**Choosing the Right Error Type:**

- Use **ValidationError** for field-level input validation (e.g., invalid email format, missing required fields)
- Use **BadRequestError** for syntactic/structural issues (e.g., malformed JSON, invalid query parameters)
- Use **DomainError** for business logic violations (e.g., insufficient funds, order quantity limits)
- Use **ConflictError** for state-based conflicts (e.g., duplicate email, concurrent modification)
- Use **UnexpectedError** for infrastructure/system failures (e.g., database errors, network timeouts)

**Error Combining:**

When multiple errors occur, they are intelligently combined:
- Multiple `ValidationError` instances ? Merged into a single `ValidationError` with all field errors
- Mixing `ValidationError` with other error types ? Creates an `AggregateError`
- Multiple non-validation errors ? Creates an `AggregateError`

```csharp
// Validation errors are merged
var error1 = Error.Validation("Email required", "email");
var error2 = Error.Validation("Password required", "password");
var combined = error1.Combine(error2); // Single ValidationError with both fields

// Mixed error types create AggregateError
var validation = Error.Validation("Invalid input", "field");
var notFound = Error.NotFound("Resource not found");
var aggregate = validation.Combine(notFound); // AggregateError with 2 errors
```

## Getting Started

Here's a simple example demonstrating the power of Railway Oriented Programming:

```csharp
public record User(string Id, string Email, bool IsActive);

public Result<User> GetActiveUser(string userId)
{
    return GetUserById(userId)
        .ToResult(Error.NotFound($"User {userId} not found"))
        .Ensure(user => user.IsActive, 
               Error.Validation("User account is not active"))
        .Tap(user => LogUserAccess(user.Id));
}

private User? GetUserById(string id) { /* ... */ }
private void LogUserAccess(string userId) { /* ... */ }
```

## Core Operations

### Bind

`Bind` chains operations that return `Result`. It calls the function only if the current result is successful.

**Use when:** You need to chain operations where each step can fail.

```csharp
// Basic bind
Result<int> ParseAge(string input) => 
    int.TryParse(input, out var age) 
        ? Result.Success(age) 
        : Error.Validation("Invalid age");

Result<string> ValidateAge(int age) =>
    age >= 18 
        ? Result.Success($"Age {age} is valid") 
        : Error.Validation("Must be 18 or older");

var result = ParseAge("25")
    .Bind(age => ValidateAge(age)); // Success("Age 25 is valid")

var invalid = ParseAge("15")
    .Bind(age => ValidateAge(age)); // Failure
```

**Async variant:**

```csharp
async Task<Result<User>> GetUserAsync(string id) { /* ... */ }
async Task<Result<Order>> GetLastOrderAsync(User user) { /* ... */ }

var result = await GetUserAsync("123")
    .BindAsync(user => GetLastOrderAsync(user));
```

**Async with CancellationToken:**

```csharp
async Task<Result<User>> GetUserAsync(string id, CancellationToken ct) { /* ... */ }
async Task<Result<Order>> GetLastOrderAsync(User user, CancellationToken ct) { /* ... */ }

var ct = cancellationToken;

// Single parameter
var result = await GetUserAsync("123", ct)
    .BindAsync(user => GetLastOrderAsync(user, ct));

// Works with tuples too
var complexResult = EmailAddress.TryCreate("user@example.com")
    .Combine(UserId.TryCreate("123"))
    .BindAsync((email, userId) => CreateUserAsync(email, userId, ct));
```

### Map

`Map` transforms the value inside a successful `Result`. Unlike `Bind`, the transformation function returns a plain value, not a `Result`.

**Use when:** You need to transform a value without introducing failure.

```csharp
var result = Result.Success(5)
    .Map(x => x * 2)           // Success(10)
    .Map(x => x.ToString());   // Success("10")

// With failure
var failure = Result.Failure<int>(Error.NotFound("Number not found"))
    .Map(x => x * 2);          // Still Failure, Map is not called
```

**Async variant:**

```csharp
var result = await GetUserAsync("123")
    .MapAsync(user => user.Email.ToLowerInvariant());
```

### Tap

`Tap` executes a side effect (like logging) on success without changing the result. It returns the same `Result`.

**Use when:** You need to perform side effects (logging, metrics, etc.) without transforming the value.

```csharp
var result = Result.Success(42)
    .Tap(x => Console.WriteLine($"Value: {x}"))  // Logs "Value: 42"
    .Tap(x => _metrics.IncrementCounter())       // Records metric
    .Map(x => x * 2);                            // Success(84)

// With failure - Tap is skipped
var failure = Result.Failure<int>(Error.NotFound("Not found"))
    .Tap(x => Console.WriteLine("This won't run"))
    .Map(x => x * 2);  // Still Failure
```

**Async variant:**

```csharp
var result = await GetUserAsync("123")
    .TapAsync(async user => await AuditLogAsync(user.Id))
    .TapAsync(user => SendWelcomeEmail(user.Email));
```

**Async with CancellationToken:**

```csharp
var ct = cancellationToken;

// Single parameter
var result = await GetUserAsync("123", ct)
    .TapAsync(user => AuditLogAsync(user.Id, ct))
    .TapAsync(user => SendWelcomeEmailAsync(user.Email, ct));
```

### Ensure

`Ensure` validates a condition on success. If the condition is false, it returns a failure with the specified error.

**Use when:** You need to validate business rules or conditions.

```csharp
Result<User> CreatePremiumUser(string name, int age)
{
    return User.Create(name, age)
        .Ensure(user => user.Age >= 18, 
               Error.Validation("Must be 18 or older"))
        .Ensure(user => !string.IsNullOrEmpty(user.Name), 
               Error.Validation("Name is required"))
        .Tap(user => user.GrantPremiumAccess());
}
```

**Multiple conditions:**

```csharp
var result = GetProduct(productId)
    .Ensure(p => p.Stock > 0, Error.Validation("Out of stock"))
    .Ensure(p => p.Price > 0, Error.Validation("Invalid price"))
    .Ensure(p => !p.IsDiscontinued, Error.Validation("Product discontinued"));
```

**Async variant:**

```csharp
var result = await GetUserAsync("123")
    .EnsureAsync(async user => await IsEmailVerifiedAsync(user.Email),
                Error.Validation("Email not verified"));
```

### RecoverOnFailure

`RecoverOnFailure` provides error recovery by calling a fallback function when a result fails. Useful for providing default values or alternative paths.

**Use when:** You need fallback behavior or error recovery.

**Basic recovery:**

```csharp
// RecoverOnFailure without accessing the error
Result<User> result = GetUser(userId)
    .RecoverOnFailure(() => CreateGuestUser());

// RecoverOnFailure with access to the error
Result<User> result = GetUser(userId)
    .RecoverOnFailure(error => CreateUserFromError(error));
```

**Conditional recovery with predicate:**

RecoverOnFailure only when specific error conditions are met:

```csharp
// RecoverOnFailure only for NotFound errors
Result<User> result = GetUser(userId)
    .RecoverOnFailure(
        predicate: error => error is NotFoundError,
        func: () => CreateDefaultUser()
    );

// RecoverOnFailure with error context
Result<User> result = GetUser(userId)
    .RecoverOnFailure(
        predicate: error => error is NotFoundError,
        func: error => CreateUserFromError(error)
    );

// RecoverOnFailure based on error code
Result<Data> result = FetchData(id)
    .RecoverOnFailure(
        predicate: error => error.Code == "not.found.error",
        func: () => GetCachedData(id)
    );

// RecoverOnFailure for multiple error types
Result<Config> result = LoadConfig()
    .RecoverOnFailure(
        predicate: error => error is NotFoundError or UnauthorizedError,
        func: () => GetDefaultConfig()
    );
```

**Async variant:**

```csharp
var result = await GetUserAsync(userId)
    .RecoverOnFailureAsync(async error => await GetFromCacheAsync(userId));
```

### Combine

`Combine` aggregates multiple `Result` objects. If all succeed, returns success with all values. If any fail, returns all errors combined.

**Use when:** You need to validate multiple independent operations before proceeding.

```csharp
// Combine multiple validations
var result = EmailAddress.TryCreate("user@example.com")
    .Combine(FirstName.TryCreate("John"))
    .Combine(LastName.TryCreate("Doe"))
    .Bind((email, firstName, lastName) => 
        User.Create(email, firstName, lastName));

// All validations must pass
if (result.IsSuccess)
{
    var user = result.Value; // All inputs were valid
}
else
{
    var errors = result.Error; // Contains all validation errors
}
```

**With optional values:**

In this scenario, `firstName` is optional. If provided, it will be validated; if not, it will be skipped. 
In other words, FirstName.TryCreate is only called if firstName is not null.

```csharp
string? firstName = null;  // Optional
string email = "user@example.com";
string? lastName = "Doe";

var result = EmailAddress.TryCreate(email)
    .Combine(Maybe.Optional(firstName, FirstName.TryCreate))
    .Combine(Maybe.Optional(lastName, LastName.TryCreate))
    .Bind((e, f, l) => CreateProfile(e, f, l));
```

## Advanced Features

### LINQ Query Syntax

You can use C# query expressions with `Result` via `Select`, `SelectMany`, and `Where`:

```csharp
// Chaining operations with query syntax
var total = from a in Result.Success(2)
            from b in Result.Success(3)
            from c in Result.Success(5)
            select a + b + c;  // Success(10)

// With failure
var result = from x in Result.Success(5)
             where x > 10  // Predicate fails -> UnexpectedError
             select x;

// Practical example
var userOrder = from user in GetUser(userId)
                from order in GetOrder(orderId)
                where order.UserId == user.Id
                select (user, order);
```

**Note:** `where` uses an `UnexpectedError` if the predicate fails. For domain-specific errors, prefer `Ensure`.

### Pattern Matching

Use `Match` to handle both success and failure cases inline:

```csharp
// Synchronous match
var description = GetUser("123").Match(
    onSuccess: user => $"User: {user.Name}",
    onFailure: error => $"Error: {error.Code}"
);

// Async match
await ProcessOrderAsync(order).MatchAsync(
    onSuccess: async order => await SendConfirmationAsync(order),
    onFailure: async error => await LogErrorAsync(error)
);

// With return value
var httpResult = SaveData(data).Match(
    onSuccess: data => Results.Ok(data),
    onFailure: error => error.ToErrorResult()
);
```

### Exception Capture

Use `Try` and `TryAsync` to safely capture exceptions and convert them to `Result`:

**Use when:** Integrating with code that throws exceptions.

```csharp
// Synchronous
Result<string> LoadFile(string path)
{
    return Result.Try(() => File.ReadAllText(path));
}

// Async
async Task<Result<User>> FetchUserAsync(string url)
{
    return await Result.TryAsync(async () => 
        await _httpClient.GetFromJsonAsync<User>(url));
}

// Usage
var content = LoadFile("config.json")
    .Ensure(c => !string.IsNullOrEmpty(c), 
           Error.Validation("File is empty"))
    .Bind(ParseConfig);
```

### Parallel Operations

Run multiple async operations in parallel and combine their results:

```csharp
var result = await GetStudentInfoAsync(studentId)
    .ParallelAsync(GetStudentGradesAsync(studentId))
    .ParallelAsync(GetLibraryBooksAsync(studentId))
    .AwaitAsync()
    .BindAsync((info, grades, books) => 
        PrepareReport(info, grades, books));
```

### Error Transformation

Transform errors while preserving success values:

```csharp
Result<int> GetUserPoints(string userId) { /* ... */ }

var apiResult = GetUserPoints(userId)
    .MapError(err => Error.NotFound($"Points for user {userId} not found"));

// Success values pass through unchanged
// Failure errors are replaced with the new error
```

## Common Patterns

### Validation Pipeline

```csharp
public Result<Order> ProcessOrder(OrderRequest request)
{
    return ValidateRequest(request)
        .Bind(req => CheckInventory(req.ProductId, req.Quantity))
        .Bind(product => ValidatePayment(request.PaymentInfo))
        .Bind(payment => CreateOrder(request, payment))
        .Tap(order => SendConfirmationEmail(order))
        .TapError(error => LogOrderFailure(error));
}
```

### Error Recovery with Fallbacks

```csharp
public Result<Config> LoadConfiguration()
{
    return LoadFromFile("config.json")
        .RecoverOnFailure(error => error is NotFoundError, 
                   () => LoadFromEnvironment())
        .RecoverOnFailure(error => error is NotFoundError, 
                   () => GetDefaultConfig())
        .Ensure(cfg => cfg.IsValid, 
               Error.Validation("Invalid configuration"));
}
```

### Multi-Field Validation

```csharp
public Result<User> RegisterUser(string email, string firstName, string lastName, int age)
{
    return EmailAddress.TryCreate(email)
        .Combine(FirstName.TryCreate(firstName))
        .Combine(LastName.TryCreate(lastName))
        .Combine(EnsureExtensions.Ensure(age >= 18, 
                Error.Validation("Must be 18 or older", "age")))
        .Bind((e, f, l) => User.Create(e, f, l, age));
}
```

### Async Chain with Side Effects

```csharp
public async Task<Result<string>> PromoteCustomerAsync(string customerId)
{
    return await GetCustomerByIdAsync(customerId)
        .ToResultAsync(Error.NotFound($"Customer {customerId} not found"))
        .EnsureAsync(customer => customer.CanBePromoted,
                    Error.Validation("Customer has highest status"))
        .TapAsync(customer => customer.PromoteAsync())
        .BindAsync(customer => SendPromotionEmailAsync(customer.Email))
        .MatchAsync(
            onSuccess: _ => "Promotion successful",
            onFailure: error => error.Detail
        );
}
```

## Debugging Railway Oriented Programming

One of the challenges with functional programming and chained operations is debugging. When a chain fails, it can be difficult to determine which step caused the failure. This section provides strategies and techniques to effectively debug ROP code.

### Understanding the Railway Track

Railway Oriented Programming creates a chain of operations where:
- **Success Track**: Operations continue flowing through the chain
- **Failure Track**: Once an error occurs, the chain short-circuits and subsequent operations are skipped

When debugging, understand that **only the first failure in a chain matters** - everything after that failure is bypassed.

### Common Debugging Challenges

#### 1. Which Step Failed?

**Problem**: A long chain fails, but you don't know which operation caused the failure.

```csharp
// Which of these 5 operations failed?
var result = await GetUserAsync(id)
    .ToResultAsync(Error.NotFound("User not found"))
    .EnsureAsync(u => u.IsActive, Error.Validation("User inactive"))
    .BindAsync(u => GetOrdersAsync(u.Id))
    .EnsureAsync(orders => orders.Any(), Error.NotFound("No orders"))
    .MapAsync(orders => orders.Sum(o => o.Total));
```

**Solution 1**: Use `Tap` or `TapError` to add logging at each step:

```csharp
var result = await GetUserAsync(id)
    .Tap(u => _logger.LogDebug("Found user: {UserId}", u.Id))
    .ToResultAsync(Error.NotFound("User not found"))
    .TapError(err => _logger.LogWarning("Failed to find user: {Error}", err))
    .EnsureAsync(u => u.IsActive, Error.Validation("User inactive"))
    .Tap(u => _logger.LogDebug("User {UserId} is active", u.Id))
    .TapError(err => _logger.LogWarning("User validation failed: {Error}", err))
    .BindAsync(u => GetOrdersAsync(u.Id))
    .Tap(orders => _logger.LogDebug("Found {Count} orders", orders.Count))
    .TapError(err => _logger.LogWarning("Failed to get orders: {Error}", err))
    .EnsureAsync(orders => orders.Any(), Error.NotFound("No orders"))
    .MapAsync(orders => orders.Sum(o => o.Total))
    .Tap(total => _logger.LogDebug("Calculated total: {Total}", total));
```

**Solution 2**: Break the chain into smaller, named steps:

```csharp
var userResult = await GetUserAsync(id)
    .ToResultAsync(Error.NotFound("User not found"));
    
if (userResult.IsFailure)
{
    _logger.LogWarning("GetUser failed: {Error}", userResult.Error);
    return userResult.Error;
}

var activeUserResult = userResult
    .Ensure(u => u.IsActive, Error.Validation("User inactive"));
    
if (activeUserResult.IsFailure)
{
    _logger.LogWarning("User validation failed: {Error}", activeUserResult.Error);
    return activeUserResult.Error;
}

var ordersResult = await GetOrdersAsync(activeUserResult.Value.Id);  // Another breakpoint
if (ordersResult.IsFailure) return ordersResult.Error;

```

**Solution 3**: Use descriptive error messages with context:

```csharp
var result = await GetUserAsync(id)
    .ToResultAsync(Error.NotFound($"User {userId} not found in database"))
    .EnsureAsync(u => u.IsActive, 
        Error.Validation($"User {userId} account is inactive since {u.DeactivatedAt}"))
    .BindAsync(u => GetOrdersAsync(u.Id))
    .EnsureAsync(orders => orders.Any(), 
        Error.NotFound($"No orders found for user {userId}"))
    .MapAsync(orders => orders.Sum(o => o.Total));

// When this fails, the error message tells you exactly where it failed
```


#### 2. Inspecting Values Mid-Chain

**Problem**: You want to see what value is flowing through the chain at a specific point.

**Solution 1**: Use `Tap` with a breakpoint:

```csharp
var result = await GetUserAsync(id)
    .Tap(user => 
    {
        // Set breakpoint here to inspect 'user'
        var debug = new { user.Id, user.Name, user.Email };
        _logger.LogDebug("User state: {@User}", debug);
    })
    .BindAsync(u => ProcessUserAsync(u));
```

**Solution 2**: Use `Tap` to capture values for assertions in tests:

```csharp
[Fact]
public async Task Should_Process_Valid_User()
{
    User? capturedUser = null;
    
    var result = await GetUserAsync("123")
        .Tap(user => capturedUser = user)  // Capture the value
        .BindAsync(u => ProcessUserAsync(u));
    
    Assert.NotNull(capturedUser);
    Assert.Equal("123", capturedUser.Id);
    result.IsSuccess.Should().BeTrue();
}
```

**Solution 3**: Use `Map` to temporarily transform for inspection:

```csharp
var result = await GetOrdersAsync(userId)
    .Map(orders => 
    {
        _logger.LogDebug("Order count: {Count}, Total: {Total}", 
            orders.Count, orders.Sum(o => o.Total));
        return orders;  // Return unchanged for the chain
    })
    .BindAsync(orders => ProcessOrdersAsync(orders));
```

#### 3. Async Debugging

**Problem**: Async chains are harder to step through in the debugger.

**Solution 1**: Add `.ConfigureAwait(false)` when appropriate and use named variables:

```csharp
// Instead of one long chain
var result = await GetUserAsync(id)
    .BindAsync(u => GetOrdersAsync(u.Id))
    .MapAsync(orders => ProcessOrders(orders));

// Break it up
var userResult = await GetUserAsync(id);  // Can set breakpoint and inspect
if (userResult.IsFailure) return userResult.Error;

var ordersResult = await GetOrdersAsync(userResult.Value.Id);  // Another breakpoint
if (ordersResult.IsFailure) return ordersResult.Error;

var processed = ordersResult.Map(orders => ProcessOrders(orders));  // Inspect here
return processed;
```

**Solution 2**: Use `TapAsync` with logging for async side effects:

```csharp
var result = await GetUserAsync(id)
    .TapAsync(async user => 
    {
        await Task.Delay(1);  // Simulate async
        _logger.LogDebug("Processing user {UserId} at {Time}", user.Id, DateTime.UtcNow);
    })
    .BindAsync(u => GetOrdersAsync(u.Id));
```

#### 4. Testing Individual Steps

**Problem**: A complex chain makes it hard to test individual operations.

**Solution**: Extract operations into testable methods:

```csharp
// Instead of inline operations
public Result<User> ValidateAndProcessUser(string id)
{
    return GetUser(id)
        .Ensure(u => u.IsActive, Error.Validation("Inactive"))
        .Ensure(u => u.Email.Contains("@"), Error.Validation("Invalid email"))
        .Tap(u => u.LastLoginAt = DateTime.UtcNow);
}

// Extract testable pieces
public Result<User> GetActiveUser(string id) =>
    GetUser(id)
        .Ensure(u => u.IsActive, Error.Validation("User is inactive"));

public Result<User> ValidateUserEmail(User user) =>
    user.Email.Contains("@")
        ? Result.Success(user)
        : Error.Validation("Invalid email format");

public void UpdateLastLogin(User user) =>
    user.LastLoginAt = DateTime.UtcNow;

// Now compose and test separately
public Result<User> ValidateAndProcessUser(string id) =>
    GetActiveUser(id)
        .Bind(ValidateUserEmail)
        .Tap(UpdateLastLogin);

// Easy to test each part
[Fact]
public void GetActiveUser_Should_Fail_For_Inactive_User()
{
    var result = GetActiveUser("inactive-user-id");
    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be("validation.error");
}
```

#### 5. Combine Errors Are Aggregated

**Problem**: When using `Combine`, all errors are collected. Understanding which operations failed requires inspecting the error type.

```csharp
var result = GetUserAsync(userId)
    .ToResultAsync(Error.NotFound("User not found"))
    .Combine(FetchUserPreferencesAsync(userId))
    .Combine(ValidateSubscriptionAsync(userId));

// This might fail with multiple errors - which operations failed?
```

**Solution**: Use `TapError` to log errors, handling both `AggregateError` (mixed error types) and `ValidationError` (merged validations):

```csharp
var result = GetUserAsync(userId)
    .ToResultAsync(Error.NotFound($"User {userId} not found"))
    .Combine(FetchUserSettingsAsync(userId))
    .Combine(EmailAddress.TryCreate(email))
    .TapError(error => 
    {
        if (error is AggregateError aggregated)
        {
            // Mixed error types (e.g., NotFound + Validation)
            foreach (var err in aggregated.Errors)
            {
                _logger.LogWarning("Operation failed: {ErrorType} - {Code} - {Detail}", 
                    err.GetType().Name, err.Code, err.Detail);
            }
        }
        else if (error is ValidationError validation)
        {
            // Multiple validation errors merged into one
            foreach (var fieldError in validation.FieldErrors)
            {
                _logger.LogWarning("Validation failed for {Field}: {Details}", 
                    fieldError.FieldName, string.Join(", ", fieldError.Details));
            }
        }
        else
        {
            _logger.LogWarning("Operation failed: {Detail}", error.Detail);
        }
    });
```

**Or in tests, check individual errors:**

```csharp
[Fact]
public void Combine_Should_Return_All_Validation_Errors()
{
    var result = EmailAddress.TryCreate("bad-email")
        .Combine(FirstName.TryCreate(""))
        .Combine(Age.Ensure(15, e => e >= 18, Error.Validation("Must be 18 or older")));
    
    result.IsFailure.Should().BeTrue();
    result.Error.Should().BeOfType<ValidationError>();
    
    var validation = (ValidationError)result.Error;
    validation.FieldErrors.Should().HaveCount(3);
    validation.FieldErrors.Should().Contain(e => e.FieldName == "email");
    validation.FieldErrors.Should().Contain(e => e.FieldName == "firstName");
    validation.FieldErrors.Should().Contain(e => e.Details.Any(d => d.Contains("Age")));
}
```

### Debugging Tools & Techniques

#### 1. Conditional Breakpoints

Set conditional breakpoints in `Tap` operations:

```csharp
var result = ProcessUsers(users)
    .Tap(user => 
    {
        // Breakpoint - only hit when user.Id == "problem-id"
        if (user.Id == "problem-id")
        {
            _logger.LogDebug("Processing problem user: {@User}", user);
        }
    });
```

#### 2. Built-in Debug Extension Methods

The library includes `Debug` extension methods that are only compiled in DEBUG builds:

```csharp
// Basic debug output
var result = GetUser(id)
    .Debug("After GetUser")
    .Ensure(u => u.IsActive, Error.Validation("Inactive"))
    .Debug("After Ensure")
    .Bind(ProcessUser)
    .DebugDetailed("Final result");

// Detailed debug output (includes error properties and aggregated errors)
var result = EmailAddress.TryCreate(email)
    .Combine(FirstName.TryCreate(firstName))
    .Combine(LastName.TryCreate(lastName))
    .DebugDetailed("After validation");

// Debug with stack trace
var result = ProcessOrder(orderId)
    .DebugWithStack("Processing order", includeStackTrace: true);

// Custom debug actions
var result = GetUser(id)
    .DebugOnSuccess(user => 
    {
        Console.WriteLine($"User: {user.Id}, Email: {user.Email}");
        Console.WriteLine($"IsActive: {user.IsActive}");
    })
    .DebugOnFailure(error => 
    {
        Console.WriteLine($"Error Type: {error.GetType().Name}");
        Console.WriteLine($"Message: {error.Detail}");
    });

// Async variants available
var result = await GetUserAsync(id)
    .DebugAsync("After GetUser")
    .BindAsync(u => GetOrdersAsync(u.Id))
    .DebugDetailedAsync("After GetOrders");
```

**Note:** These methods are automatically excluded from RELEASE builds, so there's no performance impact in production.

#### 3. Result Inspection in Tests

Use FluentAssertions (or similar) for readable test assertions:

```csharp
using FluentAssertions;

[Fact]
public void Should_Fail_With_Validation_Error()
{
    var result = ProcessOrder(invalidOrder);
    
    result.IsFailure.Should().BeTrue();
    result.Error.Should().BeOfType<ValidationError>();
    result.Error.Code.Should().Be("validation.error");
    result.Error.Detail.Should().Contain("invalid quantity");
}

[Fact]
public void Should_Return_Processed_User()
{
    var result = ProcessUser(validUserId);
    
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Status.Should().Be(UserStatus.Active);
}
```

#### 4. Logging Strategies

Create a logging policy for your ROP chains:

```csharp
public static class ResultLoggingExtensions
{
    public static Result<T> LogOnFailure<T>(
        this Result<T> result, 
        ILogger logger, 
        string operation)
    {
        return result.TapError(error => 
            logger.LogWarning("Operation {Operation} failed: {ErrorCode} - {Message}",
                operation, error.Code, error.Detail));
    }
    
    public static Result<T> LogOnSuccess<T>(
        this Result<T> result, 
        ILogger logger, 
        string operation)
    {
        return result.Tap(value => 
            logger.LogInformation("Operation {Operation} succeeded with value: {Value}",
                operation, value));
    }
}

// Usage
var result = await GetUserAsync(id)
    .LogOnFailure(_logger, "GetUser")
    .LogOnSuccess(_logger, "GetUser")
    .BindAsync(u => GetOrdersAsync(u.Id))
    .LogOnFailure(_logger, "GetOrders")
    .LogOnSuccess(_logger, "GetOrders");
```

#### 5. Tracing with OpenTelemetry

Enable distributed tracing for ROP operations:

```csharp
services.AddOpenTelemetryTracing(builder =>
{
    builder
        .AddRailwayOrientedProgrammingInstrumentation()  // Built-in instrumentation
        .AddOtlpExporter();
});

// This automatically traces your ROP chains
var result = await GetUserAsync(id)  // Traced as "GetUserAsync"
    .BindAsync(u => GetOrdersAsync(u.Id))  // Traced as "GetOrdersAsync"
    .MapAsync(orders => ProcessOrders(orders));  // Traced as "ProcessOrders"

// View the trace in your APM tool (Jaeger, Zipkin, Application Insights, etc.)
```

### Best Practices for Debuggable ROP Code

1. **Use descriptive error messages** with context (IDs, parameters, timestamps)
   ```csharp
   Error.NotFound($"Order {orderId} not found for user {userId} at {DateTime.UtcNow}")
   ```

2. **Add `Tap` calls at key decision points** in long chains
   ```csharp
   .Tap(x => _logger.LogDebug("Validated: {Value}", x))
   ```

3. **Break complex chains** into smaller, named methods for better stack traces
   ```csharp
   var userResult = await GetActiveUserAsync(id);
   var ordersResult = await GetUserOrdersAsync(userResult);
   // vs one giant chain
   ```

4. **Test each operation independently** before composing
   ```csharp
   [Fact] public void Validate_Email_Format() { /* test */ }
   [Fact] public void Validate_Age_Requirement() { /* test */ }
   [Fact] public void Combine_All_Validations() { /* integration test */ }
   ```

5. **Use structured logging** with correlation IDs
   ```csharp
   .Tap(user => _logger.LogDebug("Processing user {UserId} in request {RequestId}", 
       user.Id, _correlationId))
   ```

6. **Include property names in validation errors** for easier debugging
   
   ```csharp
   // Good
   Error.Validation("Email format is invalid", "email")
   
   // Avoid
   Error.Validation("Invalid format")
   ```

7. **Use built-in debug extension methods** for development
```csharp
// Automatically excluded from RELEASE builds
var result = GetUser(id)
    .Debug("After GetUser")
    .Bind(ProcessUser)
    .DebugDetailed("Final result");
```

8. **Handle errors at boundaries** (controllers, entry points)
   
   ```csharp
   [HttpPost]
   public ActionResult<User> Register(RegisterRequest request) =>
       RegisterUser(request)
           .ToActionResult(this);  // Converts Result to ActionResult
   ```

9. **Use `Try/TryAsync` for exception boundaries**
   
   ```csharp
   Result<Data> LoadData() =>
       Result.Try(() => File.ReadAllText(path))
           .Bind(json => ParseJson(json));
   ```

10. **Use CancellationToken with async operations** for proper cancellation support
   
   ```csharp
   var ct = cancellationToken;
   
   var result = await GetUserAsync(id, ct)
       .BindAsync(user => GetOrderAsync(user.Id, ct))
       .TapAsync(order => LogOrderAsync(order, ct));
   ```

11. **Provide CancellationToken parameter** when calling async operations to enable timeouts and graceful shutdown
   
   ```csharp
   // Good - supports cancellation
   async Task<Result<User>> ProcessUserAsync(string id, CancellationToken ct)
   {
       return await GetUserAsync(id, ct)
           .BindAsync(user => ValidateAsync(user, ct))
           .TapAsync(user => NotifyAsync(user, ct));
   }
   
   // Avoid - no cancellation support
   async Task<Result<User>> ProcessUserAsync(string id)
   {
       return await GetUserAsync(id)
           .BindAsync(user => ValidateAsync(user))
           .TapAsync(user => NotifyAsync(user));
   }
   ```
