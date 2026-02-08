# Examples

This page provides quick code snippets to get you started. For comprehensive real-world examples, see the [Examples Directory](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples).

## Real-World Examples

The repository includes production-ready examples demonstrating complete systems:

### 🛒 [E-Commerce Order Processing](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples/EcommerceExample)
Complete order processing with payment, inventory management, and email notifications. Demonstrates complex workflows, recovery patterns, and transaction-like behavior.

**Key Concepts**: Aggregate lifecycle, recovery, parallel validation, async workflows

### 🏦 [Banking Transactions](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples/BankingExample)
Banking system with fraud detection, daily limits, overdraft protection, and interest calculations. Shows security patterns and state machines.

**Key Concepts**: Fraud detection, parallel fraud checks, MFA, account freeze, audit trail

### 🌐 [Web API Integration](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples/SampleWeb/SampleWebApplication)
ASP.NET Core MVC and Minimal API examples with automatic error-to-HTTP status mapping.

**Key Concepts**: ToActionResult, ToHttpResult, API integration, HTTP status codes, automatic value object validation

See the [Examples README](https://github.com/xavierjohn/FunctionalDDD/tree/main/Examples/README.md) for a complete guide including complexity ratings, learning paths, and common patterns.

---

## Quick Code Snippets

### Compose Multiple Operations in a Single Chain

```csharp
await GetCustomerByIdAsync(id, cancellationToken)
    .ToResultAsync(Error.NotFound($"Customer {id} not found"))
    .EnsureAsync(customer => customer.CanBePromoted,
        Error.Validation("The customer has the highest status possible"))
    .TapAsync(customer => customer.Promote())
    .TapAsync(async (customer, ct) => 
        await EmailGateway.SendPromotionNotificationAsync(customer.Email, ct), 
        cancellationToken)
    .MatchAsync(
        onSuccess: _ => "Okay",
        onFailure: error => error.Detail
    );
```

**Explanation**:
- `GetCustomerByIdAsync` returns a `Customer?` (nullable)
- `ToResultAsync` converts `null` to a failure `Result` with `NotFoundError`
- `EnsureAsync` validates business rules (can the customer be promoted?)
- `TapAsync` executes side effects (promote the customer)
- `TapAsync` sends email notification (side effect - doesn't change the result)
- `MatchAsync` terminates the chain and returns a string

### Multi-Field Validation with Combine

```csharp
EmailAddress.TryCreate("user@example.com")
    .Combine(FirstName.TryCreate("John"))
    .Combine(LastName.TryCreate("Doe"))
    .Bind((email, firstName, lastName) =>
        User.Create(email, firstName, lastName));
```

**Key Points**:
- `Combine` validates multiple fields independently
- If **any** fail, all errors are collected (validation errors are merged)
- Tuple destructuring automatically unpacks the three values
- Avoiding primitive obsession prevents parameter confusion

### Validation with FluentValidation

This library integrates with [FluentValidation](https://docs.fluentvalidation.net). Domain validation logic can be reused at the API layer to return `BadRequest` with detailed validation errors.

```csharp
public class User : Aggregate<UserId>
{
    public FirstName FirstName { get; }
    public LastName LastName { get; }

    public static Result<User> TryCreate(FirstName firstName, LastName lastName)
    {
        var user = new User(firstName, lastName);
        return Validator.ValidateToResult(user);
    }

    private User(FirstName firstName, LastName lastName)
        : base(UserId.NewUnique())
    {
        FirstName = firstName;
        LastName = lastName;
    }

    // FluentValidation rules
    private static readonly InlineValidator<User> Validator = new()
    {
        v => v.RuleFor(x => x.FirstName).NotNull(),
        v => v.RuleFor(x => x.LastName).NotNull(),
    };
}
```

**API Response** when LastName is missing:

```http
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json; charset=utf-8

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-c86cd9b34ca9435b688ec3a6b905b8e4-5f4c286ce90f99cb-00",
  "errors": {
    "lastName": [
      "Last Name cannot be empty."
    ]
  }
}
```

### Running Parallel Async Operations

Execute multiple independent async operations concurrently for better performance:

```csharp
var result = await Result.ParallelAsync(
    () => GetStudentInfoAsync(studentId, cancellationToken),
    () => GetStudentGradesAsync(studentId, cancellationToken),
    () => GetLibraryBooksAsync(studentId, cancellationToken)
)
.WhenAllAsync()
.BindAsync(
    (info, grades, books, ct) => PrepareReportAsync(info, grades, books, ct),
    cancellationToken
);
```

**Key Points:**
- `Result.ParallelAsync` accepts factory functions that return `Task<Result<T>>`
- All three `Get*Async` operations run **concurrently** (not sequentially)
- `.WhenAllAsync()` waits for all operations to complete
- Results are automatically destructured into `(info, grades, books)` tuple
- `BindAsync` processes the combined results with `CancellationToken` support

**Performance:**
- **Sequential:** 3 × 50ms = 150ms
- **Parallel:** max(50ms, 50ms, 50ms) = ~50ms
- **3x faster!**

### Error Matching and Handling

Handle different error types with specific logic:

```csharp
return await ProcessOrderAsync(order, cancellationToken)
    .MatchErrorAsync(
        onValidation: err => 
            Results.BadRequest(new { 
                errors = err.FieldErrors.ToDictionary(
                    f => f.FieldName, 
                    f => f.Details.ToArray()
                )
            }),
        onNotFound: err => 
            Results.NotFound(new { message = err.Detail }),
        onConflict: err => 
            Results.Conflict(new { message = err.Detail }),
        onDomain: err =>
            Results.UnprocessableEntity(new { message = err.Detail }),
        onError: err => 
            Results.StatusCode(500),  // Fallback for all other errors
        onSuccess: order => 
            Results.Ok(new { orderId = order.Id }),
        cancellationToken: cancellationToken
    );
```

**Key Points**:
- `MatchErrorAsync` discriminates between error types
- Each error type can have its own handler
- `onError` provides a fallback for unhandled error types
- Automatically maps to appropriate HTTP status codes

### Error Side Effects with TapError

Execute side effects when errors occur without changing the result:

```csharp
var result = await ProcessPaymentAsync(order, cancellationToken)
    .TapAsync(payment => 
        _logger.LogInformation("Payment succeeded: {PaymentId}", payment.Id))
    .TapOnFailureAsync(async (error, ct) => 
        await _logger.LogErrorAsync("Payment failed: {Error}", error.Detail, ct),
        cancellationToken)
    .TapOnFailureAsync(async (error, ct) => 
        await _notificationService.NotifyAdminAsync(error, ct),
        cancellationToken);
```

**Key Points**:
- `TapAsync` executes only on **success**
- `TapErrorAsync` executes only on **failure**
- Side effects don't change the `Result` value
- Perfect for logging, metrics, and notifications

### Error Recovery with RecoverOnFailure

Provide fallback behavior when specific errors occur:

```csharp
var result = await GetUserFromCacheAsync(userId, cancellationToken)
    .RecoverOnFailureAsync(
        predicate: error => error is NotFoundError,
        func: async ct => await GetUserFromDatabaseAsync(userId, ct),
        cancellationToken: cancellationToken
    )
    .TapAsync(user => 
        _logger.LogInformation("User retrieved from {Source}", 
            user.Source == "cache" ? "cache" : "database"));
```

**Key Points**:
- `RecoverOnFailureAsync` provides fallback on specific error types
- Predicate determines which errors trigger recovery
- Useful for retry logic, fallback services, default values

### Retry Transient Failures

Automatically retry operations that may fail temporarily:

```csharp
var result = await RetryExtensions.RetryAsync(
    operation: async ct => await CallExternalServiceAsync(ct),
    maxRetries: 3,
    initialDelay: TimeSpan.FromMilliseconds(100),
    shouldRetry: error => error is ServiceUnavailableError,
    cancellationToken: cancellationToken
);
```

**Key Points**:
- Retries up to `maxRetries` times (3 in this example = 4 total attempts)
- Exponential backoff with `initialDelay` (100ms, 200ms, 400ms)
- `shouldRetry` predicate controls which errors to retry
- Supports `CancellationToken` for graceful cancellation

### Read HTTP Response as Result

Convert HTTP responses to `Result` with proper error handling:

#### Option 1: Handle NotFound Specifically

```csharp
var result = await _httpClient.GetAsync($"api/person/{id}", cancellationToken)
    .HandleNotFoundAsync(Error.NotFound($"Person {id} not found"))
    .BindAsync(response => 
        response.ReadResultMaybeFromJsonAsync<Person>(
            PersonContext.Default.Person, 
            cancellationToken))
    .BindAsync(maybePerson => 
        maybePerson.ToResult(Error.NotFound($"Person {id} returned null")));
```

#### Option 2: Custom Error Handling

```csharp
async Task<Error> HandleFailure(
    HttpResponseMessage response, 
    string personId, 
    CancellationToken ct)
{
    var content = await response.Content.ReadAsStringAsync(ct);
    _logger.LogError(
        "Person API failed: {StatusCode}, {Content}, PersonId: {PersonId}", 
        response.StatusCode, content, personId);
    
    return response.StatusCode switch
    {
        HttpStatusCode.NotFound => Error.NotFound($"Person {personId} not found"),
        HttpStatusCode.BadRequest => Error.BadRequest("Invalid person ID format"),
        HttpStatusCode.Unauthorized => Error.Unauthorized("Authentication required"),
        _ => Error.Unexpected($"Unexpected error: {response.StatusCode}")
    };
}

var result = await _httpClient.GetAsync($"api/person/{id}", cancellationToken)
    .HandleFailureAsync(HandleFailure, id, cancellationToken)
    .ReadResultFromJsonAsync<Person>(
        PersonContext.Default.Person, 
        cancellationToken);
```

**Key Points**:
- `HandleNotFoundAsync` specifically handles 404 responses
- `HandleFailureAsync` provides custom error handling for all failure status codes
- `ReadResultMaybeFromJsonAsync` returns `Result<Maybe<Person>>` (handles null JSON)
- `ReadResultFromJsonAsync` returns `Result<Person>` (fails if JSON is null)

### Converting Nullable to Result

Convert nullable values to `Result` for consistent error handling:

```csharp
// Convert nullable reference type
User? user = await _repository.GetByIdAsync(userId);
var userResult = user.ToResult(Error.NotFound($"User {userId} not found"));

// Convert nullable value type
int? age = GetAge();
var ageResult = age.ToResult(Error.Validation("Age is required"));

// Async variant
var result = await _repository.GetByIdAsync(userId)
    .ToResultAsync(Error.NotFound($"User {userId} not found"));
```

### Exception Handling with Try/TryAsync

Safely wrap exception-throwing code:

```csharp
// Synchronous
Result<string> LoadFile(string path)
{
    return Result.Try(() => File.ReadAllText(path));
    // Or with custom error mapping:
    // return Result.Try(
    //     () => File.ReadAllText(path),
    //     ex => ex switch
    //     {
    //         FileNotFoundException => Error.NotFound($"File not found: {path}"),
    //         UnauthorizedAccessException => Error.Forbidden("Access denied"),
    //         _ => Error.Unexpected(ex.Message)
    //     }
    // );
}

// Asynchronous
async Task<Result<User>> FetchUserAsync(string url, CancellationToken ct)
{
    return await Result.TryAsync(
        async ct => await _httpClient.GetFromJsonAsync<User>(url, ct),
        cancellationToken: ct
    );
}
```

### LINQ Query Syntax

Use C# query syntax for multi-step operations:

```csharp
var result = 
    from user in GetUser(userId)
    from order in GetLastOrder(user)
    from payment in ProcessPayment(order)
    select new OrderConfirmation(user, order, payment);

// Async variant
var asyncResult = await (
    from userId in UserId.TryCreate(userIdInput)
    from user in GetUserAsync(userId)
    from permissions in GetPermissionsAsync(user.Id)
    select new UserWithPermissions(user, permissions)
).ConfigureAwait(false);
```

**Note**: `where` clauses use a generic "filtered out" error. For domain-specific errors, use `Ensure` instead.

## Common Patterns

### 1. Validation Pipeline

```csharp
public Result<Order> ProcessOrder(OrderRequest request)
{
    return ValidateRequest(request)
        .Bind(req => CheckInventory(req.ProductId, req.Quantity))
        .Bind(product => ValidatePayment(request.PaymentInfo))
        .Bind(payment => CreateOrder(request, payment))
        .Tap(order => SendConfirmationEmail(order))
        .TapOnFailure(error => LogOrderFailure(error));
}
```

### 2. Async Workflow with Cancellation

```csharp
public async Task<Result<string>> PromoteCustomerAsync(
    string customerId, 
    CancellationToken ct)
{
    return await GetCustomerByIdAsync(customerId, ct)
        .ToResultAsync(Error.NotFound($"Customer {customerId} not found"))
        .EnsureAsync(customer => customer.CanBePromoted,
            Error.Validation("Customer has highest status"))
        .TapAsync(async (customer, ct) => await customer.PromoteAsync(ct), ct)
        .BindAsync(
            async (customer, ct) => 
                await SendPromotionEmailAsync(customer.Email, ct), 
            ct);
}
```

### 3. Parallel Fraud Detection

```csharp
public async Task<Result<Transaction>> ValidateTransactionAsync(
    Transaction transaction,
    CancellationToken ct)
{
    return await Result.ParallelAsync(
        () => CheckBlacklistAsync(transaction.AccountId, ct),
        () => CheckVelocityLimitsAsync(transaction, ct),
        () => CheckAmountThresholdAsync(transaction, ct),
        () => CheckGeolocationAsync(transaction, ct)
    )
    .WhenAllAsync()
    .BindAsync(
        (check1, check2, check3, check4, ct) => 
            ApproveTransactionAsync(transaction, ct),
        ct
    );
}
