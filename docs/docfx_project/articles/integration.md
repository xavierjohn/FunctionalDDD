# Integration

Integrate FunctionalDDD with ASP.NET Core, FluentValidation, OpenTelemetry, and Entity Framework Core.

## Table of Contents

- [ASP.NET Core Integration](#aspnet-core-integration)
- [FluentValidation Integration](#fluentvalidation-integration)
- [OpenTelemetry Tracing](#opentelemetry-tracing)
- [Entity Framework Core](#entity-framework-core)
- [Problem Details (RFC 7807)](#problem-details-rfc-7807)
- [Best Practices](#best-practices)

## ASP.NET Core Integration

The `FunctionalDDD.Asp` package provides seamless conversion from `Result<T>` to HTTP responses with automatic error-to-status-code mapping and Problem Details support.

### Installation

```bash
dotnet add package FunctionalDDD.Asp
```

### MVC Controllers

Use `ToActionResult` to convert `Result<T>` to `ActionResult<T>`:

```csharp
using FunctionalDdd;
using Microsoft.AspNetCore.Mvc;
using Mediator;  // OSS alternative to MediatR

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken ct)
        => await FirstName.TryCreate(request.FirstName)
            .Combine(LastName.TryCreate(request.LastName))
            .Combine(EmailAddress.TryCreate(request.Email))
            .Bind((firstName, lastName, email) => 
                RegisterUserCommand.TryCreate(firstName, lastName, email))
            .BindAsync(command => _mediator.Send(command, ct), ct)
            .MapAsync(user => user.Adapt<UserDto>())
            .ToActionResultAsync(this);

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(
        string id,
        CancellationToken ct)
        => await UserId.TryCreate(id)
            .Bind(userId => GetUserQuery.TryCreate(userId))
            .BindAsync(query => _mediator.Send(query, ct), ct)
            .MapAsync(user => user.Adapt<UserDto>())
            .ToActionResultAsync(this);

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(
        string id,
        [FromBody] UpdateUserRequest request,
        CancellationToken ct)
        => await UserId.TryCreate(id)
            .Bind(userId => UpdateUserCommand.TryCreate(userId, request))
            .BindAsync(command => _mediator.Send(command, ct), ct)
            .MapAsync(user => user.Adapt<UserDto>())
            .ToActionResultAsync(this);

    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteUser(
        string id,
        CancellationToken ct)
        => await UserId.TryCreate(id)
            .Bind(userId => DeleteUserCommand.TryCreate(userId))
            .BindAsync(command => _mediator.Send(command, ct), ct)
            .ToActionResultAsync(this);
}
```

**Application Layer** (Command Handler):

```csharp
using Mediator;

public record RegisterUserCommand(
    FirstName FirstName,
    LastName LastName,
    EmailAddress Email
) : IRequest<Result<User>>
{
    public static Result<RegisterUserCommand> TryCreate(
        FirstName firstName,
        LastName lastName,
        EmailAddress email)
        => Result.Success(new RegisterUserCommand(firstName, lastName, email));
}

public class RegisterUserCommandHandler 
    : IRequestHandler<RegisterUserCommand, Result<User>>
{
    private readonly IUserRepository _repository;
    private readonly IValidator<RegisterUserCommand> _validator;

    public RegisterUserCommandHandler(
        IUserRepository repository,
        IValidator<RegisterUserCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async ValueTask<Result<User>> Handle(
        RegisterUserCommand command,
        CancellationToken ct)
        => await _validator.ValidateToResultAsync(command, ct)
            .BindAsync((cmd, cancellationToken) =>
                User.TryCreate(cmd.FirstName, cmd.LastName, cmd.Email), ct)
            .TapAsync(async (user, cancellationToken) =>
                await _repository.AddAsync(user, cancellationToken), ct)
            .TapAsync(async (_, cancellationToken) =>
                await _repository.SaveChangesAsync(cancellationToken), ct);
}
```

**Query Example**:

```csharp
public record GetUserQuery(UserId UserId) : IRequest<Result<User>>
{
    public static Result<GetUserQuery> TryCreate(UserId userId)
        => Result.Success(new GetUserQuery(userId));
}

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<User>>
{
    private readonly IUserRepository _repository;

    public GetUserQueryHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<Result<User>> Handle(
        GetUserQuery query,
        CancellationToken ct)
        => await _repository.GetByIdAsync(query.UserId, ct);
}
```

**Update Command Example**:

```csharp
public record UpdateUserCommand(
    UserId UserId,
    FirstName FirstName,
    LastName LastName
) : IRequest<Result<User>>
{
    public static Result<UpdateUserCommand> TryCreate(
        UserId userId,
        UpdateUserRequest request)
        => FirstName.TryCreate(request.FirstName)
            .Combine(LastName.TryCreate(request.LastName))
            .Bind((firstName, lastName) =>
                Result.Success(new UpdateUserCommand(userId, firstName, lastName)));
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<User>>
{
    private readonly IUserRepository _repository;

    public UpdateUserCommandHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<Result<User>> Handle(
        UpdateUserCommand command,
        CancellationToken ct)
        => await _repository.GetByIdAsync(command.UserId, ct)
            .Bind(user => user.Update(command.FirstName, command.LastName))
            .TapAsync(async (user, cancellationToken) =>
                await _repository.SaveAsync(user, cancellationToken), ct);
}
```

**Command Validator**:

```csharp
public class RegisterUserCommandValidator 
    : AbstractValidator<RegisterUserCommand>
{
    private readonly IUserRepository _repository;

    public RegisterUserCommandValidator(IUserRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(BeUniqueEmailAsync)
            .WithMessage("Email already registered");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50);
    }

    private async Task<bool> BeUniqueEmailAsync(string email, CancellationToken ct)
    {
        var exists = await _repository.ExistsByEmailAsync(email, ct);
        return !exists;
    }
}
```

**Domain Layer** (Aggregate):

```csharp
public class User : Aggregate<UserId>
{
    public FirstName FirstName { get; private set; }
    public LastName LastName { get; private set; }
    public EmailAddress Email { get; private set; }

    public static Result<User> TryCreate(
        FirstName firstName,
        LastName lastName,
        EmailAddress email)
    {
        var user = new User(firstName, lastName, email);
        return Validator.ValidateToResult(user);
    }

    private User(FirstName firstName, LastName lastName, EmailAddress email)
        : base(UserId.NewUnique())
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    private static readonly InlineValidator<User> Validator = new()
    {
        v => v.RuleFor(x => x.FirstName).NotNull(),
        v => v.RuleFor(x => x.LastName).NotNull(),
        v => v.RuleFor(x => x.Email).NotNull()
    };
}
```

**Dependency Injection** (Program.cs):

```csharp
using Mediator;

var builder = WebApplication.CreateBuilder(args);

// Register Mediator (OSS alternative to MediatR)
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
});

// Register validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
```

### Minimal API

Use `ToHttpResult` for Minimal API endpoints:

```csharp
using FunctionalDdd;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

var userApi = app.MapGroup("/api/users")
    .WithTags("Users")
    .WithOpenApi();

userApi.MapPost("/", (
    RegisterUserRequest request,
    IUserService userService) =>
    FirstName.TryCreate(request.FirstName)
        .Combine(LastName.TryCreate(request.LastName))
        .Combine(EmailAddress.TryCreate(request.Email))
        .Bind((firstName, lastName, email) => 
            userService.CreateUser(firstName, lastName, email))
        .Map(user => new UserDto(user))
        .ToHttpResult())
    .WithName("CreateUser")
    .Produces<UserDto>(StatusCodes.Status200OK)
    .ProducesValidationProblem()
    .ProducesProblem(StatusCodes.Status409Conflict);

userApi.MapGet("/{id}", async (
    string id,
    IUserRepository repository,
    CancellationToken ct) =>
    await repository.GetByIdAsync(id, ct)
        .ToResultAsync(Error.NotFound($"User {id} not found"))
        .MapAsync(user => new UserDto(user))
        .ToHttpResultAsync())
    .WithName("GetUser")
    .Produces<UserDto>()
    .ProducesProblem(StatusCodes.Status404NotFound);

userApi.MapPut("/{id}", async (
    string id,
    UpdateUserRequest request,
    IUserRepository repository,
    CancellationToken ct) =>
    await repository.GetByIdAsync(id, ct)
        .ToResultAsync(Error.NotFound($"User {id} not found"))
        .Bind(user => user.Update(request))
        .TapAsync(async (user, cancellationToken) => 
            await repository.SaveAsync(user, cancellationToken), ct)
        .MapAsync(user => new UserDto(user))
        .ToHttpResultAsync())
    .WithName("UpdateUser")
    .Produces<UserDto>()
    .ProducesValidationProblem()
    .ProducesProblem(StatusCodes.Status404NotFound);

userApi.MapDelete("/{id}", async (
    string id,
    IUserRepository repository,
    CancellationToken ct) =>
    await UserId.TryCreate(id)
        .BindAsync(async (userId, cancellationToken) => 
            await repository.DeleteAsync(userId, cancellationToken), ct)
        .ToHttpResultAsync())
    .WithName("DeleteUser")
    .Produces(StatusCodes.Status204NoContent)
    .ProducesProblem(StatusCodes.Status404NotFound);

app.Run();
```

### Automatic Error Mapping

Results are automatically mapped to appropriate HTTP status codes and Problem Details format:

| Error Type | HTTP Status | Example Use Case |
|------------|-------------|------------------|
| `ValidationError` | 400 Bad Request | Invalid email format, required field missing |
| `BadRequestError` | 400 Bad Request | Malformed request, invalid query parameters |
| `UnauthorizedError` | 401 Unauthorized | Missing authentication token |
| `ForbiddenError` | 403 Forbidden | Insufficient permissions for action |
| `NotFoundError` | 404 Not Found | User not found, resource doesn't exist |
| `ConflictError` | 409 Conflict | Duplicate email, concurrent modification |
| `DomainError` | 422 Unprocessable Entity | Business rule violation |
| `RateLimitError` | 429 Too Many Requests | API rate limit exceeded |
| `UnexpectedError` | 500 Internal Server Error | Database connection failed |
| `ServiceUnavailableError` | 503 Service Unavailable | Service under maintenance |
| `AggregateError` | Varies | Multiple errors (uses first error's status) |

**Key Features:**
- ✅ **Automatic Status Codes** - No manual status code mapping required
- ✅ **Problem Details (RFC 7807)** - Standard error response format
- ✅ **Validation Error Formatting** - Field-level errors in ModelState format
- ✅ **Unit Type Support** - Successful `Unit` results return 204 No Content
- ✅ **Async Support** - Full async/await with `CancellationToken`

### Custom Error Responses

Use `MatchError` for custom error handling when you need more control:

```csharp
app.MapPost("/orders", async (
    CreateOrderRequest request,
    IOrderService orderService,
    CancellationToken ct) =>
{
    return await orderService.ProcessOrderAsync(request, ct)
        .MatchErrorAsync(
            onValidation: err => Results.BadRequest(new 
            { 
                message = "Validation failed",
                errors = err.FieldErrors
                    .ToDictionary(
                        f => f.FieldName, 
                        f => f.Details.ToArray())
            }),
            onNotFound: err => 
                Results.NotFound(new { message = err.Detail }),
            onConflict: err => 
                Results.Conflict(new { message = err.Detail }),
            onDomain: err =>
                Results.Problem(
                    detail: err.Detail,
                    statusCode: StatusCodes.Status422UnprocessableEntity),
            onSuccess: order => 
                Results.Created($"/orders/{order.Id}", order),
            cancellationToken: ct
        );
});
```

### Pagination Support

The library provides built-in support for HTTP 206 Partial Content responses with Content-Range headers:

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersAsync(
    [FromQuery] int page = 0,
    [FromQuery] int pageSize = 25,
    CancellationToken ct)
{
    var from = page * pageSize;
    var to = from + pageSize - 1;
    
    var result = await _repository.GetPagedAsync(from, pageSize, ct);
    
    // Returns:
    // - 200 OK if all items fit in one page (to >= totalCount - 1)
    // - 206 Partial Content with Content-Range header if partial results
    return result
        .Map(pagedData => (pagedData.Users, pagedData.TotalCount))
        .Map(x => x.Users.Select(u => new UserDto(u)))
        .ToActionResult(this, from, to, result.Value.TotalCount);
}
```

**Response Headers:**
```http
HTTP/1.1 206 Partial Content
Content-Range: items 0-24/100
Content-Type: application/json
```

**Advanced Pagination with Custom Range Extraction:**

```csharp
public record PagedResult<T>(
    IEnumerable<T> Items, 
    long From, 
    long To, 
    long TotalCount);

[HttpGet]
public ActionResult<IEnumerable<UserDto>> GetUsers(
    [FromQuery] int page = 0,
    [FromQuery] int pageSize = 25)
{
    return _userService
        .GetPagedUsers(page, pageSize)
        .ToActionResult(
            this,
            funcRange: pagedResult => new ContentRangeHeaderValue(
                pagedResult.From,
                pagedResult.To,
                pagedResult.TotalCount)
            {
                Unit = "items"
            },
            funcValue: pagedResult => pagedResult.Items.Select(u => new UserDto(u))
        );
}
```

## FluentValidation Integration

The `FunctionalDDD.FluentValidation` package integrates FluentValidation with Railway Oriented Programming.

### Installation

```bash
dotnet add package FunctionalDDD.FluentValidation
dotnet add package FluentValidation
```

### Inline Validator

Use `InlineValidator` for simple validation rules within aggregates:

```csharp
using FluentValidation;
using FunctionalDdd;

public class User : Aggregate<UserId>
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
        
        // ValidateToResult converts FluentValidation results to Result<User>
        return Validator.ValidateToResult(user);
    }

    private User(
        FirstName firstName, 
        LastName lastName, 
        EmailAddress email, 
        int age)
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
        
        RuleFor(x => x.TotalAmount)
            .GreaterThan(0)
            .WithMessage("Order total must be greater than zero");
    }
}

// Usage in service
public class OrderService : IOrderService
{
    private readonly IValidator<CreateOrderCommand> _validator;
    private readonly IOrderRepository _repository;

    public OrderService(
        IValidator<CreateOrderCommand> validator,
        IOrderRepository repository)
    {
        _validator = validator;
        _repository = repository;
    }

    public Result<Order> CreateOrder(CreateOrderCommand command)
    {
        return _validator.ValidateToResult(command)
            .Bind(validCommand => Order.Create(validCommand))
            .Tap(order => _repository.Add(order));
    }
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
        
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(3, 50)
            .MustAsync(BeUniqueUsernameAsync)
            .WithMessage("Username is already taken");
    }

    private async Task<bool> BeUniqueEmailAsync(
        string email,
        CancellationToken ct)
    {
        var exists = await _repository.ExistsByEmailAsync(email, ct);
        return !exists;
    }

    private async Task<bool> BeUniqueUsernameAsync(
        string username,
        CancellationToken ct)
    {
        var exists = await _repository.ExistsByUsernameAsync(username, ct);
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
            User.CreateAsync(validCommand, cancellationToken), ct)
        .TapAsync(async (user, cancellationToken) => 
            await _repository.SaveAsync(user, cancellationToken), ct);
}
```

### Dependency Injection

Register validators with ASP.NET Core DI:

```csharp
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Register all validators from assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Or register specific validators
builder.Services.AddScoped<IValidator<CreateOrderCommand>, CreateOrderValidator>();
builder.Services.AddScoped<IValidator<RegisterUserCommand>, RegisterUserValidator>();

// Register repositories and services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();
```

## OpenTelemetry Tracing

Enable distributed tracing for Railway Oriented Programming operations and Value Objects.

### Installation

```bash
dotnet add package OpenTelemetry
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add package OpenTelemetry.Extensions.Hosting
```

### Configuration

```csharp
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("MyApplication"))
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddFunctionalDddRopInstrumentation()      // ROP tracing
            .AddFunctionalDddCvoInstrumentation()      // Common Value Objects tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();  // Export to OpenTelemetry Collector
            
        // Or use specific exporters:
        // .AddConsoleExporter()  // Development
        // .AddJaegerExporter()   // Jaeger
        // .AddZipkinExporter()   // Zipkin
    });

var app = builder.Build();
```

### What Gets Traced

**Railway Oriented Programming:**
- `Result<T>` creation (success/failure)
- Operation chains (`Bind`, `Map`, `Tap`, `Ensure`, etc.)
- Error propagation and transformation
- Compensation operations
- Async operations with timing

**Common Value Objects:**
- Value object creation attempts
- Validation success/failure
- Error details and field names

### Trace Attributes

Traces include detailed attributes for observability:

```csharp
// Example trace for EmailAddress.TryCreate("invalid@")
Span: EmailAddress.TryCreate
├─ Attributes:
│  ├─ result.is_success: false
│  ├─ result.error.type: ValidationError
│  ├─ result.error.code: email.invalid
│  ├─ result.error.detail: Invalid email format
│  ├─ input.value: invalid@
│  └─ operation.duration_ms: 0.42

// Example trace for a Result chain
Span: ProcessOrder
├─ Span: ValidateCustomerId
│  ├─ result.is_success: true
│  └─ duration: 0.15ms
├─ Span: CheckInventory
│  ├─ result.is_success: true
│  └─ duration: 12.3ms
├─ Span: ProcessPayment
│  ├─ result.is_success: false
│  ├─ result.error.type: DomainError
│  └─ result.error.detail: Insufficient funds
```

### Viewing Traces

Compatible with popular tracing backends:

- **[Jaeger](https://www.jaegertracing.io/)** - Open-source distributed tracing
- **[Zipkin](https://zipkin.io/)** - Distributed tracing system
- **[Azure Application Insights](https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview)** - Cloud-based APM
- **[Grafana Tempo](https://grafana.com/oss/tempo/)** - Open-source tracing backend
- **[Honeycomb](https://www.honeycomb.io/)** - Observability platform
- **[Datadog APM](https://www.datadoghq.com/product/apm/)** - Application performance monitoring

## Entity Framework Core

Integrate Result types with Entity Framework Core repositories:

### Repository Pattern with Results

```csharp
using Microsoft.EntityFrameworkCore;
using FunctionalDdd;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<User>> GetByIdAsync(
        UserId id, 
        CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        
        return user != null
            ? Result.Success(user)
            : Result.Failure<User>(Error.NotFound($"User {id} not found"));
    }

    public async Task<Result<User>> GetByEmailAsync(
        EmailAddress email,
        CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);
        
        return user.ToResult(Error.NotFound($"User with email {email} not found"));
    }

    public async Task<Result<Unit>> SaveAsync(
        User user,
        CancellationToken ct)
    {
        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error.Conflict("User was modified by another process");
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
        {
            return Error.Conflict("User with this email already exists");
        }
    }

    public async Task<Result<Unit>> DeleteAsync(
        UserId id,
        CancellationToken ct)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, ct);
        
        if (user == null)
            return Error.NotFound($"User {id} not found");
        
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static bool IsDuplicateKeyException(DbUpdateException ex)
    {
        // Check for SQL Server unique constraint violation
        return ex.InnerException?.Message.Contains("duplicate key") ?? false;
    }
}
```

### Extension Method for Nullable Conversion

Create a reusable extension method for common nullable-to-Result conversions:

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
    
    public static async Task<Result<T>> ToResultAsync<T>(
        this Task<T?> task,
        Error notFoundError) where T : struct
    {
        var entity = await task;
        return entity.HasValue
            ? Result.Success(entity.Value)
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

### Handling Database Exceptions

```csharp
public async Task<Result<Order>> CreateOrderAsync(
    Order order,
    CancellationToken ct)
{
    try
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(ct);
        return Result.Success(order);
    }
    catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
    {
        return Error.Conflict(
            $"Order with ID {order.Id} already exists",
            order.Id.ToString());
    }
    catch (DbUpdateConcurrencyException)
    {
        return Error.Conflict(
            "Order was modified by another process",
            order.Id.ToString());
    }
    catch (DbUpdateException ex)
    {
        // Log the exception details
        _logger.LogError(ex, "Database error creating order {OrderId}", order.Id);
        return Error.Unexpected(
            "Failed to save order to database",
            order.Id.ToString());
    }
}
```

### Pagination with EF Core

```csharp
public async Task<Result<PagedResult<User>>> GetPagedAsync(
    int page,
    int pageSize,
    CancellationToken ct)
{
    try
    {
        var skip = page * pageSize;
        var totalCount = await _context.Users.CountAsync(ct);
        
        var users = await _context.Users
            .OrderBy(u => u.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);
        
        var result = new PagedResult<User>(
            Items: users,
            From: skip,
            To: skip + users.Count - 1,
            TotalCount: totalCount
        );
        
        return Result.Success(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching paged users");
        return Error.Unexpected("Failed to retrieve users");
    }
}
```

## Problem Details (RFC 7807)

The Asp package automatically formats errors using the [Problem Details](https://tools.ietf.org/html/rfc7807) standard.

### Standard Problem Details Response

```http
HTTP/1.1 404 Not Found
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "User 12345 not found",
  "instance": "/api/users/12345"
}
```

### Validation Error Response

Validation errors include field-level details:

```http
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "User registration validation failed",
  "instance": "/api/users",
  "errors": {
    "email": [
      "Email address is invalid",
      "Email is required"
    ],
    "age": [
      "Must be 18 or older"
    ],
    "password": [
      "Password must be at least 8 characters"
    ]
  }
}
```

### Custom Problem Details

You can customize Problem Details responses using `MatchError`:

```csharp
app.MapPost("/orders", async (CreateOrderRequest request, CancellationToken ct) =>
{
    return await ProcessOrderAsync(request, ct)
        .MatchErrorAsync(
            onValidation: err => Results.Problem(
                type: "https://myapi.com/errors/validation",
                title: "Validation Failed",
                detail: err.Detail,
                statusCode: 400,
                extensions: new Dictionary<string, object?>
                {
                    ["errors"] = err.FieldErrors.ToDictionary(
                        f => f.FieldName,
                        f => f.Details.ToArray()),
                    ["traceId"] = Activity.Current?.Id
                }),
            onDomain: err => Results.Problem(
                type: "https://myapi.com/errors/business-rule",
                title: "Business Rule Violation",
                detail: err.Detail,
                statusCode: 422),
            onSuccess: order => Results.Created($"/orders/{order.Id}", order),
            cancellationToken: ct
        );
});
```

## Best Practices

### 1. Use ToActionResult/ToHttpResult at API Boundaries

Keep `Result<T>` types internal to your application logic. Convert to HTTP responses only at the controller/endpoint level.

```csharp
// ✅ Good - Result stays in domain/application layer
public class UserService
{
    public async Task<Result<User>> CreateUserAsync(
        CreateUserCommand command,
        CancellationToken ct)
    {
        return await _validator.ValidateToResultAsync(command, ct)
            .BindAsync((cmd, cancellationToken) => 
                User.CreateAsync(cmd, cancellationToken), ct);
    }
}

[HttpPost]
public async Task<ActionResult<UserDto>> CreateUser(
    CreateUserRequest request,
    CancellationToken ct) =>
    await _userService.CreateUserAsync(request.ToCommand(), ct)
        .MapAsync(user => new UserDto(user))
        .ToActionResultAsync(this);  // Convert at boundary

// ❌ Bad - exposing Result in controller signature
public async Task<Result<User>> CreateUser(...)
```

### 2. Validate Early

Use FluentValidation at the edge of your system (API layer or application services):

```csharp
public async Task<Result<Order>> CreateOrderAsync(
    CreateOrderCommand command,
    CancellationToken ct)
{
    // Validate first, fail fast
    return await _validator.ValidateToResultAsync(command, ct)
        .BindAsync((validCmd, cancellationToken) => 
            ProcessOrderAsync(validCmd, cancellationToken), ct);
}
```

### 3. Enable Tracing in Production

Monitor error rates and performance:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerBuilder =>
    {
        tracerBuilder
            .AddFunctionalDddRopInstrumentation()
            .AddFunctionalDddCvoInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Endpoint"]);
            });
    });
```

### 4. Use Consistent Error Messages

Structure error messages with context:

```csharp
// ✅ Good - includes context
Error.NotFound($"User {userId} not found", userId.ToString())
Error.Validation("Email format is invalid", "email")
Error.Conflict("Email already in use", $"email:{email}")

// ❌ Bad - generic, no context
Error.NotFound("Not found")
Error.Validation("Invalid")
```

### 5. Repository Pattern Returns Result

Repositories should return `Result<T>`, not throw exceptions:

```csharp
// ✅ Good
public async Task<Result<User>> GetByIdAsync(UserId id, CancellationToken ct)
{
    var user = await _context.Users.FindAsync(id, ct);
    return user.ToResult(Error.NotFound($"User {id} not found"));
}

// ❌ Bad
public async Task<User> GetByIdAsync(UserId id, CancellationToken ct)
{
    var user = await _context.Users.FindAsync(id, ct);
    if (user == null)
        throw new NotFoundException($"User {id} not found");
    return user;
}
```

### 6. Always Pass CancellationToken

Support graceful cancellation in async operations:

```csharp
public async Task<Result<Order>> ProcessOrderAsync(
    CreateOrderCommand command,
    CancellationToken ct)  // ✅ Accept CancellationToken
{
    return await _validator.ValidateToResultAsync(command, ct)
        .BindAsync(
            async (cmd, cancellationToken) => 
                await CreateOrderAsync(cmd, cancellationToken),
            ct)  // ✅ Pass it through
        .TapAsync(
            async (order, cancellationToken) => 
                await _repository.SaveAsync(order, cancellationToken),
            ct);  // ✅ Pass to all async operations
}
```

### 7. Use Unit for Side-Effect Operations

Operations that don't return data should return `Result<Unit>`:

```csharp
[HttpDelete("{id}")]
public async Task<ActionResult<Unit>> DeleteUser(string id, CancellationToken ct) =>
    await _userService.DeleteUserAsync(id, ct)
        .ToActionResultAsync(this);
// Automatically returns 204 No Content on success
```

## Next Steps

- See [Error Handling](error-handling.md) for discriminated error matching and custom error types
- Learn about [Examples](examples.md) for complete working applications
- Review [Debugging](debugging.md) for troubleshooting ROP chains
- Check [Advanced Features](advanced-features.md) for LINQ, parallel operations, and more
