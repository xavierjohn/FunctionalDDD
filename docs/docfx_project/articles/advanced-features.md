# Advanced Features

Advanced Railway Oriented Programming patterns for complex scenarios.

## Table of Contents

- [Pattern Matching](#pattern-matching)
- [Tuple Destructuring](#tuple-destructuring)
- [Exception Capture](#exception-capture)
- [Parallel Operations](#parallel-operations)
- [LINQ Query Syntax](#linq-query-syntax)
- [Maybe Type](#maybe-type)

## Pattern Matching

`Match` handles both success and failure cases elegantly:

### Basic Pattern Matching

```csharp
var description = GetUser("123").Match(
    onSuccess: user => $"User: {user.Name}",
    onFailure: error => $"Error: {error.Code}"
);
```

### Async Pattern Matching

```csharp
await ProcessOrderAsync(order).MatchAsync(
    onSuccess: async order => await SendConfirmationAsync(order),
    onFailure: async error => await LogErrorAsync(error)
);
```

### With HTTP Results

```csharp
app.MapGet("/users/{id}", async (string id) =>
{
    return await GetUserAsync(id)
        .ToResultAsync(Error.NotFound($"User {id} not found"))
        .MatchAsync(
            onSuccess: user => Results.Ok(user),
            onFailure: error => error.ToHttpResult()
        );
});
```

## Tuple Destructuring

Automatically destructure tuples in Match and Bind operations:

### Automatic Destructuring

```csharp
var result = EmailAddress.TryCreate(email)
    .Combine(UserId.TryCreate(userId))
    .Combine(OrderId.TryCreate(orderId))
    .Match(
        // Tuple automatically destructured into named parameters
        onSuccess: (email, userId, orderId) => 
            $"Order {orderId} for user {userId} at {email}",
        onFailure: error => 
            $"Validation failed: {error.Detail}"
    );
```

### Tuple Destructuring in Bind

```csharp
var result = FirstName.TryCreate("John")
    .Combine(LastName.TryCreate("Smith"))
    .Combine(EmailAddress.TryCreate("john@example.com"))
    .Bind((firstName, lastName, email) => 
        CreateUser(firstName, lastName, email)
    );
```

### Support for 2-9 Parameters

Tuple destructuring supports 2 to 9 combined values:

```csharp
// Works with any number of combined results
var result = value1.TryCreate()
    .Combine(value2.TryCreate())
    .Combine(value3.TryCreate())
    .Combine(value4.TryCreate())
    // ... up to 9 values
    .Bind((v1, v2, v3, v4, /* ... */) => ProcessAll(...));
```

## Exception Capture

Convert exception-throwing code into Results using `Try` and `TryAsync`:

### Synchronous Exception Capture

```csharp
Result<string> LoadFile(string path)
{
    return Result.Try(() => File.ReadAllText(path));
}

// Usage
var content = LoadFile("config.json")
    .Ensure(c => !string.IsNullOrEmpty(c), 
           Error.Validation("File is empty"))
    .Bind(ParseConfig);
```

### Async Exception Capture

```csharp
async Task<Result<User>> FetchUserAsync(string url)
{
    return await Result.TryAsync(async () => 
        await _httpClient.GetFromJsonAsync<User>(url));
}

// Usage with chaining
var user = await FetchUserAsync(apiUrl)
    .EnsureAsync(u => u != null, Error.NotFound("User not found"))
    .TapAsync(u => LogUserAccessAsync(u.Id));
```

### Custom Exception Mapping

```csharp
Result<string> ReadFileWithCustomErrors(string path)
{
    return Result.Try(
        () => File.ReadAllText(path),
        exception => exception switch
        {
            FileNotFoundException => Error.NotFound($"File not found: {path}"),
            UnauthorizedAccessException => Error.Forbidden("Access denied"),
            _ => Error.Unexpected(exception.Message)
        }
    );
}
```

## Parallel Operations

Execute multiple async operations in parallel while maintaining ROP style:

### Parallel Execution with WhenAll

```csharp
// Start all operations in parallel
var userTask = GetUserAsync(userId);
var ordersTask = GetOrdersAsync(userId);
var preferencesTask = GetPreferencesAsync(userId);

// Combine results
var result = await Task.WhenAll(userTask, ordersTask, preferencesTask)
    .ThenAsync(results => results[0]
        .Combine(results[1])
        .Combine(results[2])
    )
    .BindAsync((user, orders, preferences) => 
        CreateDashboard(user, orders, preferences)
    );
```

### Parallel with Result Collection

```csharp
// Execute multiple validations in parallel
var tasks = userIds.Select(id => ValidateUserAsync(id));
var results = await Task.WhenAll(tasks);

// Combine all results - fails if any validation fails
var combinedResult = results.Aggregate(
    Result.Success(Enumerable.Empty<User>()),
    (acc, result) => acc.Combine(result)
);
```

### Real-World Example: Fraud Detection

```csharp
public async Task<Result<Transaction>> ProcessTransactionAsync(
    Transaction transaction,
    CancellationToken ct)
{
    // Run all fraud checks in parallel
    var checksTask = Task.WhenAll(
        CheckBlacklistAsync(transaction.AccountId, ct),
        CheckVelocityLimitsAsync(transaction, ct),
        CheckAmountThresholdAsync(transaction, ct),
        CheckGeolocationAsync(transaction, ct)
    );
    
    return await checksTask
        .ThenAsync(checks => checks.Aggregate(
            Result.Success(transaction),
            (result, check) => result.Combine(check)
        ))
        .BindAsync((tx, ct) => ApproveTransactionAsync(tx, ct), ct);
}
```

## LINQ Query Syntax

Use C#'s LINQ query syntax for readable multi-step operations:

### Basic LINQ Query

```csharp
var result =
    from user in GetUser(userId)
    from order in GetLastOrder(user)
    from payment in ProcessPayment(order)
    select new OrderConfirmation(user, order, payment);
```

### LINQ with Additional Operations

```csharp
var result =
    from email in EmailAddress.TryCreate(emailInput)
    from user in GetUserByEmail(email)
    let isActive = user.IsActive
    where isActive
    from orders in GetUserOrders(user.Id)
    select new UserSummary(user, orders);
```

### LINQ with Async Operations

```csharp
var result = await (
    from userId in UserId.TryCreate(userIdInput)
    from user in GetUserAsync(userId)
    from permissions in GetPermissionsAsync(user.Id)
    select new UserWithPermissions(user, permissions)
).ConfigureAwait(false);
```

## Maybe Type

`Maybe<T>` represents an optional value that may or may not exist, without implying an error:

### Creating Maybe Values

```csharp
// From nullable value
Maybe<User> user = Maybe.From(nullableUser);

// Implicit conversion
Maybe<string> some = "value";
Maybe<string> none = Maybe.None<string>();

// Checking for value
if (user.HasValue)
{
    Console.WriteLine($"Hello {user.Value.Name}");
}
else
{
    Console.WriteLine("No user found");
}
```

### Maybe vs Result

Use `Maybe<T>` when absence is **not an error** (optional data):
```csharp
Maybe<string> middleName = GetMiddleName(user); // OK to be missing
```

Use `Result<T>` when you need to **track why** something failed:
```csharp
Result<User> user = GetUser(id); // Need to know why it failed
```

### Converting Maybe to Result

```csharp
Maybe<User> maybeUser = FindUserInCache(id);

Result<User> result = maybeUser
    .ToResult(Error.NotFound($"User {id} not found in cache"));
```

### Using Maybe Operations

```csharp
Maybe<User> maybeUser = GetUserById(id);

// Convert to Result for chaining
var result = maybeUser
    .ToResult(Error.NotFound($"User {id} not found"))
    .Bind(user => GetEmailPreferences(user.Email));

// Or use directly with HasValue
string preferences = maybeUser.HasValue 
    ? GetEmailPreferences(maybeUser.Value.Email)
    : "No preferences found";
```

## Best Practices

1. **Use Try for Third-Party Code**: Wrap exception-throwing code with `Result.Try` or `Result.TryAsync`
2. **Leverage Tuples**: Use tuple destructuring for combining multiple validations
3. **Parallel When Possible**: Use `Task.WhenAll` for independent async operations
4. **Choose Maybe vs Result Carefully**: Use Maybe for optional data, Result for operations that can fail
5. **LINQ for Readability**: Use LINQ query syntax for complex multi-step operations

## Next Steps

- Learn about [Error Handling](error-handling.md) for discriminated error matching
- See [Async & Cancellation](async-cancellation.md) for CancellationToken support
- Check [Integration](integration.md) for ASP.NET and FluentValidation usage
