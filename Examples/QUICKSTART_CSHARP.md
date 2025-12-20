# Quick Start for C# Developers

**New to functional programming?** This guide uses familiar C# patterns to get you started quickly.

We'll use the **alias methods** (`Then`, `Peek`, `OrElse`, `Require`) which feel more natural to C# developers. Once comfortable, you can transition to the standard functional programming names (`Bind`, `Tap`, `Compensate`, `Ensure`) used in the main documentation.

## Installation

```bash
dotnet add package FunctionalDdd.RailwayOrientedProgramming
```

## The Problem: Nested Error Handling Hell

You've probably written code like this:

```csharp
public async Task<User> RegisterUserAsync(string firstName, string lastName, string email, string password)
{
    // Validate each field
    if (string.IsNullOrWhiteSpace(firstName))
        throw new ValidationException("First name is required");
    
    if (firstName.Length > 50)
        throw new ValidationException("First name too long");
    
    if (string.IsNullOrWhiteSpace(lastName))
        throw new ValidationException("Last name is required");
    
    if (!IsValidEmail(email))
        throw new ValidationException("Invalid email format");
    
    // Check if user exists
    var existing = await _db.FindUserByEmailAsync(email);
    if (existing != null)
        throw new ConflictException("User already exists");
    
    // Create user
    try
    {
        var user = new User(firstName, lastName, email);
        await user.SetPasswordAsync(password);
        await _db.SaveAsync(user);
        await _emailService.SendWelcomeEmailAsync(user);
        return user;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to create user");
        throw;
    }
}
```

**Problems:**
- ❌ Exceptions for control flow (expensive)
- ❌ Lost validation context when first error is thrown
- ❌ Hard to return multiple validation errors at once
- ❌ Difficult to handle different error types gracefully
- ❌ Testing requires try-catch blocks everywhere

## The Solution: Railway Oriented Programming

Think of your code as a railway track with two paths:
- **Success track** 🟢 - Everything works, keep going
- **Failure track** 🔴 - Something failed, skip remaining operations

```csharp
using FunctionalDdd;
using FunctionalDdd.Aliases;  // 👈 Import alias methods

public async Task<Result<User>> RegisterUserAsync(
    string firstName, 
    string lastName, 
    string email, 
    string password)
{
    return FirstName.TryCreate(firstName)
        .Combine(LastName.TryCreate(lastName))
        .Combine(EmailAddress.TryCreate(email))
        .ThenAsync(async (fn, ln, em) =>             // All validations passed
            await CheckUserNotExistsAsync(em, (fn, ln, em)))
        .ThenAsync(async tuple =>                     // Create user
            await User.CreateAsync(tuple.fn, tuple.ln, tuple.em, password))
        .Peek(user => _logger.LogInformation("User created: {Email}", user.Email))
        .PeekAsync(async user =>                      // Send welcome email
            await _emailService.SendWelcomeEmailAsync(user));
}
```

**Benefits:**
- ✅ First validation failure stops the chain (fails fast)
- ✅ No exceptions thrown (return values)
- ✅ Can collect multiple errors with `Combine`
- ✅ Easy to test (just check `.IsSuccess`)
- ✅ Clear separation of concerns

## Core Concept: Result<T>

Instead of throwing exceptions, methods return `Result<T>`:

```csharp
// Old way: throws exception
public string ValidateEmail(string email)
{
    if (!IsValid(email))
        throw new ValidationException("Invalid email");
    return email;
}

// New way: returns Result
public Result<Email> ValidateEmail(string email)
{
    if (!IsValid(email))
        return Error.Validation("Invalid email format", "email");
    return new Email(email);
}
```

**Using the result:**
```csharp
var result = ValidateEmail("john@example.com");

if (result.IsSuccess)
    Console.WriteLine($"Valid: {result.Value}");
else
    Console.WriteLine($"Error: {result.Error.Detail}");
```

## Essential Alias Methods

Import the aliases at the top of your file:
```csharp
using FunctionalDdd.Aliases;
```

💡 **Want to see these in action?** Check out the [complete working examples](../Xunit/AliasExamples.cs) with 16 passing tests covering all scenarios!

### 1. `Then` - Chain operations (like LINQ's `SelectMany`)

Use `Then` when the next operation might fail:

```csharp
var result = ValidateEmail(email)
    .Then(validEmail => CreateUser(validEmail))
    .Then(user => AssignRole(user, "Customer"));
// Stops at first failure, returns final Result
```

**Think of it as:** "If this succeeds, **then** do that"

**Note:** Use `Map` instead of `Then` when your transformation doesn't return a `Result`:
```csharp
.Map(user => user.ToDto())  // Simple transformation
.Then(dto => SaveAsync(dto)) // Operation that can fail
```

### 2. `Peek` - Do something without changing the value (like LINQ's `Do`)

Use `Peek` for logging, metrics, or side effects:

```csharp
var result = CreateOrder(customerId)
    .Peek(order => _logger.LogInformation("Order created: {OrderId}", order.Id))
    .Peek(order => _metrics.Increment("orders.created"))
    .Then(order => ProcessPayment(order));
// Peek doesn't change the order, just observes it
```

**Think of it as:** "Peek at the value and do something, but don't change it"

### 3. `OrElse` - Provide fallback when it fails (like `??`)

Use `OrElse` for retry logic or default values:

```csharp
var result = GetUserFromCache(id)
    .OrElse(() => GetUserFromDatabase(id))
    .OrElse(() => GetDefaultGuestUser());
// First success wins
```

**Think of it as:** "Try this, **or else** try that"

### 4. `Require` - Validate a condition (like guard clauses)

Use `Require` to ensure business rules:

```csharp
var result = GetUser(id)
    .Require(user => user.IsActive, Error.Validation("User is inactive"))
    .Require(user => user.EmailVerified, Error.Validation("Email not verified"))
    .Then(user => GrantAccess(user));
// Each Require must pass
```

**Think of it as:** Guard clauses that return errors instead of throwing

## Real-World Example: User Registration

Let's build a complete registration flow:

```csharp
using FunctionalDdd;
using FunctionalDdd.Aliases;

public class UserRegistrationService
{
    private readonly IUserRepository _userRepo;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserRegistrationService> _logger;

    public async Task<Result<User>> RegisterAsync(RegisterRequest request)
    {
        return await ValidateInputs(request)
            .ThenAsync(async data => await EnsureEmailNotTakenAsync(data.email, data))
            .ThenAsync(async data => await CreateUserAsync(data))
            .Peek(user => _logger.LogInformation("User registered: {Email}", user.Email))
            .PeekAsync(async user => await SendWelcomeEmailAsync(user))
            .OrElseAsync(async error => await HandleRegistrationErrorAsync(error));
    }

    private Result<(FirstName first, LastName last, Email email, Password pass)> ValidateInputs(
        RegisterRequest request)
    {
        // Validate all fields and combine results
        return FirstName.TryCreate(request.FirstName)
            .Combine(LastName.TryCreate(request.LastName))
            .Combine(Email.TryCreate(request.Email))
            .Combine(Password.TryCreate(request.Password));
        
        // If ANY validation fails, returns ALL errors
        // If ALL succeed, returns tuple with all values
    }

    private async Task<Result<(FirstName, LastName, Email, Password)>> EnsureEmailNotTakenAsync(
        Email email,
        (FirstName, LastName, Email, Password) data)
    {
        var exists = await _userRepo.ExistsByEmailAsync(email);
        
        return exists
            ? Error.Conflict($"User with email {email} already exists")
            : data.ToResult();
    }

    private async Task<Result<User>> CreateUserAsync(
        (FirstName first, LastName last, Email email, Password password) data)
    {
        var user = new User(data.first, data.last, data.email);
        await user.SetPasswordAsync(data.password);
        await _userRepo.SaveAsync(user);
        return user.ToResult();
    }

    private async Task SendWelcomeEmailAsync(User user)
    {
        await _emailService.SendAsync(new WelcomeEmail
        {
            To = user.Email,
            UserName = user.FirstName
        });
    }

    private async Task<Result<User>> HandleRegistrationErrorAsync(Error error)
    {
        // Log the error
        _logger.LogWarning("Registration failed: {Error}", error.Detail);
        
        // For demo: Try to suggest similar existing users
        if (error is ConflictError)
        {
            // Could suggest "Did you mean to login?"
            // For now, just return the original error
        }
        
        return error;
    }
}
```

**Using it in a controller:**
```csharp
[HttpPost("register")]
public async Task<ActionResult<User>> Register([FromBody] RegisterRequest request)
{
    var result = await _registrationService.RegisterAsync(request);
    
    return result.Match(
        onSuccess: user => CreatedAtAction(nameof(GetUser), new { id = user.Id }, user),
        onFailure: error => error.ToActionResult<User>(this));
}
```

**What happens:**
1. ✅ All fields validated simultaneously → Returns all errors if any fail
2. ✅ Email uniqueness checked → Returns conflict error if exists
3. ✅ User created and saved → Returns unexpected error if database fails
4. ✅ Welcome email sent (doesn't fail the operation if it fails)
5. ✅ Appropriate HTTP status returned based on error type

## Validating Multiple Fields

Use `Combine` to validate multiple fields and collect **all** errors:

```csharp
// Old way: Returns first error only
public User CreateUser(string first, string last, string email)
{
    if (string.IsNullOrWhiteSpace(first))
        throw new ValidationException("First name required");
    // Never reaches here if first fails
    if (string.IsNullOrWhiteSpace(last))
        throw new ValidationException("Last name required");
    // ...
}

// New way: Returns ALL errors
public Result<User> CreateUser(string first, string last, string email)
{
    return FirstName.TryCreate(first)
        .Combine(LastName.TryCreate(last))
        .Combine(Email.TryCreate(email))
        .Map((firstName, lastName, emailAddr) => 
            new User(firstName, lastName, emailAddr));
}
```

**HTTP Response when multiple validations fail:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "firstName": ["First name cannot be empty"],
    "email": ["Invalid email format"]
  }
}
```

Perfect for form validation in web APIs! The response follows the standard `ProblemDetails` format (RFC 9110).

## Async Operations

All methods have async variants with `Async` suffix:

```csharp
public async Task<Result<Order>> PlaceOrderAsync(PlaceOrderRequest request)
{
    return await CustomerId.TryCreate(request.CustomerId)
        .ThenAsync(async customerId => await GetCustomerAsync(customerId))
        .Require(customer => customer.IsActive, Error.Validation("Inactive customer"))
        .ThenAsync(async customer => await CreateOrderAsync(customer, request.Items))
        .Peek(order => _logger.LogInformation("Order placed: {OrderId}", order.Id))
        .PeekAsync(async order => await ReserveInventoryAsync(order))
        .ThenAsync(async order => await ProcessPaymentAsync(order, request.Payment))
        .PeekAsync(async order => await SendConfirmationEmailAsync(order))
        .OrElseAsync(async error => await HandleOrderErrorAsync(error));
}
```

**Async methods:**
- `ThenAsync` - Chain async operations
- `PeekAsync` - Async side effects  
- `RequireAsync` - Async validation
- `OrElseAsync` - Async fallback

## Error Handling Patterns

### Pattern 1: Specific Error Recovery

```csharp
var result = await ProcessPaymentAsync(order, paymentInfo)
    .OrElse(
        predicate: error => error.Code == "insufficient_funds",
        fallback: () => OfferPaymentPlanAsync(order));
// Only handle insufficient funds, let other errors through
```

### Pattern 2: Retry on Transient Errors

```csharp
var result = await SaveToExternalApiAsync(data)
    .OrElseAsync(
        predicate: error => error is ServiceUnavailableError,
        fallbackAsync: async () => await RetryWithBackoffAsync(data));
```

### Pattern 3: Default Values

```csharp
var result = GetUserPreferences(userId)
    .OrElse(() => GetDefaultPreferences());
```

### Pattern 4: Multiple Validation Rules

```csharp
var result = CreateAccount(accountData)
    .Require(acc => acc.Balance >= 0, Error.Validation("Negative balance"))
    .Require(acc => acc.Owner.Age >= 18, Error.Validation("Must be 18+"))
    .Require(acc => acc.Currency == "USD", Error.Validation("USD only"));
```

## Web API Integration

Automatic HTTP status code mapping:

```csharp
using FunctionalDdd.Asp;

[HttpPost]
public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
{
    var result = await _orderService.CreateOrderAsync(request);
    
    return result.ToActionResult(this);
    // ValidationError → 400 Bad Request
    // NotFoundError → 404 Not Found
    // ConflictError → 409 Conflict
    // UnauthorizedError → 401 Unauthorized
    // ForbiddenError → 403 Forbidden
    // UnexpectedError → 500 Internal Server Error
}
```

Or use `Match` for custom responses:

```csharp
[HttpPost]
public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
{
    var result = await _orderService.CreateOrderAsync(request);
    
    return result.Match(
        onSuccess: order => CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order),
        onFailure: error => error switch
        {
            ValidationError => BadRequest(error),
            ConflictError => Conflict(error),
            _ => StatusCode(500, error)
        });
}
```

## Testing

Testing becomes simple - just check `IsSuccess`:

```csharp
[Fact]
public void Valid_Email_Creates_Successfully()
{
    // Arrange
    var email = "john@example.com";
    
    // Act
    var result = Email.TryCreate(email);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.ToString().Should().Be(email);
}

[Fact]
public void Invalid_Email_Returns_Validation_Error()
{
    // Arrange
    var email = "not-an-email";
    
    // Act
    var result = Email.TryCreate(email);
    
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().BeOfType<ValidationError>();
    result.Error.Detail.Should().Contain("Invalid email");
}

[Fact]
public async Task Registration_With_Existing_Email_Returns_Conflict()
{
    // Arrange
    await _userRepo.SaveAsync(new User("John", "Doe", "john@example.com"));
    
    // Act
    var result = await _service.RegisterAsync(new RegisterRequest
    {
        FirstName = "Jane",
        LastName = "Smith",
        Email = "john@example.com"  // Already exists
    });
    
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().BeOfType<ConflictError>();
}
```

## Common Patterns Cheat Sheet

| Pattern | Code | Use When |
|---------|------|----------|
| **Chain operations** | `.Then(x => DoSomething(x))` | Next step might fail |
| **Transform value** | `.Map(x => x.ToDto())` | Simple transformation |
| **Side effect** | `.Peek(x => Log(x))` | Logging, metrics |
| **Validate** | `.Require(x => x.IsValid, error)` | Business rules |
| **Fallback** | `.OrElse(() => GetDefault())` | Retry, defaults |
| **Combine validations** | `.Combine(other)` | Multiple independent checks |
| **Unwrap result** | `.Match(ok => ..., err => ...)` | End of chain |

## Error Types Reference

Choose the right error type for clear HTTP status mapping:

```csharp
// 400 Bad Request - Client sent invalid data
Error.Validation("Email format is invalid", "email")
Error.BadRequest("Missing required header")

// 401 Unauthorized - Not authenticated
Error.Unauthorized("Please log in")

// 403 Forbidden - Authenticated but not allowed
Error.Forbidden("Admin access required")

// 404 Not Found - Resource doesn't exist
Error.NotFound($"User {id} not found")

// 409 Conflict - Resource already exists or state conflict
Error.Conflict("Email already registered")

// 422 Unprocessable Entity - Business rule violation
Error.Domain("Cannot withdraw more than balance")

// 429 Too Many Requests - Rate limit exceeded
Error.RateLimit("Try again in 60 seconds")

// 500 Internal Server Error - Something unexpected
Error.Unexpected("Database connection failed")

// 503 Service Unavailable - Temporary problem
Error.ServiceUnavailable("Payment gateway is down")
```

## Transitioning to Standard FP Names

Once comfortable, you can switch to standard functional programming terminology:

| Alias (C# friendly) | Standard (FP) | Meaning |
|---------------------|---------------|---------|
| `Then` | `Bind` | Chain operations |
| `Peek` | `Tap` | Side effect |
| `OrElse` | `Compensate` | Fallback |
| `Require` | `Ensure` | Validation |

**Why learn standard names?**
- Used in F#, Haskell, Scala, Rust, Kotlin
- Better alignment with FP literature and tutorials
- More expressive of intent to FP developers

You can use both! Import both namespaces:
```csharp
using FunctionalDdd;        // Standard names
using FunctionalDdd.Aliases; // Alias names
```

## Next Steps

1. **Try it**: Install the package and convert one method
2. **Read examples**: Check [Quick Start](./QUICKSTART.md) for more complex scenarios
3. **Explore the library**: See [Railway Oriented Programming README](../RailwayOrientedProgramming/README.md)
4. **Learn more FP**: Gradually transition to standard terminology

## Philosophy: Why This Approach?

**Traditional approach:**
```csharp
try {
    var user = GetUser();
    var order = CreateOrder(user);
    var payment = ProcessPayment(order);
    return payment;
} catch (ValidationException ex) {
    return BadRequest(ex.Message);
} catch (NotFoundException ex) {
    return NotFound(ex.Message);
} catch (Exception ex) {
    _logger.LogError(ex, "Unexpected error");
    return StatusCode(500);
}
```

**Problems:**
- Exceptions are expensive (stack unwinding)
- Hard to see what errors each method can produce
- Difficult to collect multiple errors
- Business logic mixed with error handling

**Railway Oriented Programming:**
```csharp
return GetUser(id)
    .Then(user => CreateOrder(user))
    .Then(order => ProcessPayment(order))
    .ToActionResult(this);
```

**Benefits:**
- **Explicit**: Return types show what can fail
- **Fast**: No exception overhead
- **Composable**: Easy to chain operations
- **Testable**: Pure functions, no try-catch
- **Maintainable**: Clear separation of concerns

**Think of it as:**
- Promises/async-await pattern (you're already familiar with this!)
- LINQ (chaining operations)
- Null-conditional operators (`?.`, `??`) but for errors

You're not learning something radically new - you're using patterns you already know in a more powerful way!

## FAQ

**Q: Do I have to use aliases?**  
A: No! They're optional. Use what feels natural to you.

**Q: Can I mix aliases and standard names?**  
A: Yes, but stick to one style per file for consistency.

**Q: What about performance?**  
A: Faster than exceptions! See [benchmarks](../Benchmark/README.md).

**Q: Does this work with Entity Framework?**  
A: Yes! See [Entity Framework Core configuration examples](../CommonValueObjects/SAMPLES.md#entity-framework-core) for value object mappings.

**Q: How do I handle validation in Minimal APIs?**  
A: See [Minimal API example](./SampleMinimalApi/API/UserRoutes.cs).

**Q: Can I use this with FluentValidation?**  
A: Yes! There's a dedicated integration package. See [FluentValidation integration](../FluentValidation/README.md).

---

**Ready to eliminate exception handling hell?** Start with one method today! 🚀
