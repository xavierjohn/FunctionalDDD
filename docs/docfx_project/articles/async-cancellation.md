# Async Operations & Cancellation

This guide covers async patterns and CancellationToken integration for Railway Oriented Programming.

## Table of Contents

- [Async Operation Basics](#async-operation-basics)
- [CancellationToken Support](#cancellationtoken-support)
- [Async with Tuples](#async-with-tuples)
- [Timeout Patterns](#timeout-patterns)
- [Best Practices](#best-practices)

## Async Operation Basics

Every ROP operation has an async variant with the `Async` suffix:

### Core Async Operations

```csharp
// BindAsync - Chain async operations
var result = await GetUserAsync(userId)
    .BindAsync(user => GetOrdersAsync(user.Id))
    .BindAsync(orders => ProcessOrdersAsync(orders));

// MapAsync - Transform values asynchronously
var result = await GetUserAsync(userId)
    .MapAsync(user => user.ToDto())
    .MapAsync(dto => EnrichDtoAsync(dto));

// TapAsync - Async side effects
var result = await CreateOrderAsync(order)
    .TapAsync(order => SendEmailAsync(order.CustomerEmail))
    .TapAsync(order => LogOrderCreatedAsync(order.Id));

// EnsureAsync - Async validation
var result = await GetUserAsync(userId)
    .EnsureAsync(
        user => CheckUserIsActiveAsync(user),
        Error.Validation("User is not active")
    );

// MatchAsync - Async result handling
await ProcessOrderAsync(order)
    .MatchAsync(
        onSuccess: async order => await SendConfirmationAsync(order),
        onFailure: async error => await LogErrorAsync(error)
    );
```

### Mixing Sync and Async

You can mix synchronous and asynchronous operations:

```csharp
var result = await GetUserAsync(userId)          // Async
    .Map(user => user.Email)                      // Sync
    .BindAsync(email => ValidateEmailAsync(email)) // Async
    .Ensure(email => email.Length > 5,             // Sync
            Error.Validation("Email too short"))
    .TapAsync(email => LogEmailAsync(email));     // Async
```

## CancellationToken Support

All async operations support `CancellationToken` for graceful cancellation:

### Basic CancellationToken Usage

```csharp
public async Task<Result<Order>> ProcessOrderAsync(
    OrderId orderId,
    CancellationToken cancellationToken)
{
    return await GetOrderAsync(orderId, cancellationToken)
        .EnsureAsync(
            (order, ct) => ValidateOrderAsync(order, ct),
            Error.Validation("Order validation failed"),
            cancellationToken
        )
        .TapAsync(
            async (order, ct) => await NotifyWarehouseAsync(order, ct),
            cancellationToken
        )
        .BindAsync(
            (order, ct) => ProcessPaymentAsync(order, ct),
            cancellationToken
        );
}
```

### CancellationToken Patterns

#### Pattern 1: Pass to Each Operation

```csharp
async Task<Result<User>> GetUserAsync(string id, CancellationToken ct) { /* ... */ }
async Task<Result<Order>> GetLastOrderAsync(User user, CancellationToken ct) { /* ... */ }

var result = await GetUserAsync("123", cancellationToken)
    .BindAsync((user, ct) => GetLastOrderAsync(user, ct), cancellationToken);
```

#### Pattern 2: Lambda Capture

```csharp
var result = await GetUserAsync(userId, cancellationToken)
    .BindAsync(user => GetOrdersAsync(user.Id, cancellationToken))
    .TapAsync(orders => LogOrdersAsync(orders, cancellationToken));
```

#### Pattern 3: Mixed Approach

```csharp
public async Task<Result<Dashboard>> CreateDashboardAsync(
    UserId userId,
    CancellationToken ct)
{
    return await GetUserAsync(userId, ct)
        .BindAsync(
            // Explicit CT parameter
            (user, cancellationToken) => GetOrdersAsync(user.Id, cancellationToken),
            ct
        )
        .TapAsync(
            // Lambda capture
            orders => LogOrderCountAsync(orders.Count, ct),
            ct
        );
}
```

## Async with Tuples

CancellationToken works seamlessly with tuple-based operations:

### Bind with Tuples and CancellationToken

```csharp
var result = await EmailAddress.TryCreate(email)
    .Combine(UserId.TryCreate(userId))
    .Combine(OrderId.TryCreate(orderId))
    // Tuple automatically destructured, CT passed explicitly
    .BindAsync(
        (email, userId, orderId, ct) => 
            CreateOrderAsync(email, userId, orderId, ct),
        cancellationToken
    );
```

### Tap with Tuples and CancellationToken

```csharp
var result = await GetUserAsync(userId, ct)
    .Combine(await GetOrdersAsync(userId, ct))
    // Tuple destructuring with CancellationToken
    .TapAsync(
        async (user, orders, ct) => 
        {
            await LogUserActivityAsync(user.Id, ct);
            await CacheOrdersAsync(orders, ct);
        },
        cancellationToken
    );
```

### Ensure with Tuples and CancellationToken

```csharp
var result = await GetProductAsync(productId, ct)
    .Combine(await GetInventoryAsync(productId, ct))
    .EnsureAsync(
        async (product, inventory, ct) =>
        {
            var reserved = await GetReservedInventoryAsync(productId, ct);
            return inventory.Available >= reserved + requestedQty;
        },
        Error.Conflict("Insufficient inventory"),
        cancellationToken
    );
```

### Map with Tuples and CancellationToken

```csharp
var result = await GetUserAsync(userId, ct)
    .Combine(await GetSettingsAsync(userId, ct))
    .Combine(await GetPreferencesAsync(userId, ct))
    .MapAsync(
        async (user, settings, preferences, ct) =>
        {
            var enriched = await EnrichUserDataAsync(user, ct);
            return new UserProfile(enriched, settings, preferences);
        },
        cancellationToken
    );
```

## Timeout Patterns

Implement timeouts using `CancellationTokenSource`:

### Simple Timeout

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

var result = await GetUserAsync(userId, cts.Token)
    .BindAsync(
        (user, ct) => GetOrdersAsync(user.Id, ct),
        cts.Token
    );
```

### Timeout with Fallback

```csharp
public async Task<Result<User>> GetUserWithTimeoutAsync(
    UserId userId,
    CancellationToken ct)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    cts.CancelAfter(TimeSpan.FromSeconds(5));

    try
    {
        return await GetUserFromPrimaryAsync(userId, cts.Token)
            .CompensateAsync(error => 
                GetUserFromSecondaryAsync(userId, ct) // Use original CT, not timeout CT
            );
    }
    catch (OperationCanceledException) when (!ct.IsCancellationRequested)
    {
        // Timeout occurred, try fallback
        return await GetUserFromCacheAsync(userId, ct);
    }
}
```

### ASP.NET Core Request Timeout

```csharp
app.MapPost("/orders", async (
    CreateOrderRequest request,
    CancellationToken ct) =>
{
    // ct is automatically provided by ASP.NET Core
    // It's cancelled when the request is aborted
    return await ValidateOrderRequest(request)
        .BindAsync(
            (validatedRequest, cancellationToken) => 
                CreateOrderAsync(validatedRequest, cancellationToken),
            ct
        )
        .TapAsync(
            (order, cancellationToken) => 
                PublishOrderCreatedEventAsync(order, cancellationToken),
            ct
        )
        .MatchAsync(
            onSuccess: order => Results.Created($"/orders/{order.Id}", order),
            onFailure: error => error.ToHttpResult()
        );
});
```

## Best Practices

### 1. Always Accept CancellationToken

```csharp
// ✅ Good - Supports cancellation
public async Task<Result<User>> GetUserAsync(
    UserId userId, 
    CancellationToken cancellationToken)
{
    return await _repository.GetByIdAsync(userId, cancellationToken)
        .ToResultAsync(Error.NotFound($"User {userId} not found"));
}

// ❌ Bad - No cancellation support
public async Task<Result<User>> GetUserAsync(UserId userId)
{
    return await _repository.GetByIdAsync(userId)
        .ToResultAsync(Error.NotFound($"User {userId} not found"));
}
```

### 2. Pass CancellationToken Through Chains

```csharp
// ✅ Good - CT passed through entire chain
await GetUserAsync(userId, ct)
    .BindAsync((user, cancellationToken) => 
        GetOrdersAsync(user.Id, cancellationToken), ct)
    .TapAsync((orders, cancellationToken) => 
        LogAsync(orders, cancellationToken), ct);

// ❌ Bad - CT not passed
await GetUserAsync(userId, ct)
    .BindAsync(user => GetOrdersAsync(user.Id, default))
    .TapAsync(orders => LogAsync(orders, default));
```

### 3. Use ConfigureAwait in Libraries

```csharp
// ✅ Good - For library code
public async Task<Result<User>> GetUserAsync(
    UserId userId,
    CancellationToken ct)
{
    return await _repository
        .GetByIdAsync(userId, ct)
        .ConfigureAwait(false)
        .ToResultAsync(Error.NotFound($"User {userId} not found"));
}

// ✅ Also fine - For application code (ASP.NET, etc.)
public async Task<Result<User>> GetUserAsync(
    UserId userId,
    CancellationToken ct)
{
    return await _repository
        .GetByIdAsync(userId, ct)
        .ToResultAsync(Error.NotFound($"User {userId} not found"));
}
```

### 4. Handle OperationCanceledException Appropriately

```csharp
public async Task<Result<Order>> ProcessOrderAsync(
    Order order,
    CancellationToken ct)
{
    try
    {
        return await ValidateOrderAsync(order, ct)
            .BindAsync((o, cancellationToken) => 
                SaveOrderAsync(o, cancellationToken), ct);
    }
    catch (OperationCanceledException)
    {
        // Don't convert to error - let it propagate
        // ASP.NET Core handles this gracefully
        throw;
    }
}
```

### 5. Avoid Blocking Async Code

```csharp
// ❌ Bad - Blocks the thread
public Result<User> GetUser(UserId userId)
{
    return GetUserAsync(userId, CancellationToken.None)
        .Result; // Deadlock risk!
}

// ✅ Good - Keep it async
public async Task<Result<User>> GetUserAsync(
    UserId userId,
    CancellationToken ct)
{
    return await _repository.GetByIdAsync(userId, ct)
        .ToResultAsync(Error.NotFound($"User {userId} not found"));
}
```

### 6. Link CancellationTokens for Composite Operations

```csharp
public async Task<Result<Report>> GenerateReportAsync(
    ReportId reportId,
    CancellationToken ct)
{
    // Create a linked token source for internal timeout
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    cts.CancelAfter(TimeSpan.FromMinutes(5));

    return await GetReportDataAsync(reportId, cts.Token)
        .BindAsync((data, cancellationToken) => 
            ProcessDataAsync(data, cancellationToken), cts.Token)
        .BindAsync((processed, cancellationToken) => 
            GeneratePdfAsync(processed, cancellationToken), cts.Token);
}
```

## Common Patterns

### Long-Running Background Operations

```csharp
public async Task ProcessQueueAsync(CancellationToken ct)
{
    while (!ct.IsCancellationRequested)
    {
        var result = await GetNextMessageAsync(ct)
            .BindAsync((msg, cancellationToken) => 
                ProcessMessageAsync(msg, cancellationToken), ct)
            .TapAsync((processed, cancellationToken) => 
                AcknowledgeMessageAsync(processed, cancellationToken), ct)
            .CompensateAsync(error => 
                HandleErrorAsync(error, ct));

        if (result.IsFailure)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }
}
```

### Retry with Cancellation

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

        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
    }

    return Result.Failure<T>(Error.Unexpected("Max retries exceeded"));
}

// Usage
var result = await RetryAsync(
    ct => GetUserAsync(userId, ct),
    maxAttempts: 3,
    cancellationToken
);
```

## Next Steps

- See [Error Handling](error-handling.md) for handling cancellation-related errors
- Check [Advanced Features](advanced-features.md) for parallel async operations
- Learn about [Integration](integration.md) for ASP.NET Core cancellation
