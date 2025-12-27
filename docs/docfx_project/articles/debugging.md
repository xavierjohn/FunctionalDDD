# Debugging Railway Oriented Programming

Chained operations can be challenging to debug. This guide provides strategies for identifying failures in ROP chains.

## Table of Contents

- [Understanding the Railway Track](#understanding-the-railway-track)
- [Common Debugging Challenges](#common-debugging-challenges)
- [Debugging Tools & Techniques](#debugging-tools--techniques)
- [Best Practices](#best-practices)
- [Debugging Checklist](#debugging-checklist)

## Understanding the Railway Track

Railway Oriented Programming chains operations on two tracks:
- **Success Track**: Operations continue through the chain
- **Failure Track**: Once an error occurs, subsequent operations are skipped

**Key insight:** Only the first failure matters—everything after is bypassed.

```csharp
var result = Step1()      // ✅ Succeeds
    .Bind(Step2)          // ❌ Fails - switches to error track
    .Bind(Step3)          // ⏭️ Skipped - on error track
    .Bind(Step4)          // ⏭️ Skipped - on error track
    .Match(
        onSuccess: x => "Won't reach here",
        onFailure: e => "Error from Step2"
    );
```

## Common Debugging Challenges

### Identifying the Failure Point

**Problem**: A long chain fails, but you don't know which operation caused the failure.

```csharp
// Which of these operations failed?
var result = await GetUserAsync(id)
    .ToResultAsync(Error.NotFound("User not found"))
    .EnsureAsync(u => u.IsActive, Error.Validation("User inactive"))
    .BindAsync(u => GetOrdersAsync(u.Id))
    .EnsureAsync(orders => orders.Any(), Error.NotFound("No orders"))
    .MapAsync(orders => orders.Sum(o => o.Total));
```

**Solution 1**: Use `Tap` or `TapError` for logging at each step:

```csharp
var result = await GetUserAsync(id)
    .Tap(u => _logger.LogDebug("Found user: {UserId}", u.Id))
    .ToResultAsync(Error.NotFound("User not found"))
    .TapError(err => _logger.LogWarning("Failed to find user: {Error}", err))
    .EnsureAsync(u => u.IsActive, Error.Validation("User inactive"))
    .Tap(u => _logger.LogDebug("User {UserId} is active", u.Id))
    .TapError(err => _logger.LogWarning("User validation failed: {Error}", err))
    .BindAsync(u => GetOrdersAsync(u.Id))
    .Tap(orders => _logger.LogDebug("Found {Count} orders", orders.Count));
```

**Solution 2**: Break the chain into smaller, named steps:

```csharp
var userResult = await GetUserAsync(id)
    .ToResultAsync(Error.NotFound("User not found"));
    
if (userResult.IsFailure)
{
    _logger.LogWarning("GetUser failed: {Error}", userResult.Error);
    return userResult;
}

var ordersResult = await GetOrdersAsync(userResult.Value.Id);
if (ordersResult.IsFailure)
{
    _logger.LogWarning("GetOrders failed: {Error}", ordersResult.Error);
    return ordersResult;
}

return ordersResult.Map(orders => orders.Sum(o => o.Total));
```

**Solution 3**: Use descriptive error messages with context:

```csharp
var result = await GetUserAsync(id)
    .ToResultAsync(Error.NotFound($"User {id} not found in database"))
    .EnsureAsync(u => u.IsActive, 
        Error.Validation($"User {id} account inactive since {u.DeactivatedAt}"))
    .BindAsync(u => GetOrdersAsync(u.Id))
    .EnsureAsync(orders => orders.Any(), 
        Error.NotFound($"No orders found for user {id}"));
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

**Solution 2**: Capture values in tests:

```csharp
[Fact]
public async Task Should_Process_Valid_User()
{
    User? capturedUser = null;
    
    var result = await GetUserAsync("123")
        .Tap(user => capturedUser = user)  // Capture for inspection
        .BindAsync(u => ProcessUserAsync(u));
    
    Assert.NotNull(capturedUser);
    Assert.Equal("123", capturedUser.Id);
    result.IsSuccess.Should().BeTrue();
}
```

**Solution 3**: Use `Map` to inspect without changing the value:

```csharp
var result = await GetOrdersAsync(userId)
    .Map(orders => 
    {
        _logger.LogDebug("Order count: {Count}, Total: {Total}", 
            orders.Count, orders.Sum(o => o.Total));
        return orders;  // Return unchanged
    })
    .BindAsync(orders => ProcessOrdersAsync(orders));
```

### 3. Debugging Async Chains

**Problem**: Async chains are harder to step through in the debugger.

**Solution**: Break async chains into named variables:

```csharp
// Instead of one long chain
var result = await GetUserAsync(id)
    .BindAsync(u => GetOrdersAsync(u.Id))
    .MapAsync(orders => ProcessOrders(orders));

// Break it up for debugging
var userResult = await GetUserAsync(id);  // Set breakpoint here
if (userResult.IsFailure) return userResult;

var ordersResult = await GetOrdersAsync(userResult.Value.Id);  // Breakpoint
if (ordersResult.IsFailure) return ordersResult;

var processed = ordersResult.Map(orders => ProcessOrders(orders));  // Breakpoint
return processed;
```

### 4. Debugging Aggregated Errors

**Problem**: When using `Combine`, all errors are collected. Which validations failed?

```csharp
var result = EmailAddress.TryCreate("invalid")
    .Combine(FirstName.TryCreate(""))
    .Combine(Age.TryCreate(-5));
// Might fail with 3 errors - which ones?
```

**Solution**: Use `TapError` to log aggregated errors:

```csharp
var result = EmailAddress.TryCreate(email)
    .Combine(FirstName.TryCreate(firstName))
    .Combine(Age.TryCreate(age))
    .TapError(error => 
    {
        if (error is AggregateError aggregated)
        {
            foreach (var err in aggregated.Errors)
            {
                _logger.LogWarning("Validation failed: {Message}", err.Detail);
            }
        }
    });
```

### 5. Testing Individual Steps

**Problem**: A complex chain makes it hard to test individual operations.

**Solution**: Extract operations into testable methods:

```csharp
// Instead of inline
public Result<User> ValidateAndProcessUser(string id)
{
    return GetUser(id)
        .Ensure(u => u.IsActive, Error.Validation("Inactive"))
        .Ensure(u => u.Email.Contains("@"), Error.Validation("Invalid email"))
        .Tap(u => u.LastLoginAt = DateTime.UtcNow);
}

// Extract testable pieces
public Result<User> GetActiveUser(string id) =>
    GetUser(id).Ensure(u => u.IsActive, Error.Validation("User inactive"));

public Result<User> ValidateUserEmail(User user) =>
    user.Email.Contains("@") 
        ? Result.Success(user)
        : Error.Validation("Invalid email");

public void UpdateLastLogin(User user) =>
    user.LastLoginAt = DateTime.UtcNow;

// Compose
public Result<User> ValidateAndProcessUser(string id) =>
    GetActiveUser(id)
        .Bind(ValidateUserEmail)
        .Tap(UpdateLastLogin);

// Easy to test
[Fact]
public void GetActiveUser_Should_Fail_For_Inactive_User()
{
    var result = GetActiveUser("inactive-id");
    result.IsFailure.Should().BeTrue();
}
```

## Debugging Tools & Techniques

### Conditional Breakpoints

Set conditional breakpoints in `Tap` operations:

```csharp
var result = ProcessUsers(users)
    .Tap(user => 
    {
        // Set breakpoint here with condition: user.Id == "problem-id"
        if (user.Id == "problem-id")
        {
            var state = new { user.Id, user.Status, user.Email };
            Console.WriteLine($"Problem user state: {state}");
        }
    });
```

### FluentAssertions for Tests

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

[Fact]
public void Should_Return_Processed_User()
{
    var result = ProcessUser(validUserId);
    
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Status.Should().Be(UserStatus.Active);
}
```

### Logging Strategies

#### Application-Level Logging

```csharp
public async Task<Result<Order>> ProcessOrderAsync(OrderId orderId)
{
    _logger.LogInformation("Processing order {OrderId}", orderId);
    
    return await GetOrderAsync(orderId)
        .TapError(err => _logger.LogWarning(
            "Failed to get order {OrderId}: {Error}", orderId, err.Detail))
        .BindAsync(order => ValidateOrderAsync(order))
        .TapError(err => _logger.LogWarning(
            "Order {OrderId} validation failed: {Error}", orderId, err.Detail))
        .BindAsync(order => ProcessPaymentAsync(order))
        .Tap(order => _logger.LogInformation(
            "Successfully processed order {OrderId}", order.Id))
        .TapError(err => _logger.LogError(
            "Order {OrderId} processing failed: {Error}", orderId, err.Detail));
}
```

#### Structured Logging with Context

```csharp
public async Task<Result<User>> RegisterUserAsync(UserRegistration registration)
{
    using var scope = _logger.BeginScope(new Dictionary<string, object>
    {
        ["Email"] = registration.Email,
        ["RegistrationSource"] = registration.Source
    });
    
    return await ValidateEmailAsync(registration.Email)
        .Tap(_ => _logger.LogDebug("Email validated"))
        .BindAsync(email => CreateUserAsync(email, registration))
        .Tap(user => _logger.LogInformation("User created: {UserId}", user.Id))
        .TapError(err => _logger.LogWarning("Registration failed: {Error}", err));
}
```

## Best Practices

### 1. Use Descriptive Error Messages

```csharp
// ❌ Bad - Generic error
.Ensure(user => user.Age >= 18, Error.Validation("Invalid age"))

// ✅ Good - Specific, actionable error
.Ensure(user => user.Age >= 18, 
    Error.Validation($"User {user.Id} must be 18 or older. Current age: {user.Age}"))
```

### 2. Add Context to Errors

```csharp
// ✅ Include relevant IDs in error detail and instance
return await GetOrderAsync(orderId)
    .ToResultAsync(Error.NotFound(
        $\"Order {orderId} not found for user {userId}\",
        $\"order-{orderId}\"\n    ));
```

### 3. Use Tap for Side Effects, Not Logic

```csharp
// ❌ Bad - Logic in Tap
.Tap(user => user.IsActive = true)

// ✅ Good - Pure transformation
.Map(user => user with { IsActive = true })

// ✅ Good - True side effect (logging, metrics)
.Tap(user => _logger.LogInformation("User activated: {UserId}", user.Id))
```

### 4. Break Long Chains

```csharp
// ❌ Hard to debug
var result = Step1().Bind(Step2).Bind(Step3).Bind(Step4).Bind(Step5).Match(...);

// ✅ Easier to debug
var step1Result = Step1();
var step2Result = step1Result.Bind(Step2);
var step3Result = step2Result.Bind(Step3);
// Can set breakpoints and inspect each step
```

### 5. Name Your Lambdas

```csharp
// ❌ Anonymous lambda - hard to see in call stack
.BindAsync(x => ProcessAsync(x))

// ✅ Named method - shows in call stack
.BindAsync(ProcessOrderAsync)

async Task<Result<Order>> ProcessOrderAsync(Order order)
{
    // Implementation
}
```

## Debugging Checklist

When debugging a failing ROP chain:

- [ ] **Check the error message** - Does it tell you which operation failed?
- [ ] **Add `Tap` or `TapError`** - Log at each step to find the failure point
- [ ] **Break the chain** - Split into smaller variables for inspection
- [ ] **Check aggregated errors** - Are multiple validations failing?
- [ ] **Verify async operations** - Is `CancellationToken` passed correctly?
- [ ] **Review error codes** - Are custom error codes being used consistently?
- [ ] **Test individual operations** - Extract and test each step separately
- [ ] **Check for null values** - Is `ToResult` being used for nullable types?
- [ ] **Inspect metadata** - Does the error include debugging context?
- [ ] **Add structured logging** - Use correlation IDs and scopes

## Common Pitfalls

### Forgetting ToResult/ToResultAsync

```csharp
// ❌ Nullable<T> doesn't automatically convert to Result
User? user = await _repository.GetByIdAsync(userId);
return user.Bind(u => ProcessUser(u));  // Compile error!

// ✅ Convert nullable to Result first
return await _repository.GetByIdAsync(userId)
    .ToResultAsync(Error.NotFound($"User {userId} not found"))
    .BindAsync(ProcessUserAsync);
```

### Accessing Value on Failure

```csharp
// ❌ Throws exception if result is failure
var result = GetUser(userId);
var userName = result.Value.Name;  // Boom!

// ✅ Check state first
if (result.IsSuccess)
{
    var userName = result.Value.Name;
}

// ✅ Or use Match
var userName = result.Match(
    onSuccess: user => user.Name,
    onFailure: _ => "Unknown"
);
```

### Mixing Result and Exceptions

```csharp
// ❌ Don't throw in ROP chains
.Bind(x => 
{
    if (x.IsInvalid) throw new InvalidOperationException();
    return Result.Success(x);
})

// ✅ Return Result
.Bind(x => 
    x.IsInvalid 
        ? Error.Validation("Invalid operation")
        : Result.Success(x)
)
```

## Next Steps

- See [Advanced Features](advanced-features.md) for LINQ query syntax and parallel operations
- Learn about [Error Handling](error-handling.md) for discriminated error matching
- Check [Performance](performance.md) for benchmarking your ROP chains
