# Railway Oriented Programming

[![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.RailwayOrientedProgramming.svg)](https://www.nuget.org/packages/FunctionalDDD.RailwayOrientedProgramming)

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
  - [Compensate](#compensate)
  - [Combine](#combine)
- [Advanced Features](#advanced-features)
  - [LINQ Query Syntax](#linq-query-syntax)
  - [Pattern Matching](#pattern-matching)
  - [Exception Capture](#exception-capture)
  - [Parallel Operations](#parallel-operations)
  - [Error Transformation](#error-transformation)
- [Common Patterns](#common-patterns)
- [Best Practices](#best-practices)

## Installation

Install via NuGet:

```bash
dotnet add package FunctionalDDD.RailwayOrientedProgramming
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

The library provides several built-in error types:

- `Error` - Base error class
- `NotFoundError` - Resource not found
- `ValidationError` - Input validation failure
- `ConflictError` - Business rule conflict
- `UnauthorizedError` - Authentication required
- `ForbiddenError` - Insufficient permissions
- `UnexpectedError` - Unexpected system error
- `AggregatedError` - Multiple errors combined

```csharp
var notFound = Error.NotFound("User not found", "userId");
var validation = Error.Validation("Email is invalid", "email");
var conflict = Error.Conflict("Email already exists");
var unauthorized = Error.Unauthorized("Login required");
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

var result = await GetUserAsync("123", cancellationToken)
    .BindAsync((user, ct) => GetLastOrderAsync(user, ct), cancellationToken);
```

**Tuple-based operations with CancellationToken:**

When working with multiple values from combined results, you can use CancellationToken with tuple operations:

```csharp
// Combine multiple results into a tuple
var result = EmailAddress.TryCreate("user@example.com")
    .Combine(UserId.TryCreate("123"))
    .Combine(OrderId.TryCreate("456"));

// Bind with tuple parameters and CancellationToken
var orderResult = await result
    .BindAsync(
        (email, userId, orderId, ct) => FetchOrderAsync(email, userId, orderId, ct),
        cancellationToken
    );

// Works with tuples of 2-9 parameters
var complexResult = await GetUserDataAsync()
    .BindAsync(
        (id, name, email, phone, ct) => ProcessUserAsync(id, name, email, phone, ct),
        cancellationToken
    );
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
var result = await GetUserAsync("123", cancellationToken)
    .TapAsync(
        async (user, ct) => await AuditLogAsync(user.Id, ct),
        cancellationToken
    )
    .TapAsync(
        async (user, ct) => await SendWelcomeEmailAsync(user.Email, ct),
        cancellationToken
    );
```

**Tuple-based operations with CancellationToken:**

When working with tuples, you can use CancellationToken for side effects on multiple values:

```csharp
// Tap with tuple parameters and CancellationToken
var result = EmailAddress.TryCreate("user@example.com")
    .Combine(UserId.TryCreate("123"))
    .TapAsync(
        async (email, userId, ct) => await LogUserCreationAsync(email, userId, ct),
        cancellationToken
    )
    .TapAsync(
        async (email, userId, ct) => await NotifyAdminAsync(email, userId, ct),
        cancellationToken
    );

// Works with tuples of 2-9 parameters
var complexTap = await GetOrderDetailsAsync()
    .TapAsync(
        async (orderId, customerId, total, status, ct) => 
            await SendOrderNotificationAsync(orderId, customerId, total, status, ct),
        cancellationToken
    );
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

### Compensate

`Compensate` provides error recovery by calling a fallback function when a result fails. Useful for providing default values or alternative paths.

**Use when:** You need fallback behavior or error recovery.

**Basic compensation:**

```csharp
// Compensate without accessing the error
Result<User> result = GetUser(userId)
    .Compensate(() => CreateGuestUser());

// Compensate with access to the error
Result<User> result = GetUser(userId)
    .Compensate(error => CreateUserFromError(error));
```

**Conditional compensation with predicate:**

Compensate only when specific error conditions are met:

```csharp
// Compensate only for NotFound errors
Result<User> result = GetUser(userId)
    .Compensate(
        predicate: error => error is NotFoundError,
        func: () => CreateDefaultUser()
    );

// Compensate with error context
Result<User> result = GetUser(userId)
    .Compensate(
        predicate: error => error is NotFoundError,
        func: error => CreateUserFromError(error)
    );

// Compensate based on error code
Result<Data> result = FetchData(id)
    .Compensate(
        predicate: error => error.Code == "not.found.error",
        func: () => GetCachedData(id)
    );

// Compensate for multiple error types
Result<Config> result = LoadConfig()
    .Compensate(
        predicate: error => error is NotFoundError or UnauthorizedError,
        func: () => GetDefaultConfig()
    );
```

**Async variant:**

```csharp
var result = await GetUserAsync(userId)
    .CompensateAsync(async error => await GetFromCacheAsync(userId));
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
        .Compensate(error => error is NotFoundError, 
                   () => LoadFromEnvironment())
        .Compensate(error => error is NotFoundError, 
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

## Best Practices

1. **Use `Bind` for operations that can fail**, `Map` for pure transformations
   
   ```csharp
   // Good
   GetUser(id)
       .Map(user => user.Name)           // Pure transformation
       .Bind(name => ValidateName(name)) // Can fail
   
   // Avoid
   GetUser(id)
       .Bind(user => Result.Success(user.Name)) // Unnecessary Result wrapping
   ```

2. **Prefer `Ensure` over `Bind` for simple validations**
   
   ```csharp
   // Good
   GetUser(id)
       .Ensure(user => user.IsActive, Error.Validation("User not active"))
   
   // Avoid
   GetUser(id)
       .Bind(user => user.IsActive 
           ? Result.Success(user) 
           : Error.Validation("User not active"))
   ```

3. **Use `Tap` for side effects** (logging, metrics, notifications)
   
   ```csharp
   ProcessOrder(order)
       .Tap(o => _logger.LogInfo($"Order {o.Id} processed"))
       .Tap(o => _metrics.RecordOrder(o))
       .TapError(err => _logger.LogError(err.Message))
   ```

4. **Combine independent validations** instead of nesting
   
   ```csharp
   // Good
   Email.TryCreate(email)
       .Combine(Name.TryCreate(name))
       .Combine(Age.TryCreate(age))
       .Bind((e, n, a) => User.Create(e, n, a))
   
   // Avoid
   Email.TryCreate(email)
       .Bind(e => Name.TryCreate(name)
           .Bind(n => Age.TryCreate(age)
               .Bind(a => User.Create(e, n, a))))
   ```

5. **Use domain-specific errors** instead of generic ones
   
   ```csharp
   // Good
   Error.Validation("Email format is invalid", "email")
   
   // Avoid
   Error.Unexpected("Something went wrong")
   ```

6. **Handle errors at boundaries** (controllers, entry points)
   
   ```csharp
   [HttpPost]
   public ActionResult<User> Register(RegisterRequest request) =>
       RegisterUser(request)
           .ToActionResult(this);  // Converts Result to ActionResult
   ```

7. **Use `Try/TryAsync` for exception boundaries**
   
   ```csharp
   Result<Data> LoadData() =>
       Result.Try(() => File.ReadAllText(path))
           .Bind(json => ParseJson(json));
   ```

8. **Use CancellationToken with async operations** for proper cancellation support
   
   ```csharp
   // Single-parameter operations
   var result = await GetUserAsync(id, cancellationToken)
       .BindAsync((user, ct) => GetOrderAsync(user.Id, ct), cancellationToken)
       .TapAsync(async (order, ct) => await LogOrderAsync(order, ct), cancellationToken);
   
   // Tuple-based operations
   var complexResult = EmailAddress.TryCreate(email)
       .Combine(UserId.TryCreate(userId))
       .BindAsync(
           async (email, userId, ct) => await CreateUserAsync(email, userId, ct),
           cancellationToken
       );
   ```

9. **Provide CancellationToken parameter** when calling async operations to enable timeouts and graceful shutdown
   
   ```csharp
   // Good - supports cancellation
   async Task<Result<User>> ProcessUserAsync(string id, CancellationToken ct)
   {
       return await GetUserAsync(id, ct)
           .BindAsync((user, ct) => ValidateAsync(user, ct), ct)
           .TapAsync(async (user, ct) => await NotifyAsync(user, ct), ct);
   }
   
   // Avoid - no cancellation support
   async Task<Result<User>> ProcessUserAsync(string id)
   {
       return await GetUserAsync(id)
           .BindAsync(user => ValidateAsync(user))
           .TapAsync(async user => await NotifyAsync(user));
   }
   ```