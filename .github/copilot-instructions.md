# GitHub Copilot Instructions for FunctionalDDD and Clean Architecture

## Library Overview

This workspace uses the **FunctionalDDD** library, which implements Railway Oriented Programming (ROP) and functional programming patterns for .NET applications.

## Scaffolding New Clean Architecture Projects

When asked to create a new Clean Architecture project or solution, use these commands:

### Step 1: Create Solution and Project Structure

```bash
# Create solution
dotnet new sln -n {SolutionName}

# Domain Layer (ZERO dependencies)
mkdir -p Domain/src
cd Domain/src
dotnet new classlib -n Domain -f net10.0
cd ../..

# Application Layer (depends on Domain only)
mkdir -p Application/src
cd Application/src
dotnet new classlib -n Application -f net10.0
dotnet add reference ../../Domain/src/Domain.csproj
cd ../..

# Anti-Corruption Layer / Infrastructure
mkdir -p Acl/src
cd Acl/src
dotnet new classlib -n AntiCorruptionLayer -f net10.0
dotnet add reference ../../Application/src/Application.csproj
dotnet add reference ../../Domain/src/Domain.csproj
cd ../..

# API Layer / Presentation
mkdir -p Api/src
cd Api/src
dotnet new webapi -n Api -f net10.0
dotnet add reference ../../Application/src/Application.csproj
dotnet add reference ../../Domain/src/Domain.csproj
dotnet add reference ../../Acl/src/AntiCorruptionLayer.csproj
cd ../..

# Add all projects to solution
dotnet sln add Domain/src/Domain.csproj
dotnet sln add Application/src/Application.csproj
dotnet sln add Acl/src/AntiCorruptionLayer.csproj
dotnet sln add Api/src/Api.csproj
```

### Step 2: Add FunctionalDDD NuGet Packages

```bash
# Domain - DDD patterns and value objects
cd Domain/src
dotnet add package FunctionalDDD.DomainDrivenDesign --prerelease
dotnet add package FunctionalDDD.CommonValueObjects --prerelease
dotnet add package FunctionalDDD.CommonValueObjectGenerator --prerelease
dotnet add package FluentValidation
cd ../..

# Application - Result<T> and CQRS
cd Application/src
dotnet add package FunctionalDDD.RailwayOrientedProgramming --prerelease
dotnet add package FunctionalDDD.FluentValidation --prerelease
dotnet add package Mediator.SourceGenerator
dotnet add package FluentValidation
cd ../..

# Acl - Infrastructure packages
cd Acl/src
dotnet add package FunctionalDDD.RailwayOrientedProgramming --prerelease
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
# Or use other providers: Npgsql.EntityFrameworkCore.PostgreSQL, etc.
cd ../..

# Api - ASP.NET Core integration
cd Api/src
dotnet add package FunctionalDDD.Asp --prerelease
dotnet add package Asp.Versioning.Mvc.ApiExplorer
dotnet add package Mapster
dotnet add package ServiceLevelIndicators
cd ../..
```

### Step 3: Create Folder Structure

Create these directories in each project:

```bash
# Domain layer
mkdir -p Domain/src/Aggregates
mkdir -p Domain/src/ValueObjects
mkdir -p Domain/src/Events

# Application layer
mkdir -p Application/src/Abstractions

# Acl layer
mkdir -p Acl/src/Persistence
mkdir -p Acl/src/Persistence/Configurations
mkdir -p Acl/src/Persistence/Repositories
mkdir -p Acl/src/ExternalServices

# Api layer
mkdir -p Api/src/Middleware
mkdir -p Api/src/Swagger
```

### Step 4: Create Initial Files

**Domain/src/GlobalUsings.cs**:
```csharp
global using FunctionalDdd;
global using FunctionalDdd.CommonValueObjects;
```

**Application/src/GlobalUsings.cs**:
```csharp
global using FunctionalDdd;
global using Mediator;
```

**Application/src/DependencyInjection.cs**:
```csharp
namespace Application;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
        return services;
    }
}
```

**Acl/src/DependencyInjection.cs**:
```csharp
namespace AntiCorruptionLayer;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddAntiCorruptionLayer(this IServiceCollection services)
    {
        // Register repositories, services, DbContext here
        return services;
    }
}
```

**Api/src/Middleware/ErrorHandlingMiddleware.cs**:
```csharp
namespace Api.Middleware;

internal class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        if (context.RequestServices.GetService<IProblemDetailsService>() is not { } problemDetailsService)
            return;

        ProblemDetailsContext ctx = new()
        {
            HttpContext = context,
            ProblemDetails =
            {
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An error occurred in our API. Please contact support with the trace ID.",
            }
        };
        
        var traceId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;
        ctx.ProblemDetails.Extensions["traceId"] = traceId;

        await problemDetailsService.TryWriteAsync(ctx);
    }
}
```

**Api/src/DependencyInjection.cs**:
```csharp
namespace Api;
using Microsoft.Extensions.DependencyInjection;
using Api.Middleware;

internal static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        services.AddApiVersioning()
                .AddMvc()
                .AddApiExplorer();
        
        services.AddScoped<ErrorHandlingMiddleware>();
        
        return services;
    }
}
```

**Api/src/Program.cs**:
```csharp
using Api;
using Api.Middleware;
using Application;
using AntiCorruptionLayer;

var builder = WebApplication.CreateBuilder(args);

// Register layers - order matters!
builder.Services
    .AddPresentation()          // API layer
    .AddApplication()           // Use cases
    .AddAntiCorruptionLayer();  // Infrastructure

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = string.Empty; // Swagger at root URL
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapControllers();

app.Run();

// For integration testing
public partial class Program { }
```

### Step 5: Verify Build

```bash
dotnet build
```

All projects should compile successfully with zero errors.

### Step 6: Add Test Projects (Optional but Recommended)

Create test projects for each layer:

```bash
# Domain Tests
mkdir -p Domain/tests
cd Domain/tests
dotnet new xunit -n Domain.Tests -f net10.0
dotnet add reference ../src/Domain.csproj
dotnet add package FluentAssertions
cd ../..

# Application Tests
mkdir -p Application/tests
cd Application/tests
dotnet new xunit -n Application.Tests -f net10.0
dotnet add reference ../src/Application.csproj
dotnet add package FluentAssertions
dotnet add package NSubstitute
cd ../..

# ACL Tests
mkdir -p Acl/tests
cd Acl/tests
dotnet new xunit -n AntiCorruptionLayer.Tests -f net10.0
dotnet add reference ../src/AntiCorruptionLayer.csproj
dotnet add package FluentAssertions
dotnet add package NSubstitute
dotnet add package Microsoft.EntityFrameworkCore.InMemory
cd ../..

# API Tests (Integration Tests)
mkdir -p Api/tests
cd Api/tests
dotnet new xunit -n Api.Tests -f net10.0
dotnet add reference ../src/Api.csproj
dotnet add package FluentAssertions
dotnet add package Microsoft.AspNetCore.Mvc.Testing
cd ../..

# Add test projects to solution
dotnet sln add Domain/tests/Domain.Tests.csproj
dotnet sln add Application/tests/Application.Tests.csproj
dotnet sln add Acl/tests/AntiCorruptionLayer.Tests.csproj
dotnet sln add Api/tests/Api.Tests.csproj
```

### Step 7: Create Test Base Classes and Utilities

**Domain/tests/GlobalUsings.cs**:
```csharp
global using Xunit;
global using FluentAssertions;
global using FunctionalDdd;
```

**Application/tests/GlobalUsings.cs**:
```csharp
global using Xunit;
global using FluentAssertions;
global using NSubstitute;
global using FunctionalDdd;
global using Mediator;
```

**Api/tests/GlobalUsings.cs**:
```csharp
global using Xunit;
global using FluentAssertions;
global using Microsoft.AspNetCore.Mvc.Testing;
```

**Api/tests/ApiWebApplicationFactory.cs**:
```csharp
namespace Api.Tests;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Override services for testing (e.g., replace DbContext with in-memory database)
        });
    }
}
```

### Step 8: Verify All Tests Pass

```bash
dotnet test
```

All test projects should build and pass (0 tests initially).

### When to Use This Scaffolding

- User asks to "create a new Clean Architecture project"
- User asks to "set up FunctionalDDD project structure"
- User asks to "scaffold a new solution with Clean Architecture"
- User mentions "4-layer architecture" or "onion architecture"
- Starting a new microservice or API project

### Scaffolding Notes

- Always target .NET 10 (`-f net10.0`) for new projects
- Use `AntiCorruptionLayer` as the infrastructure project name (can be shortened to `Acl` in namespaces)
- Date-based API versioning folders (e.g., `2025-01-15`) should be created when adding first controller
- Test projects follow pattern: `{ProjectName}.Tests` in `{Layer}/tests/` directory
- Never add project references that violate layer dependencies (Domain must have zero dependencies)

## Core Patterns

### Result Type Pattern

Always use `Result<T>` for operations that can fail:

```csharp
public static Result<Email> TryCreate(string email) =>
    string.IsNullOrWhiteSpace(email)
        ? Error.Validation("Email is required", "Email")
        : !email.Contains('@')
            ? Error.Validation("Invalid email format", "Email")
            : Result.Success(new Email { Value = email });
```

### Error Handling

Use specific error types from `Error` class:

- `Error.Validation(message, fieldName)` - Field validation failures (400)
- `Error.BadRequest(message)` - Malformed requests (400)
- `Error.Unauthorized(message)` - Authentication required (401)
- `Error.Forbidden(message)` - Insufficient permissions (403)
- `Error.NotFound(message)` - Resource not found (404)
- `Error.Conflict(message)` - State conflicts (409)
- `Error.Domain(message)` - Business rule violations (422)
- `Error.RateLimit(message)` - Too many requests (429)
- `Error.Unexpected(message)` - System errors (500)
- `Error.ServiceUnavailable(message)` - Temporary unavailability (503)

**Never** use exceptions for expected failures - use `Result<T>` instead.

### Railway Oriented Programming

Chain operations using fluent API:

```csharp
public Result<User> RegisterUser(string email, string firstName, string lastName)
{
    return Email.TryCreate(email)
        .Combine(FirstName.TryCreate(firstName))
        .Combine(LastName.TryCreate(lastName))
        .Bind((e, f, l) => User.Create(e, f, l))
        .Tap(user => SendWelcomeEmail(user))
        .TapError(error => LogRegistrationFailure(error));
}
```

Key operations:
- **`Bind`** - Chain operations that return `Result<T>` (can fail)
- **`Map`** - Transform successful values (pure functions)
- **`Combine`** - Validate multiple inputs, collecting all errors
- **`Tap`** - Side effects on success (logging, events)
- **`TapError`** - Side effects on failure
- **`Ensure`** - Add validation conditions
- **`Compensate`** - Error recovery/fallback logic
- **`Match`** - Handle both success and failure cases

### ASP.NET Core Integration

Convert `Result<T>` to `ActionResult<T>` using extension method:

```csharp
[HttpPost("register")]
public ActionResult<User> Register([FromBody] RegisterRequest request) =>
    Email.TryCreate(request.Email)
        .Combine(FirstName.TryCreate(request.FirstName))
        .Combine(LastName.TryCreate(request.LastName))
        .Bind((e, f, l) => User.Create(e, f, l))
        .ToActionResult(this); // Note: 'this' parameter is required
```

**Important**: Always pass `this` to `ToActionResult()` - it's an extension method that needs the controller instance.

For custom status codes (like 201 Created), use `Match`:

```csharp
[HttpPost]
public ActionResult<User> Create([FromBody] CreateUserRequest request) =>
    User.TryCreate(request)
        .Match(
            onSuccess: user => CreatedAtAction(nameof(Get), new { id = user.Id }, user),
            onFailure: error => error.ToActionResult<User>(this)
        );
```

### Unit Type for No Return Value

Use `Result<Unit>` for operations that don't return a value:

```csharp
public Result<Unit> DeleteUser(UserId id) =>
    GetUser(id)
        .Bind(user => user.SoftDelete())
        .Tap(_ => PublishUserDeletedEvent(id))
        .Map(_ => default(Unit)); // or Result.Success()

// In controllers, Unit results return 204 No Content
[HttpDelete("{id}")]
public ActionResult<Unit> Delete(string id) =>
    UserId.TryCreate(id)
        .Bind(DeleteUser)
        .ToActionResult(this); // Returns 204 No Content on success
```

## Clean Architecture Guidelines

### Project Structure (Based on FunctionalDddAspTemplate)

Organize code following Clean Architecture with **4 main layers**:

```
Solution/
├── Domain/                              # Layer 1: Core business logic (ZERO dependencies)
│   ├── src/
│   │   ├── Aggregates/                 # Root entities (User, Order, etc.)
│   │   │   └── User.cs
│   │   ├── ValueObjects/               # Strongly-typed primitives
│   │   │   ├── UserId.cs              # partial class UserId : RequiredGuid
│   │   │   ├── FirstName.cs           # partial class FirstName : RequiredString  
│   │   │   └── EmailAddress.cs        # partial class EmailAddress : RequiredString
│   │   ├── {DomainConcept}.cs         # Domain entities/records
│   │   └── Domain.csproj
│   └── tests/
│       └── Domain.Tests.csproj
│
├── Application/                         # Layer 2: Use cases (depends ONLY on Domain)
│   ├── src/
│   │   ├── {Feature}/                  # Group by feature/use-case
│   │   │   ├── CreateUserCommand.cs    # Command (write operation)
│   │   │   ├── CreateUserCommandHandler.cs
│   │   │   ├── GetUserQuery.cs         # Query (read operation)
│   │   │   └── GetUserQueryHandler.cs
│   │   ├── Abstractions/               # Interfaces for external services
│   │   │   └── IWeatherForecastService.cs
│   │   ├── DependencyInjection.cs      # services.AddApplication()
│   │   └── Application.csproj
│   └── tests/
│       └── Application.Tests.csproj
│
├── Acl/                                 # Layer 3: Anti-Corruption Layer (Infrastructure)
│   ├── src/                            # Implements Application abstractions
│   │   ├── Persistence/                # Database context, repositories
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/         # EF Core entity configurations
│   │   │   └── Repositories/
│   │   ├── ExternalServices/           # Adapters for 3rd party APIs
│   │   │   └── WeatherForecastService.cs  # implements IWeatherForecastService
│   │   ├── EnvironmentOptions.cs       # Configuration model
│   │   ├── DependencyInjection.cs      # services.AddAntiCorruptionLayer()
│   │   └── AntiCorruptionLayer.csproj
│   └── tests/
│       └── Acl.Tests.csproj
│
└── Api/                                 # Layer 4: Presentation (depends on all)
    ├── src/
    │   ├── {ApiVersion}/               # Date-based API versioning (e.g., 2023-06-06)
    │   │   ├── Controllers/
    │   │   │   ├── UsersController.cs
    │   │   │   └── WeatherForecastController.cs
    │   │   └── Models/                 # DTOs for this API version
    │   │       ├── RegisterUserRequest.cs
    │   │       └── ConfigureMapster.cs # Mapster mappings
    │   ├── Middleware/
    │   │   └── ErrorHandlingMiddleware.cs
    │   ├── Swagger/
    │   │   └── ConfigureSwaggerOptions.cs
    │   ├── Program.cs                  # Entry point
    │   ├── DependencyInjection.cs      # services.AddPresentation()
    │   ├── appsettings.json
    │   └── Api.csproj
    └── tests/
        └── Api.Tests.csproj
```

### Layer Dependency Rules

**Critical**: Dependencies **ALWAYS** point inward (toward Domain):

```
Api → Acl → Application → Domain
 ↓     ↓        ↓          ↓
All   Infra   Use Cases   Core
```

- **Domain**: No dependencies (pure business logic)
- **Application**: References Domain only
- **Acl**: References Application + Domain
- **Api**: References all layers (wires everything together)

### Domain Layer Patterns

#### Value Objects with CommonValueObjects Source Generator

Use **partial classes** inheriting from `RequiredGuid`, `RequiredString`, etc.:

```csharp
// Domain/src/ValueObjects/UserId.cs
namespace YourApp.Domain;

public partial class UserId : RequiredGuid
{
}

// Domain/src/ValueObjects/FirstName.cs
public partial class FirstName : RequiredString
{
}

// Domain/src/ValueObjects/EmailAddress.cs (if using custom email - otherwise use FunctionalDDD.CommonValueObjects.EmailAddress)
public partial class EmailAddress : RequiredString
{
}
```

The source generator automatically provides:
- `TryCreate()` factory method
- Validation logic
- Implicit conversions
- Value equality

**Note**: For email addresses, use the pre-built `EmailAddress` class from `FunctionalDdd.CommonValueObjects`:

```csharp
using FunctionalDdd;

// Use the pre-built EmailAddress (no need for partial class)
var email = EmailAddress.TryCreate("user@example.com");
```

#### Aggregate Roots

Place in `Domain/src/Aggregates/`:

```csharp
namespace YourApp.Domain;
using FluentValidation;

public class User : Aggregate<UserId>
{
    public FirstName FirstName { get; }
    public LastName LastName { get; }
    public EmailAddress Email { get; }
    public string Password { get; }

    public static Result<User> TryCreate(FirstName firstName, LastName lastName, EmailAddress email, string password)
    {
        var user = new User(firstName, lastName, email, password);
        var validator = new UserValidator();
        return validator.ValidateToResult(user);
    }

    private User(FirstName firstName, LastName lastName, EmailAddress email, string password)
        : base(UserId.NewUnique())
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
    }

    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(user => user.FirstName).NotNull();
            RuleFor(user => user.LastName).NotNull();
            RuleFor(user => user.Email).NotNull();
            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password must not be empty.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        }
    }
}
```

### Application Layer Patterns (CQRS with Mediator)

#### Queries (Read Operations)

Group by feature in `Application/src/{Feature}/`:

```csharp
// Application/src/WeatherForcast/WeatherForecastQuery.cs
namespace YourApp.Application.WeatherForcast;
using YourApp.Domain;
using FluentValidation;
using Mediator;

public class WeatherForecastQuery : IQuery<Result<WeatherForecast>>
{
    public ZipCode ZipCode { get; }

    public static Result<WeatherForecastQuery> TryCreate(ZipCode zipCode)
        => s_validator.ValidateToResult(new WeatherForecastQuery(zipCode));

    private WeatherForecastQuery(ZipCode zipCode) => ZipCode = zipCode;
    
    private static readonly InlineValidator<WeatherForecastQuery> s_validator = new()
    {
        v => v.RuleFor(x => x.ZipCode).NotNull(),
    };
}

// Application/src/WeatherForcast/WeatherForecastQueryHandler.cs
public class WeatherForecastQueryHandler : IQueryHandler<WeatherForecastQuery, Result<WeatherForecast>>
{
    private readonly IWeatherForecastService _weatherForcastService;

    public WeatherForecastQueryHandler(IWeatherForecastService weatherForcastService) 
        => _weatherForcastService = weatherForcastService;

    public async ValueTask<Result<WeatherForecast>> Handle(WeatherForecastQuery query, CancellationToken cancellationToken)
        => await _weatherForcastService.GetWeatherForecast(query.ZipCode);
}
```

#### Commands (Write Operations)

Follow the same pattern but return `Result<Unit>` or `Result<T>` for created entities:

```csharp
// Application/src/Users/CreateUserCommand.cs
public class CreateUserCommand : ICommand<Result<User>>
{
    public FirstName FirstName { get; }
    public LastName LastName { get; }
    public EmailAddress Email { get; }
    
    // ...factory and validation
}

// Application/src/Users/CreateUserCommandHandler.cs
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Result<User>>
{
    public async ValueTask<Result<User>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        return User.TryCreate(command.FirstName, command.LastName, command.Email, command.Password)
            .TapAsync(async user => await _repository.AddAsync(user, cancellationToken), cancellationToken);
    }
}
```

#### Application Abstractions

Define interfaces in `Application/src/Abstractions/`:

```csharp
// Application/src/Abstractions/IWeatherForecastService.cs
namespace YourApp.Application.Abstractions;

public interface IWeatherForecastService
{
    ValueTask<Result<WeatherForecast>> GetWeatherForecast(ZipCode zipCode);
}
```

#### Application Dependency Injection

```csharp
// Application/src/DependencyInjection.cs
namespace YourApp.Application;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
        return services;
    }
}
```

### Anti-Corruption Layer (ACL/Infrastructure) Patterns

#### External Service Adapters

Implement Application abstractions in `Acl/src/`:

```csharp
// Acl/src/WeatherForecastService.cs
namespace YourApp.AntiCorruptionLayer;
using YourApp.Application.Abstractions;
using YourApp.Domain;

public class WeatherForecastService : IWeatherForecastService
{
    private static readonly string[] s_summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public ValueTask<Result<WeatherForecast>> GetWeatherForecast(ZipCode zipCode)
    {
        var dailyTempratures = Enumerable.Range(1, 5).Select(index => new DailyTemperature
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            s_summaries[Random.Shared.Next(s_summaries.Length)]
        )).ToArray();

        return ValueTask.FromResult(zipCode.Value switch
        {
            "98052" => Result.Success(new WeatherForecast(zipCode, dailyTempratures)),
            "75014" => Result.Success(new WeatherForecast(zipCode, dailyTempratures)),
            _ => Result.Failure<WeatherForecast>(Error.NotFound("No weather forecast found for the zip code.", instance: zipCode))
        });
    }
}
```

#### Environment Configuration

Use `EnvironmentOptions` for deployment-specific settings:

```csharp
// Acl/src/EnvironmentOptions.cs
namespace YourApp.AntiCorruptionLayer;

public class EnvironmentOptions
{
    public string ServiceName { get; set; } = "YourService";
    public string Region { get; set; } = "local";
    public string RegionShortName { get; set; } = "local";
    public string Environment { get; set; } = EnvironmentType.Test;
    public string Cloud { get; set; } = CloudType.AzureCloud;
}
```

#### ACL Dependency Injection

```csharp
// Acl/src/DependencyInjection.cs
namespace YourApp.AntiCorruptionLayer;
using YourApp.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddAntiCorruptionLayer(this IServiceCollection services)
        => services.AddSingleton<IWeatherForecastService, WeatherForecastService>();
}
```

### API Layer Patterns

#### API Versioning with Date-Based Folders

Organize controllers by API version using date format `YYYY-MM-DD`:

```
Api/src/2023-06-06/
  ├── Controllers/
  │   ├── UsersController.cs
  │   └── WeatherForecastController.cs
  └── Models/
      ├── RegisterUserRequest.cs
      └── ConfigureMapster.cs
```

#### Controllers

```csharp
// Api/src/2023-06-06/Controllers/UsersController.cs
namespace YourApp.Api._2023_06_06.Controllers;
using Asp.Versioning;
using YourApp.Domain;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("2023-06-06")]
[Consumes("application/json")]
[Produces("application/json")]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost]
    public ActionResult<User> RegisterUser([FromBody] RegisterUserRequest request) =>
        FirstName.TryCreate(request.FirstName)
        .Combine(LastName.TryCreate(request.LastName))
        .Combine(EmailAddress.TryCreate(request.Email))
        .Bind((firstName, lastName, email) => User.TryCreate(firstName, lastName, email, request.Password))
        .ToActionResult(this);

    [HttpDelete("{id}")]
    public ActionResult<Unit> Delete(string id) =>
        UserId.TryCreate(id).Finally(
            ok => NoContent(),
            err => err.ToActionResult<Unit>(this));
}
```

#### Request Models

Use `record` types in `Api/src/{Version}/Models/`:

```csharp
// Api/src/2023-06-06/Models/RegisterUserRequest.cs
namespace YourApp.Api._2023_06_06.Models;

public record RegisterUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password
);
```

#### Mediator Integration

For CQRS queries/commands, inject `ISender`:

```csharp
[ApiController]
[ApiVersion("2023-06-06")]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ISender _sender;

    public WeatherForecastController(ISender sender) => _sender = sender;

    [HttpGet("{zipCode}")]
    [ProducesResponseType(typeof(Models.WeatherForecast), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<Models.WeatherForecast>> Get(
        [CustomerResourceId] string zipCode, 
        CancellationToken cancellationToken)
        => await ZipCode.TryCreate(zipCode)
            .Bind(static zipCode => WeatherForecastQuery.TryCreate(zipCode))
            .BindAsync(q => _sender.Send(q, cancellationToken))
            .MapAsync(r => r.Adapt<Models.WeatherForecast>())
            .ToActionResultAsync(this);
}
```

#### Error Handling Middleware

Always include global exception handler:

```csharp
// Api/src/Middleware/ErrorHandlingMiddleware.cs
namespace YourApp.Api.Middleware;

internal class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        if (context.RequestServices.GetService<IProblemDetailsService>() is not { } problem)
            return;

        ProblemDetailsContext ctx = new()
        {
            HttpContext = context,
            ProblemDetails =
            {
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An error occurred in our API. Please refer the trace id with our support team.",
            }
        };
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        ctx.ProblemDetails.Extensions["traceId"] = traceId;

        await problem.TryWriteAsync(ctx);
    }
}
```

#### Program.cs Setup

```csharp
// Api/src/Program.cs
using YourApp.Api;
using YourApp.Api.Middleware;
using YourApp.Application;
using YourApp.AntiCorruptionLayer;
using ServiceLevelIndicators;

var builder = WebApplication.CreateBuilder(args);

// Layer registration - order matters!
builder.Services
    .AddPresentation()      // API layer
    .AddApplication()       // Use cases
    .AddAntiCorruptionLayer(); // Infrastructure

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = string.Empty; // Swagger at root
        var descriptions = app.DescribeApiVersions();
        foreach (var description in descriptions)
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint(url, name);
        }
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseServiceLevelIndicator();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapControllers();

app.Run();

public partial class Program { }
```

#### API Dependency Injection

```csharp
// Api/src/DependencyInjection.cs
namespace YourApp.Api;

internal static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.ConfigureOpenTelemetry();
        services.ConfigureServiceLevelIndicators();
        services.AddProblemDetails();
        services.AddControllers();
        services.AddSwaggerGen(/* configuration */);
        services.AddApiVersioning()
                .AddMvc()
                .AddApiExplorer();
        services.AddScoped<ErrorHandlingMiddleware>();
        return services;
    }

    private static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(
                serviceName: "YourService",
                serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown"))
            .WithMetrics(builder =>
            {
                builder.AddAspNetCoreInstrumentation();
                builder.AddMeter("Microsoft.AspNetCore.Hosting");
                builder.AddOtlpExporter();
            })
            .WithTracing(builder =>
            {
                builder.AddAspNetCoreInstrumentation();
                builder.AddOtlpExporter();
            });
        return services;
    }

    private static IServiceCollection ConfigureServiceLevelIndicators(this IServiceCollection services)
    {
        services.AddServiceLevelIndicator(options =>
        {
            options.LocationId = ServiceLevelIndicator.CreateLocationId("public", AzureLocation.WestUS3.Name);
        })
        .AddMvc()
        .AddApiVersion();
        return services;
    }
}
```

## Async Patterns

Use async variants for I/O operations:

```csharp
public async Task<Result<User>> GetUserAsync(UserId id, CancellationToken ct)
{
    return await _repository.FindByIdAsync(id, ct)
        .ToResultAsync(Error.NotFound($"User {id} not found"))
        .EnsureAsync(
            async (user, ct) => await IsActiveAsync(user, ct),
            Error.Validation("User account is inactive"),
            ct
        )
        .TapAsync(
            async (user, ct) => await LogUserAccessAsync(user.Id, ct),
            ct
        );
}
```

**Always**:
- Pass `CancellationToken` to async operations
- Use `*Async` variants: `BindAsync`, `MapAsync`, `TapAsync`, `EnsureAsync`, `CompensateAsync`
- Chain async operations with `await`

## Code Generation Rules

### When generating value objects:
1. Use `partial class {Name} : RequiredGuid` or `RequiredString` (leverages source generator)
2. For custom validation, override `TryCreate()` in the partial class
3. Place in `Domain/src/ValueObjects/` directory
4. Return appropriate `Error` types for validation failures

### When generating aggregates:
1. Inherit from `Aggregate<TId>` where `TId` is your aggregate ID type
2. Use private constructor, provide `static Result<T> TryCreate(...)` factory
3. Place in `Domain/src/Aggregates/` directory
4. Include FluentValidation validator as nested class
5. Call `validator.ValidateToResult(entity)` in factory method

### When generating queries:
1. Place in `Application/src/{Feature}/{Feature}Query.cs`
2. Implement `IQuery<Result<T>>`
3. Include static `TryCreate()` factory with FluentValidation
4. Create corresponding handler in `{Feature}QueryHandler.cs`
5. Handler implements `IQueryHandler<TQuery, Result<T>>`

### When generating commands:
1. Place in `Application/src/{Feature}/{Feature}Command.cs`
2. Implement `ICommand<Result<T>>` or `ICommand<Result<Unit>>`
3. Include static `TryCreate()` factory with FluentValidation
4. Create corresponding handler in `{Feature}CommandHandler.cs`
5. Handler implements `ICommandHandler<TCommand, Result<T>>`

### When generating controllers:
1. Place in `Api/src/{ApiVersion}/Controllers/`
2. Use `[ApiVersion("{Version}")]` attribute with date format
3. Inject `ISender` for mediator pattern
4. Return `ActionResult<T>` from action methods
5. Chain validations with `Combine`
6. Always call `.ToActionResult(this)` at the end
7. Use `ToActionResultAsync(this)` for async operations

### When generating request DTOs:
1. Use `record` types
2. Place in `Api/src/{ApiVersion}/Models/`
3. Include XML comments for Swagger documentation
4. Use simple types (string, int, etc.) - validation happens in value objects

### When generating external service adapters:
1. Place in `Acl/src/` or `Acl/src/ExternalServices/`
2. Implement Application abstraction interfaces
3. Return `Result<T>` or `ValueTask<Result<T>>`
4. Handle external errors gracefully, map to appropriate `Error` types

## Common Mistakes to Avoid

❌ **Don't throw exceptions for expected failures:**
```csharp
// BAD
if (email == null) throw new ArgumentNullException(nameof(email));

// GOOD
if (string.IsNullOrWhiteSpace(email))
    return Error.Validation("Email is required", "Email");
```

❌ **Don't use generic error messages:**
```csharp
// BAD
return Error.Unexpected("Something went wrong");

// GOOD
return Error.NotFound($"User with ID {userId} not found");
```

❌ **Don't forget to pass 'this' to ToActionResult:**
```csharp
// BAD
.ToActionResult(); // Won't compile!

// GOOD
.ToActionResult(this);
```

❌ **Don't nest Bind for independent validations:**
```csharp
// BAD - Sequential, fails fast
Email.TryCreate(email).Bind(e =>
    FirstName.TryCreate(firstName).Bind(f =>
        User.Create(e, f)));

// GOOD - Parallel, collects all errors
Email.TryCreate(email)
    .Combine(FirstName.TryCreate(firstName))
    .Bind((e, f) => User.Create(e, f));
```

❌ **Don't access Result.Value without checking:**
```csharp
// BAD - Throws if IsFailure
var user = GetUser(id).Value;

// GOOD
var result = GetUser(id);
if (result.IsSuccess)
    var user = result.Value;

// BETTER
GetUser(id).Match(
    onSuccess: user => ProcessUser(user),
    onFailure: error => HandleError(error)
);
```

❌ **Don't mix layer concerns:**
```csharp
// BAD - Domain referencing Application
public class User
{
    private readonly IEmailService _emailService; // NO!
}

// GOOD - Raise domain event, handle in Application
public class User
{
    public Result<Unit> Activate()
    {
        // ... business logic
        RaiseDomainEvent(new UserActivatedEvent(Id));
        return Result.Success();
    }
}
```

❌ **Don't create controllers without API versioning:**
```csharp
// BAD
namespace YourApp.Api.Controllers; // No version!

// GOOD
namespace YourApp.Api._2023_06_06.Controllers;
```

## Testing Patterns

### Unit Tests

```csharp
[Fact]
public void TryCreate_WithInvalidEmail_ReturnsValidationError()
{
    // Arrange
    var invalidEmail = "not-an-email";

    // Act
    var result = EmailAddress.TryCreate(invalidEmail);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().BeOfType<ValidationError>();
    result.Error.Code.Should().Be("validation.error");
}

[Fact]
public void RegisterUser_WithValidInputs_ReturnsSuccess()
{
    // Arrange
    var email = "user@example.com";
    var firstName = "John";
    var lastName = "Doe";

    // Act
    var result = RegisterUser(email, firstName, lastName);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Email.Value.Should().Be(email);
}

[Fact]
public void RegisterUser_WithMultipleInvalidInputs_ReturnsAllErrors()
{
    // Arrange
    var email = ""; // Invalid
    var firstName = ""; // Invalid
    var lastName = "Doe";

    // Act
    var result = RegisterUser(email, firstName, lastName);

    // Assert
    result.IsFailure.Should().BeTrue();
    var validationError = result.Error.Should().BeOfType<ValidationError>().Subject;
    validationError.FieldErrors.Should().HaveCount(2);
    validationError.FieldErrors.Should().Contain(e => e.FieldName == "Email");
    validationError.FieldErrors.Should().Contain(e => e.FieldName == "FirstName");
}
```

## FluentValidation Integration

When using FluentValidation with FunctionalDDD:

```csharp
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50);
    }
}

// In aggregate/entity
public static Result<User> TryCreate(FirstName firstName, LastName lastName, EmailAddress email, string password)
{
    var user = new User(firstName, lastName, email, password);
    var validator = new UserValidator();
    return validator.ValidateToResult(user); // Extension method from FunctionalDDD.FluentValidation
}
```

## Summary

When generating code for FunctionalDDD repositories:

1. ✅ Use `Result<T>` for all operations that can fail
2. ✅ Follow 4-layer Clean Architecture (Domain → Application → Acl → Api)
3. ✅ Use partial classes with `RequiredGuid`/`RequiredString` for value objects
4. ✅ Organize Application by feature with CQRS (Commands/Queries + Handlers)
5. ✅ Place controllers in `Api/src/{ApiVersion}/Controllers/`
6. ✅ Implement Application abstractions in Acl layer
7. ✅ Chain operations with `Bind`, `Map`, `Combine`, `Tap`
8. ✅ Use specific `Error` types (Validation, NotFound, Domain, etc.)
9. ✅ Always pass `this` to `.ToActionResult(this)`
10. ✅ Combine independent validations to collect all errors
11. ✅ Pass `CancellationToken` for async operations
12. ✅ Register middleware in `Program.cs` (especially `ErrorHandlingMiddleware`)

Remember: **No exceptions for expected failures** - use `Result<T>` instead!
