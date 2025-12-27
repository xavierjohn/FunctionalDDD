# Integration

Integrate FunctionalDDD with ASP.NET Core, FluentValidation, OpenTelemetry, and Entity Framework Core.

## Table of Contents

- [ASP.NET Core Integration](#aspnet-core-integration)
- [FluentValidation Integration](#fluentvalidation-integration)
- [OpenTelemetry Tracing](#opentelemetry-tracing)
- [Entity Framework Core](#entity-framework-core)
- [Dependency Injection](#dependency-injection)

## ASP.NET Core Integration

The `FunctionalDDD.Asp` package provides seamless conversion from `Result<T>` to HTTP responses.

### Installation

```bash
dotnet add package FunctionalDDD.Asp
```

### MVC Controllers

Use `ToActionResult` to convert `Result<T>` to `ActionResult<T>`:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;

    [HttpPost]
    public ActionResult<User> Register([FromBody] RegisterRequest request) =>
        FirstName.TryCreate(request.FirstName)
            .Combine(LastName.TryCreate(request.LastName))
            .Combine(EmailAddress.TryCreate(request.Email))
            .Bind((firstName, lastName, email) => 
                User.TryCreate(firstName, lastName, email))
            .ToActionResult(this);

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserAsync(
        string id,
        CancellationToken ct) =>
        await _repository.GetByIdAsync(id, ct)
            .ToResultAsync(Error.NotFound($"User {id} not found"))
            .ToActionResultAsync(this);

    [HttpPut("{id}")]
    public async Task<ActionResult<User>> UpdateUserAsync(
        string id,
        [FromBody] UpdateUserRequest request,
        CancellationToken ct) =>
        await _repository.GetByIdAsync(id, ct)
            .ToResultAsync(Error.NotFound($"User {id} not found"))
            .Bind(user => user.Update(request))
            .TapAsync((user, cancellationToken) => 
                _repository.SaveAsync(user, cancellationToken), ct)
            .ToActionResultAsync(this);
}
```

### Minimal API

Use `ToHttpResult` for Minimal API endpoints:

```csharp
var app = WebApplication.Create(args);

var userApi = app.MapGroup("/api/users");

userApi.MapPost("/", (RegisterUserRequest request) =>
    FirstName.TryCreate(request.FirstName)
        .Combine(LastName.TryCreate(request.LastName))
        .Combine(EmailAddress.TryCreate(request.Email))
        .Bind((firstName, lastName, email) => 
            User.TryCreate(firstName, lastName, email))
        .ToHttpResult());

userApi.MapGet("/{id}", async (
    string id,
    IUserRepository repository,
    CancellationToken ct) =>
    await repository.GetByIdAsync(id, ct)
        .ToResultAsync(Error.NotFound($"User {id} not found"))
        .ToHttpResultAsync());

userApi.MapPut("/{id}", async (
    string id,
    UpdateUserRequest request,
    IUserRepository repository,
    CancellationToken ct) =>
    await repository.GetByIdAsync(id, ct)
        .ToResultAsync(Error.NotFound($"User {id} not found"))
        .Bind(user => user.Update(request))
        .TapAsync((u, cancellationToken) => 
            repository.SaveAsync(u, cancellationToken), ct)
        .ToHttpResultAsync());

app.Run();
```

### Automatic Error Mapping

Results are automatically mapped to appropriate HTTP status codes:

| Error Type | HTTP Status | Example |
|------------|-------------|---------|
| `ValidationError` | 400 Bad Request | Invalid email format |
| `NotFoundError` | 404 Not Found | User not found |
| `UnauthorizedError` | 401 Unauthorized | Missing token |
| `ForbiddenError` | 403 Forbidden | Insufficient permissions |
| `ConflictError` | 409 Conflict | Duplicate email |
| `UnexpectedError` | 500 Internal Server Error | Database error |

### Custom Error Responses

Use `MatchError` for custom error handling:

```csharp
app.MapPost("/orders", async (CreateOrderRequest request, CancellationToken ct) =>
{
    return await ProcessOrderAsync(request, ct)
        .MatchError(
            onValidation: err => 
                Results.BadRequest(new 
                { 
                    message = "Validation failed",
                    errors = err.FieldErrors.ToDictionary(f => f.FieldName, f => f.Details.ToArray())
                }),
            onNotFound: err => 
                Results.NotFound(new { message = err.Detail }),
            onConflict: err => 
                Results.Conflict(new { message = err.Detail }),
            onSuccess: order => 
                Results.Created($"/orders/{order.Id}", order)
        );
});
```

### Pagination Support

The library automatically handles pagination with proper HTTP headers:

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<User>>> GetUsersAsync(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken ct)
{
    var result = await _repository.GetPagedAsync(page, pageSize, ct);
    
    // Automatically sets Content-Range header and returns 200 or 206
    return result.ToActionResult(this);
}
```

Returns:
- **200 OK** if all items fit in one page
- **206 Partial Content** with `Content-Range` header for paginated results

## FluentValidation Integration

The `FunctionalDDD.FluentValidation` package integrates FluentValidation with Railway Oriented Programming.

### Installation

```bash
dotnet add package FunctionalDDD.FluentValidation
dotnet add package FluentValidation
```

### Inline Validator

Use `InlineValidator` for simple validation rules:

```csharp
public partial class User : Aggregate<UserId>
{
    public FirstName FirstName { get; }
    public LastName LastName { get; }
    public EmailAddress Email { get; }
    public int Age { get; }

    public static Result<User> TryCreate(
        FirstName firstName, 
        LastName lastName, 
        EmailAddress email,
        int age)
    {
        var user = new User(firstName, lastName, email, age);
        return Validator.ValidateToResult(user);
    }

    private User(FirstName firstName, LastName lastName, EmailAddress email, int age)
        : base(UserId.NewUnique())
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Age = age;
    }

    private static readonly InlineValidator<User> Validator = new()
    {
        v => v.RuleFor(x => x.FirstName).NotNull(),
        v => v.RuleFor(x => x.LastName).NotNull(),
        v => v.RuleFor(x => x.Email).NotNull(),
        v => v.RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18)
            .WithMessage("Must be 18 or older")
    };
}
```

### Separate Validator Class

For complex validation, create dedicated validator classes:

```csharp
public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must contain at least one item");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemValidator());

        RuleFor(x => x.ShippingAddress)
            .NotNull()
            .SetValidator(new AddressValidator());
    }
}

// Usage
public Result<Order> CreateOrder(CreateOrderCommand command)
{
    return _validator.ValidateToResult(command)
        .Bind(validCommand => Order.Create(validCommand));
}
```

### Async Validation

Support for async validation rules:

```csharp
public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    private readonly IUserRepository _repository;

    public RegisterUserValidator(IUserRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(BeUniqueEmailAsync)
            .WithMessage("Email is already registered");
    }

    private async Task<bool> BeUniqueEmailAsync(
        string email,
        CancellationToken ct)
    {
        var exists = await _repository.ExistsByEmailAsync(email, ct);
        return !exists;
    }
}

// Usage
public async Task<Result<User>> RegisterUserAsync(
    RegisterUserCommand command,
    CancellationToken ct)
{
    return await _validator.ValidateToResultAsync(command, ct)
        .BindAsync((validCommand, cancellationToken) => 
            User.CreateAsync(validCommand, cancellationToken), ct);
}
```

## OpenTelemetry Tracing

Enable distributed tracing for Railway Oriented Programming and Value Objects.

### Installation

```bash
dotnet add package OpenTelemetry
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
```

### Configuration

```csharp
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using FunctionalDdd.CommonValueObjects;
using FunctionalDdd.RailwayOrientedProgramming;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder
                .CreateDefault()
                .AddService("MyApplication"))
            .AddFunctionalDddRopInstrumentation()      // ROP tracing
            .AddFunctionalDddCvoInstrumentation()      // Common Value Objects tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    });
```

### What Gets Traced

**Railway Oriented Programming:**
- `Result<T>` creation (success/failure)
- Operation chains (Bind, Map, Tap, etc.)
- Error propagation
- Compensate operations

**Common Value Objects:**
- Value object creation attempts
- Validation success/failure
- Error details

### Trace Attributes

Traces include detailed attributes:

```csharp
// Example trace for EmailAddress.TryCreate("invalid")
Span: EmailAddress.TryCreate
├─ Attributes:
│  ├─ result.is_success: false
│  ├─ result.error.type: ValidationError
│  ├─ result.error.code: email.invalid
│  ├─ result.error.detail: Invalid email format
│  └─ input.value: invalid
```

### Viewing Traces

Use tools like:
- **Jaeger**: Open-source distributed tracing
- **Zipkin**: Distributed tracing system
- **Azure Application Insights**: Cloud-based APM
- **Grafana Tempo**: Open-source tracing backend

## Entity Framework Core

Integrate Result types with Entity Framework Core:

### ToResultAsync Helper

```csharp
public static class RepositoryExtensions
{
    public static async Task<Result<T>> ToResultAsync<T>(
        this Task<T?> task,
        Error notFoundError) where T : class
    {
        var entity = await task;
        return entity != null
            ? Result.Success(entity)
            : Result.Failure<T>(notFoundError);
    }
}

// Usage
public async Task<Result<User>> GetByIdAsync(UserId id, CancellationToken ct)
{
    return await _context.Users
        .FirstOrDefaultAsync(u => u.Id == id, ct)
        .ToResultAsync(Error.NotFound($"User {id} not found"));
}
```

### Save Changes with Result

```csharp
public async Task<Result<User>> SaveUserAsync(User user, CancellationToken ct)
{
    try
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        return Result.Success(user);
    }
    catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
    {
        return Error.Conflict("User with this email already exists");
    }
    catch (DbUpdateConcurrencyException)
    {
        return Error.Conflict("User was modified by another process");
    }
}
```

## Dependency Injection

Register validators and repositories with DI:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();
```

### Using in Services

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IValidator<CreateUserCommand> _validator;

    public UserService(
        IUserRepository repository,
        IValidator<CreateUserCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<User>> CreateUserAsync(
        CreateUserCommand command,
        CancellationToken ct)
    {
        return await _validator.ValidateToResultAsync(command, ct)
            .BindAsync((cmd, cancellationToken) => 
                User.CreateAsync(cmd, cancellationToken), ct)
            .TapAsync((user, cancellationToken) => 
                _repository.SaveAsync(user, cancellationToken), ct);
    }
}
```

## Best Practices

1. **Use ToActionResult/ToHttpResult at API Boundaries**: Keep Result types internal to your application
2. **Validate Early**: Use FluentValidation at the edge of your system
3. **Enable Tracing in Production**: Monitor error rates and performance
4. **Consistent Error Messages**: Use error codes and structured error details
5. **Repository Pattern**: Return Result from repositories, not throw exceptions
6. **CancellationToken**: Always pass through to async database operations

## Next Steps

- See [Error Handling](error-handling.md) for custom error types
- Learn about [Async & Cancellation](async-cancellation.md) for proper CT usage
- Review [Examples](examples.md) for complete working applications
