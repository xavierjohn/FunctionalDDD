# Async Operations & Cancellation

This guide covers async patterns and CancellationToken integration for Railway Oriented Programming.

## Table of Contents

- [Async Operation Basics](#async-operation-basics)
- [CancellationToken Support](#cancellationtoken-support)
- [Async with Tuples](#async-with-tuples)
- [Parallel Operations](#parallel-operations)
- [Timeout Patterns](#timeout-patterns)
- [Best Practices](#best-practices)

## Async Operation Basics

Every ROP operation has an async variant with the `Async` suffix:

### Core Async Operations

```csharp
var ct = cancellationToken;

// BindAsync - Chain async operations
var result = await GetUserAsync(userId, ct)
    .BindAsync(user => GetOrdersAsync(user.Id, ct))
    .BindAsync(orders => ProcessOrdersAsync(orders, ct));

// MapAsync - Transform values asynchronously
var result = await GetUserAsync(userId, ct)
    .MapAsync(user => user.Adapt<UserDto>())
    .MapAsync(dto => EnrichDtoAsync(dto, ct));

// TapAsync - Async side effects
var result = await CreateOrderAsync(order, ct)
    .TapAsync(order => SendEmailAsync(order.CustomerEmail, ct))
    .TapAsync(order => LogOrderCreatedAsync(order.Id, ct));

// EnsureAsync - Async validation
var result = await GetUserAsync(userId, ct)
    .EnsureAsync(
        user => CheckUserIsActiveAsync(user, ct),
        Error.Validation("User is not active"));

// MatchAsync - Async result handling
await ProcessOrderAsync(order, ct)
    .MatchAsync(
        onSuccess: order => SendConfirmationAsync(order, ct),
        onFailure: error -> LogErrorAsync(error, ct));
```

### Mixing Sync and Async

You can mix synchronous and asynchronous operations seamlessly:

```csharp
var ct = cancellationToken;

var result = await GetUserAsync(userId, ct)          // Async
    .Map(user => user.Email)                          // Sync
    .BindAsync(email => ValidateEmailAsync(email, ct)) // Async
    .Ensure(email => email.Length > 5,                // Sync
            Error.Validation("Email too short"))
    .TapAsync(email => LogEmailAsync(email, ct));     // Async
```

### Expression-Body Style

Use expression bodies for clean, concise async methods:

```csharp
// ✅ Clean expression-body style with lambda capture
public async Task<ActionResult<UserDto>> GetUser(string id, CancellationToken cancellationToken)
{
    var ct = cancellationToken;
    return await UserId.TryCreate(id)
        .BindAsync(userId => _repository.GetByIdAsync(userId, ct))
        .MapAsync(user => user.Adapt<UserDto>())
        .ToActionResultAsync(this);
}

// ✅ Repository method
public async Task<Result<User>> GetByIdAsync(UserId userId, CancellationToken ct)
    => await _context.Users
        .FirstOrDefaultAsync(u => u.Id == userId, ct)
        .ToResultAsync(Error.NotFound($"User {userId} not found"));
```

## CancellationToken Support

All async operations support `CancellationToken` for graceful cancellation using **lambda closure capture**.

### Lambda Capture Pattern

The recommended approach is to capture the `CancellationToken` in lambdas:

```csharp
public async Task<Result<Order>> ProcessOrderAsync(
    OrderId orderId,
    CancellationToken cancellationToken)
{
    var ct = cancellationToken;
    
    return await GetOrderAsync(orderId, ct)
        .EnsureAsync(
            order => ValidateOrderAsync(order, ct),
            Error.Validation("Order validation failed"))
        .TapAsync(order => NotifyWarehouseAsync(order, ct))
        .BindAsync(order => ProcessPaymentAsync(order, ct));
}
```

**Why this pattern?**
- ✅ Concise and readable
- ✅ Works seamlessly with tuple destructuring
- ✅ Consistent with modern C# style
- ✅ Single source of truth for the token
- ✅ Simplifies method signatures

## Async with Tuples

CancellationToken works seamlessly with tuple-based operations using lambda closure:

### Combine with BindAsync

```csharp
public async Task<Result<Order>> CreateOrderAsync(
    CreateOrderRequest request,
    CancellationToken cancellationToken)
{
    var ct = cancellationToken;
    
    return await EmailAddress.TryCreate(request.Email)
        .Combine(UserId.TryCreate(request.UserId))
        .Combine(OrderId.TryCreate(request.OrderId))
        .BindAsync((email, userId, orderId) => 
            SaveOrderAsync(email, userId, orderId, ct));
}
```

### Combine with MapAsync

```csharp
public async Task<Result<UserProfile>> GetUserProfileAsync(
    UserId userId,
    CancellationToken cancellationToken)
{
    var ct = cancellationToken;
    
    return await GetUserAsync(userId, ct)
        .ParallelAsync(GetSettingsAsync(userId, ct))
        .ParallelAsync(GetPreferencesAsync(userId, ct))
        .AwaitAsync()
        .MapAsync((user, settings, preferences) =>
            new UserProfile(user, settings, preferences));
}
```

### Ensure with Tuples

```csharp
public async Task<Result<Reservation>> CheckInventoryAsync(
    ProductId productId,
    int quantity,
    CancellationToken cancellationToken)
{
    var ct = cancellationToken;
    
    return await GetProductAsync(productId, ct)
        .ParallelAsync(GetInventoryAsync(productId, ct))
        .AwaitAsync()
        .EnsureAsync(
            (product, inventory) => 
                CheckAvailabilityAsync(inventory, quantity, ct),
            Error.Conflict("Insufficient inventory"));
}
```

### Tap with Tuples

```csharp
public async Task<Result<OrderConfirmation>> ProcessOrderAsync(
    OrderId orderId,
    CancellationToken cancellationToken)
{
    var ct = cancellationToken;
    
    return await GetOrderAsync(orderId, ct)
        .ParallelAsync(GetCustomerAsync(orderId, ct))
        .AwaitAsync()
        .TapAsync((order, customer) => 
            LogOrderProcessedAsync(order.Id, customer.Id, ct))
        .MapAsync((order, customer) => 
            new OrderConfirmation(order, customer));
}
```

## Parallel Operations

Execute multiple async operations concurrently with `CancellationToken` support:

### Basic Parallel Execution

```csharp
public async Task<Result<StudentReport>> GetStudentReportAsync(
    StudentId studentId,
    CancellationToken cancellationToken)
{
    var ct = cancellationToken;
    
    return await GetStudentInfoAsync(studentId, ct)
        .ParallelAsync(GetGradesAsync(studentId, ct))
        .ParallelAsync(GetAttendanceAsync(studentId, ct))
        .ParallelAsync(GetLibraryBooksAsync(studentId, ct))
        .AwaitAsync()
        .MapAsync((info, grades, attendance, books) =>
            new StudentReport(info, grades, attendance, books));
}
```

**Key Points:**
- All operations run **concurrently**
- `AwaitAsync()` waits for all to complete
- If any operation fails, the entire chain fails
- Cancellation propagates to all parallel operations

### Parallel Validation

```csharp
public async Task<Result<Transaction>> ValidateTransactionAsync(
    Transaction transaction,
    CancellationToken cancellationToken)
{
    var ct = cancellationToken;
    
    return await CheckBlacklistAsync(transaction.AccountId, ct)
        .ParallelAsync(CheckVelocityLimitsAsync(transaction, ct))
        .ParallelAsync(CheckAmountThresholdAsync(transaction, ct))
        .ParallelAsync(CheckGeolocationAsync(transaction, ct))
        .AwaitAsync()
        .BindAsync((check1, check2, check3, check4) =>
            ApproveTransactionAsync(transaction, ct));
}
```

## Timeout Patterns

Implement timeouts using `CancellationTokenSource`:

### Simple Timeout

```csharp
public async Task<Result<User>> GetUserWithTimeoutAsync(
    UserId userId,
    CancellationToken cancellationToken)
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
    var ct = linked.Token;
    
    return await GetUserAsync(userId, ct)
        .BindAsync(user => GetOrdersAsync(user.Id, ct));
}
```

### Timeout with Fallback

```csharp
public async Task<Result<User>> GetUserWithFallbackAsync(
    UserId userId,
    CancellationToken cancellationToken)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    cts.CancelAfter(TimeSpan.FromSeconds(5));
    var ct = cts.Token;

    try
    {
        return await GetUserFromPrimaryAsync(userId, ct)
            .CompensateAsync(
                predicate: error => error is ServiceUnavailableError,
                func: () => GetUserFromSecondaryAsync(userId, cancellationToken));
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
        // Primary service timed out, use cache
        return await GetUserFromCacheAsync(userId, cancellationToken);
    }
}
```

### ASP.NET Core Request Timeout

```csharp
app.MapPost("/orders", async (
    CreateOrderRequest request,
    IMediator mediator,
    CancellationToken cancellationToken) =>
{
    var ct = cancellationToken;
    
    return await CreateOrderCommand.TryCreate(request)
        .BindAsync(command => mediator.Send(command, ct))
        .MapAsync(order => order.Adapt<OrderDto>())
        .MatchErrorAsync(
            onValidation: err => Results.BadRequest(new { errors = err.FieldErrors }),
            onNotFound: err => Results.NotFound(new { message = err.Detail }),
            onConflict: err => Results.Conflict(new { message = err.Detail }),
            onSuccess: order => Results.Created($"/orders/{order.Id}", order));
});
```

**Note:** ASP.NET Core automatically cancels the `CancellationToken` when:
- Client disconnects
- Request timeout is reached
- Application is shutting down

## Best Practices

### 1. Always Accept CancellationToken

```csharp
// ✅ Good - Supports cancellation
public async Task<Result<User>> GetUserAsync(UserId userId, CancellationToken ct)
    => await _repository.GetByIdAsync(userId, ct)
        .ToResultAsync(Error.NotFound($"User {userId} not found"));

// ❌ Bad - No cancellation support
public async Task<Result<User>> GetUserAsync(UserId userId)
    => await _repository.GetByIdAsync(userId)
        .ToResultAsync(Error.NotFound($"User not found"));
```

### 2. Use Lambda Capture for Token Propagation

```csharp
// ✅ Good - Capture token in local variable
public async Task<Result<Order>> ProcessOrderAsync(Order order, CancellationToken cancellationToken)
{
    var ct = cancellationToken;
    return await ValidateOrderAsync(order, ct)
        .BindAsync(o => SaveOrderAsync(o, ct))
        .TapAsync(o => PublishEventAsync(o, ct));
}

// ❌ Bad - Passing cancellationToken repeatedly
public async Task<Result<Order>> ProcessOrderAsync(Order order, CancellationToken cancellationToken)
    => await ValidateOrderAsync(order, cancellationToken)
        .BindAsync(o => SaveOrderAsync(o, cancellationToken))
        .TapAsync(o => PublishEventAsync(o, cancellationToken));
```

### 3. Pass CancellationToken Through Chains

```csharp
// ✅ Good - CT passed through entire chain
public async Task<Result<Dashboard>> GetDashboardAsync(UserId userId, CancellationToken cancellationToken)
{
    var ct = cancellationToken;
    return await GetUserAsync(userId, ct)
        .BindAsync(user => GetOrdersAsync(user.Id, ct))
        .TapAsync(orders => LogAsync(orders, ct));
}

// ❌ Bad - CT not passed (ignores cancellation)
public async Task<Result<Dashboard>> GetDashboardAsync(UserId userId, CancellationToken cancellationToken)
{
    var ct = cancellationToken;
    return await GetUserAsync(userId, ct)
        .BindAsync(user => GetOrdersAsync(user.Id, default))
        .TapAsync(orders => LogAsync(orders, default));
}
```

### 4. Don't Catch OperationCanceledException

```csharp
// ✅ Good - Let cancellation propagate
public async Task<Result<Order>> ProcessOrderAsync(Order order, CancellationToken cancellationToken)
{
    var ct = cancellationToken;
    return await ValidateOrderAsync(order, ct)
        .BindAsync(o => SaveOrderAsync(o, ct));
}

// ❌ Bad - Swallowing cancellation
public async Task<Result<Order>> ProcessOrderAsync(Order order, CancellationToken cancellationToken)
{
    try
    {
        var ct = cancellationToken;
        return await ValidateOrderAsync(order, ct)
            .BindAsync(o => SaveOrderAsync(o, ct));
    }
    catch (OperationCanceledException)
    {
        return Error.Unexpected("Operation was cancelled");
    }
}
```

**Why?** ASP.NET Core and other frameworks handle `OperationCanceledException` gracefully. Let it propagate naturally.

### 5. Never Block Async Code

```csharp
// ❌ Bad - Blocks the thread, deadlock risk
public Result<User> GetUser(UserId userId)
    => GetUserAsync(userId, CancellationToken.None).Result;

// ❌ Bad - Same problem with .Wait()
public Result<User> GetUser(UserId userId)
{
    var task = GetUserAsync(userId, CancellationToken.None);
    task.Wait();
    return task.Result;
}

// ✅ Good - Keep it async all the way
public async Task<Result<User>> GetUserAsync(UserId userId, CancellationToken ct)
    => await _repository.GetByIdAsync(userId, ct)
        .ToResultAsync(Error.NotFound($"User {userId} not found"));
```

### 6. Link CancellationTokens for Composite Operations

```csharp
public async Task<Result<Report>> GenerateReportAsync(
    ReportId reportId,
    CancellationToken cancellationToken)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    cts.CancelAfter(TimeSpan.FromMinutes(5));
    var ct = cts.Token;

    return await GetReportDataAsync(reportId, ct)
        .BindAsync(data => ProcessDataAsync(data, ct))
        .BindAsync(processed => GeneratePdfAsync(processed, ct));
}
```

### 7. Use Parallel Operations for Independent Work

```csharp
// ✅ Good - Parallel execution for independent operations
public async Task<Result<Summary>> GetSummaryAsync(UserId userId, CancellationToken cancellationToken)
{
    var ct = cancellationToken;
    return await GetUserAsync(userId, ct)
        .ParallelAsync(GetOrdersAsync(userId, ct))
        .ParallelAsync(GetInvoicesAsync(userId, ct))
        .AwaitAsync()
        .MapAsync((user, orders, invoices) => 
            new Summary(user, orders, invoices));
}

// ❌ Bad - Sequential execution (slower)
public async Task<Result<Summary>> GetSummaryAsync(UserId userId, CancellationToken cancellationToken)
{
    var ct = cancellationToken;
    return await GetUserAsync(userId, ct)
        .BindAsync(user => GetOrdersAsync(userId, ct)
            .Bind(orders => GetInvoicesAsync(userId, ct)
                .Map(invoices => new Summary(user, orders, invoices))));
}
```

## Common Patterns

### Long-Running Background Operations

```csharp
public async Task ProcessQueueAsync(CancellationToken cancellationToken)
{
    var ct = cancellationToken;
    
    while (!ct.IsCancellationRequested)
    {
        var result = await GetNextMessageAsync(ct)
            .BindAsync(msg => ProcessMessageAsync(msg, ct))
            .TapAsync(processed => AcknowledgeMessageAsync(processed, ct))
            .TapErrorAsync(error => LogErrorAsync(error, ct));

        if (result.IsFailure)
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
    }
}
```

### Retry with Exponential Backoff

```csharp
public async Task<Result<T>> RetryAsync<T>(
    Func<CancellationToken, Task<Result<T>>> operation,
    int maxAttempts,
    CancellationToken ct)
{
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        ct.ThrowIfCancellationRequested();

        var result = await operation(ct);
        
        if (result.IsSuccess || attempt == maxAttempts)
            return result;

        var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
        await Task.Delay(delay, ct);
    }

    return Result.Failure<T>(Error.Unexpected("Max retries exceeded"));
}

// Usage with lambda capture
public async Task<Result<User>> GetUserWithRetryAsync(UserId userId, CancellationToken cancellationToken)
    => await RetryAsync(
        ct => GetUserAsync(userId, ct),
        maxAttempts: 3,
        cancellationToken);
```

### Batch Processing with Cancellation

```csharp
public async Task<Result<BatchResult>> ProcessBatchAsync(
    IEnumerable<Order> orders,
    CancellationToken ct)
{
    var results = new List<Result<ProcessedOrder>>();
    
    foreach (var order in orders)
    {
        ct.ThrowIfCancellationRequested();
        
        var result = await ProcessOrderAsync(order, ct);
        results.Add(result);
    }
    
    return BatchResult.Create(results);
}
```

### Timeout with Retry

```csharp
public async Task<Result<T>> GetWithTimeoutAndRetryAsync<T>(
    Func<CancellationToken, Task<Result<T>>> operation,
    TimeSpan timeout,
    int maxRetries,
    CancellationToken cancellationToken)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            return await operation(cts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            if (attempt == maxRetries)
                return Result.Failure<T>(Error.Unexpected("Operation timed out after retries"));
            
            await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
        }
    }

    return Result.Failure<T>(Error.Unexpected("Max retries exceeded"));
}
```

## Performance Considerations

### ConfigureAwait is Optional in Modern .NET

In .NET 6+ and ASP.NET Core applications, you generally **don't need** `ConfigureAwait(false)`:

```csharp
// ✅ Fine for ASP.NET Core / Console apps (.NET 6+)
public async Task<Result<User>> GetUserAsync(UserId userId, CancellationToken ct)
    => await _repository.GetByIdAsync(userId, ct)
        .ToResultAsync(Error.NotFound($"User {userId} not found"));

// ✅ Use ConfigureAwait(false) ONLY in library code targeting .NET Standard
public async Task<Result<User>> GetUserAsync(UserId userId, CancellationToken ct)
    => await _repository.GetByIdAsync(userId, ct)
        .ConfigureAwait(false)
        .ToResultAsync(Error.NotFound($"User {userId} not found"));
```

**When to use `ConfigureAwait(false)`:**
- You're writing a library targeting .NET Standard 2.0
- You need maximum performance in library code
- You're not using any `SynchronizationContext` features

**When NOT to use it:**
- ASP.NET Core applications (.NET 6+)
- Console applications
- Modern .NET applications with no special sync context

## Next Steps

- See [Error Handling](error-handling.md) for handling cancellation-related errors
- Check [Advanced Features](advanced-features.md) for more parallel patterns
- Learn about [Integration](integration.md) for ASP.NET Core best practices
- Review [Examples](examples.md) for complete real-world scenarios
