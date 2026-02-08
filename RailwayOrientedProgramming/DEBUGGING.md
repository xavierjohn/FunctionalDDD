# Debugging Railway Oriented Programming

One of the challenges with functional programming and chained operations is debugging. When a chain fails, it can be difficult to determine which step caused the failure. This guide provides strategies and techniques to effectively debug ROP code.

## Understanding the Railway Track

Railway Oriented Programming creates a chain of operations where:
- **Success Track**: Operations continue flowing through the chain
- **Failure Track**: Once an error occurs, the chain short-circuits and subsequent operations are skipped

When debugging, understand that **only the first failure in a chain matters** — everything after that failure is bypassed.

## Common Debugging Challenges

### 1. Which Step Failed?

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

var ordersResult = await GetOrdersAsync(activeUserResult.Value.Id);
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

### 2. Inspecting Values Mid-Chain

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

### 3. Async Debugging

**Problem**: Async chains are harder to step through in the debugger.

**Solution**: Break up long async chains into named variables:

```csharp
// Instead of one long chain
var result = await GetUserAsync(id)
    .BindAsync(u => GetOrdersAsync(u.Id))
    .MapAsync(orders => ProcessOrders(orders));

// Break it up — set breakpoints on each line
var userResult = await GetUserAsync(id);
if (userResult.IsFailure) return userResult.Error;

var ordersResult = await GetOrdersAsync(userResult.Value.Id);
if (ordersResult.IsFailure) return ordersResult.Error;

var processed = ordersResult.Map(orders => ProcessOrders(orders));
return processed;
```

### 4. Testing Individual Steps

**Problem**: A complex chain makes it hard to test individual operations.

**Solution**: Extract operations into testable methods:

```csharp
// Instead of inline operations
public Result<User> ValidateAndProcessUser(string id)
{
    return GetUser(id)
        .Ensure(u => u.IsActive, Error.Validation("Inactive"))
        .Ensure(u => u.Email.Contains('@'), Error.Validation("Invalid email"))
        .Tap(u => u.LastLoginAt = DateTime.UtcNow);
}

// Extract testable pieces
public Result<User> GetActiveUser(string id) =>
    GetUser(id)
        .Ensure(u => u.IsActive, Error.Validation("User is inactive"));

public Result<User> ValidateUserEmail(User user) =>
    user.Email.Contains('@')
        ? Result.Success(user)
        : Error.Validation("Invalid email format");

// Compose and test separately
public Result<User> ValidateAndProcessUser(string id) =>
    GetActiveUser(id)
        .Bind(ValidateUserEmail)
        .Tap(UpdateLastLogin);
```

### 5. Combine Errors Are Aggregated

**Problem**: When using `Combine`, all errors are collected. Understanding which operations failed requires inspecting the error type.

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
            foreach (var err in aggregated.Errors)
                _logger.LogWarning("Operation failed: {ErrorType} - {Code} - {Detail}", 
                    err.GetType().Name, err.Code, err.Detail);
        }
        else if (error is ValidationError validation)
        {
            foreach (var fieldError in validation.FieldErrors)
                _logger.LogWarning("Validation failed for {Field}: {Details}", 
                    fieldError.FieldName, string.Join(", ", fieldError.Details));
        }
        else
        {
            _logger.LogWarning("Operation failed: {Detail}", error.Detail);
        }
    });
```

## Debugging Tools & Techniques

### Conditional Breakpoints

Set conditional breakpoints in `Tap` operations:

```csharp
var result = ProcessUsers(users)
    .Tap(user => 
    {
        if (user.Id == "problem-id")
            _logger.LogDebug("Processing problem user: {@User}", user);
    });
```

### Built-in Debug Extension Methods

The library includes `Debug` extension methods that are only compiled in DEBUG builds:

```csharp
var result = GetUser(id)
    .Debug("After GetUser")
    .Ensure(u => u.IsActive, Error.Validation("Inactive"))
    .Debug("After Ensure")
    .Bind(ProcessUser)
    .DebugDetailed("Final result");

// Custom debug actions
var result = GetUser(id)
    .DebugOnSuccess(user => Console.WriteLine($"User: {user.Id}, Email: {user.Email}"))
    .DebugOnFailure(error => Console.WriteLine($"Error: {error.GetType().Name} - {error.Detail}"));

// Async variants
var result = await GetUserAsync(id)
    .DebugAsync("After GetUser")
    .BindAsync(u => GetOrdersAsync(u.Id))
    .DebugDetailedAsync("After GetOrders");
```

**Note:** These methods are automatically excluded from RELEASE builds — no performance impact in production.

### Logging Extensions

Create a logging policy for your ROP chains:

```csharp
public static class ResultLoggingExtensions
{
    public static Result<T> LogOnFailure<T>(
        this Result<T> result, ILogger logger, string operation)
    {
        return result.TapError(error => 
            logger.LogWarning("Operation {Operation} failed: {ErrorCode} - {Message}",
                operation, error.Code, error.Detail));
    }
}

// Usage
var result = await GetUserAsync(id)
    .LogOnFailure(_logger, "GetUser")
    .BindAsync(u => GetOrdersAsync(u.Id))
    .LogOnFailure(_logger, "GetOrders");
```

### OpenTelemetry Tracing

Enable distributed tracing for ROP operations:

```csharp
services.AddOpenTelemetryTracing(builder =>
{
    builder
        .AddRailwayOrientedProgrammingInstrumentation()
        .AddOtlpExporter();
});

// Operations are automatically traced
var result = await GetUserAsync(id)
    .BindAsync(u => GetOrdersAsync(u.Id))
    .MapAsync(orders => ProcessOrders(orders));
```

### Result Inspection in Tests

Use FluentAssertions for readable test assertions:

```csharp
[Fact]
public void Should_Fail_With_Validation_Error()
{
    var result = ProcessOrder(invalidOrder);
    
    result.IsFailure.Should().BeTrue();
    result.Error.Should().BeOfType<ValidationError>();
    result.Error.Detail.Should().Contain("invalid quantity");
}
```

## Debugging Checklist

1. ✅ Use descriptive error messages with context (IDs, parameters)
2. ✅ Add `Tap`/`TapError` calls at key decision points
3. ✅ Break complex chains into named methods for better stack traces
4. ✅ Test each operation independently before composing
5. ✅ Use structured logging with correlation IDs
6. ✅ Include `fieldName` in validation errors
7. ✅ Use built-in `Debug` extensions during development
