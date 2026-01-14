# ASP Extension

[![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.Asp.svg)](https://www.nuget.org/packages/FunctionalDDD.Asp)

This library converts Railway Oriented Programming `Result` types to ASP.NET Core HTTP responses, providing seamless integration between your functional domain layer and web API layer.

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
  - [MVC Controllers](#mvc-controllers)
  - [Minimal API](#minimal-api)
- [Automatic Value Object Validation](#automatic-value-object-validation)
  - [MVC Controllers Setup](#mvc-controllers-setup)
  - [Minimal API Setup](#minimal-api-setup)
  - [AOT Compatibility](#aot-compatibility)
- [Core Concepts](#core-concepts)
- [Best Practices](#best-practices)
- [Resources](#resources)

## Installation

Install via NuGet:

```bash
dotnet add package FunctionalDDD.Asp
```

For **AOT-compatible** applications, also reference the source generator:

```bash
dotnet add package FunctionalDDD.Asp.Generator
```

## Quick Start

### MVC Controllers

Use `ToActionResult` to convert `Result<T>` to `ActionResult<T>`:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost]
    public ActionResult<User> Register([FromBody] RegisterRequest request) =>
        FirstName.TryCreate(request.FirstName)
            .Combine(LastName.TryCreate(request.LastName))
            .Combine(EmailAddress.TryCreate(request.Email))
            .Bind((firstName, lastName, email) => 
                User.TryCreate(firstName, lastName, email, request.Password))
            .ToActionResult(this);

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserAsync(
        string id,
        CancellationToken cancellationToken) =>
        await _userRepository.GetByIdAsync(id, cancellationToken)
            .ToResultAsync(Error.NotFound($"User {id} not found"))
            .ToActionResultAsync(this);
}
```

### Minimal API

Use `ToHttpResult` to convert `Result<T>` to `IResult`:

```csharp
var userApi = app.MapGroup("/api/users");

userApi.MapPost("/register", (RegisterUserRequest request) =>
    FirstName.TryCreate(request.FirstName)
        .Combine(LastName.TryCreate(request.LastName))
        .Combine(EmailAddress.TryCreate(request.Email))
        .Bind((firstName, lastName, email) => 
            User.TryCreate(firstName, lastName, email, request.Password))
        .ToHttpResult());

userApi.MapGet("/{id}", async (
    string id,
    UserRepository repository,
    CancellationToken cancellationToken) =>
    await repository.GetByIdAsync(id, cancellationToken)
        .ToResultAsync(Error.NotFound($"User {id} not found"))
        .ToHttpResultAsync());
```

## Automatic Value Object Validation

Eliminate manual `Combine` chains by using value objects directly in your DTOs. Validation errors are automatically collected during JSON deserialization.

### The Problem

Every endpoint requires repetitive validation boilerplate:

```csharp
// ❌ Before: Manual validation in every action
[HttpPost]
public ActionResult<User> Register([FromBody] RegisterRequest request) =>
    FirstName.TryCreate(request.FirstName)
        .Combine(LastName.TryCreate(request.LastName))
        .Combine(EmailAddress.TryCreate(request.Email))
        .Bind((first, last, email) => User.TryCreate(first, last, email, request.Password))
        .ToActionResult(this);
```

### The Solution

Use value objects directly in your DTOs with automatic validation:

```csharp
// DTO with value objects
public record CreateUserRequest(
    FirstName FirstName,      // ✅ Automatically validated
    LastName LastName,        // ✅ Automatically validated
    EmailAddress Email        // ✅ Automatically validated
);

// ✅ After: Clean controller/endpoint code
[HttpPost]
public ActionResult<User> Register([FromBody] CreateUserRequest request) =>
    User.TryCreate(request.FirstName, request.LastName, request.Email, request.Password)
        .ToActionResult(this);
```

### MVC Controllers Setup

1. **Add services and middleware** in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddValueObjectValidation();  // Add this line

var app = builder.Build();

app.UseValueObjectValidation();  // Add this before routing
app.MapControllers();

app.Run();
```

2. **Create DTOs with value objects** and use them in controllers - validation happens automatically via the action filter.

### Minimal API Setup

For Minimal APIs, use the `WithValueObjectValidation()` endpoint filter:

1. **Add middleware** in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValueObjectValidation();  // Add validation services

var app = builder.Build();

app.UseValueObjectValidation();  // Enable validation scope per request
```

2. **Apply the endpoint filter** to routes that accept DTOs with value objects:

```csharp
// DTO with value objects
public record CreateUserRequest(
    FirstName FirstName,
    LastName LastName,
    EmailAddress Email,
    string Password
);

// Apply filter to enable automatic validation
app.MapPost("/users/register", (CreateUserRequest request) =>
    User.TryCreate(request.FirstName, request.LastName, request.Email, request.Password)
        .ToHttpResult())
    .WithValueObjectValidation();  // ← Add this!
```

The `WithValueObjectValidation()` filter:
- Checks for validation errors collected during JSON deserialization
- Returns 400 Bad Request with validation problem details if errors exist
- Allows the endpoint to execute if no validation errors

### How It Works

1. **Middleware** (`UseValueObjectValidation`) creates a validation scope for each request
2. **JSON Converter** deserializes value objects using `TryCreate`, collecting validation errors
3. **Action Filter** (MVC) or **Endpoint Filter** (Minimal API) checks for collected errors
4. If errors exist, returns **400 Bad Request** with validation problem details

### AOT Compatibility

The automatic validation system is **fully AOT-compatible** when using the source generator.

#### Without Source Generator (Reflection-based)

By default, the `ValidatingJsonConverterFactory` uses reflection to create converters at runtime. This works in JIT-compiled applications but **is not AOT-compatible**.

#### With Source Generator (AOT-compatible)

The `FunctionalDDD.Asp.Generator` source generator scans your project and all referenced assemblies for types implementing `ITryCreatable<T>`, then generates a module initializer that pre-registers converters at compile time.

**Setup for AOT:**

1. **Add the generator package** (or project reference):

```xml
<!-- Option 1: NuGet package (when available) -->
<PackageReference Include="FunctionalDDD.Asp.Generator" 
                  OutputItemType="Analyzer" 
                  ReferenceOutputAssembly="false" />

<!-- Option 2: Project reference -->
<ProjectReference Include="path\to\Asp.Generator.csproj" 
                  OutputItemType="Analyzer" 
                  ReferenceOutputAssembly="false" />
```

2. **Build your project** - the generator automatically creates registration code:

```csharp
// Auto-generated at compile time
namespace FunctionalDdd.Generated;

internal static class ValidatingConverterRegistration
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        ValidatingConverterRegistry.Register<EmailAddress>();
        ValidatingConverterRegistry.Register<FirstName>();
        ValidatingConverterRegistry.Register<LastName>();
        ValidatingConverterRegistry.RegisterStruct<Amount>();
        // All ITryCreatable types from your project and dependencies
    }
}
```

3. **No code changes needed** - the `ValidatingJsonConverterFactory` automatically uses pre-registered converters when available, falling back to reflection only for types not discovered at compile time.

#### How the Generator Works

```
┌─────────────────────┐
│   Your Application  │
│  (references Asp)   │
└─────────┬───────────┘
          │
          ▼
┌─────────────────────────────────────────────────────────┐
│              Asp.Generator (Source Generator)           │
├─────────────────────────────────────────────────────────┤
│  1. Scans current compilation for ITryCreatable<T>     │
│  2. Scans all referenced assemblies                     │
│  3. Generates ValidatingConverterRegistration.g.cs     │
│  4. Uses [ModuleInitializer] for auto-registration     │
└─────────────────────────────────────────────────────────┘
          │
          ▼
┌─────────────────────────────────────────────────────────┐
│           ValidatingConverterRegistry                   │
├─────────────────────────────────────────────────────────┤
│  • Thread-safe ConcurrentDictionary of converters      │
│  • Pre-instantiated converters (no reflection needed)  │
│  • Supports both class and struct value objects        │
└─────────────────────────────────────────────────────────┘
          │
          ▼
┌─────────────────────────────────────────────────────────┐
│         ValidatingJsonConverterFactory                  │
├─────────────────────────────────────────────────────────┤
│  1. Checks registry first (AOT-safe path)              │
│  2. Falls back to reflection if type not found         │
│  3. Returns pre-instantiated converter from registry   │
└─────────────────────────────────────────────────────────┘
```

#### Benefits of AOT Support

| Feature | Without Generator | With Generator |
|---------|-------------------|----------------|
| **AOT Compatible** | ❌ No | ✅ Yes |
| **Startup Performance** | Slower (reflection) | Faster (pre-compiled) |
| **Runtime Allocations** | Creates converters on-demand | Pre-instantiated |
| **Trimming Safe** | ❌ No | ✅ Yes |
| **Native AOT** | ❌ No | ✅ Yes |

### Error Response Format

Invalid requests return standard ASP.NET Core validation problem details:

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "firstName": ["First Name cannot be empty."],
        "email": ["Email address is not valid."]
    }
}
```

### Benefits

- ✅ **No boilerplate** - Validation happens automatically
- ✅ **Type safety** - Compiler enforces correct types (can't mix FirstName/LastName)
- ✅ **Self-documenting DTOs** - Clear what validation is applied
- ✅ **Consistent validation** - Same rules everywhere
- ✅ **All errors collected** - Multiple validation failures returned at once
- ✅ **Standard error format** - Matches ASP.NET Core validation responses
- ✅ **AOT-compatible** - With source generator for Native AOT support

## Core Concepts

The ASP extension automatically converts `Result<T>` outcomes to appropriate HTTP responses:

| Result Type | HTTP Status | Description |
|------------|-------------|-------------|
| Success | 200 OK | Success with content |
| Success (Unit) | 204 No Content | Success without content |
| ValidationError | 400 Bad Request | Validation errors with details |
| BadRequestError | 400 Bad Request | Invalid request |
| UnauthorizedError | 401 Unauthorized | Authentication required |
| ForbiddenError | 403 Forbidden | Access denied |
| NotFoundError | 404 Not Found | Resource not found |
| ConflictError | 409 Conflict | Resource conflict |
| DomainError | 422 Unprocessable Entity | Domain rule violation |
| RateLimitError | 429 Too Many Requests | Rate limit exceeded |
| UnexpectedError | 500 Internal Server Error | Unexpected error |
| ServiceUnavailableError | 503 Service Unavailable | Service unavailable |

**Validation Error Response Format:**

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "lastName": ["Last Name cannot be empty."],
        "email": ["Email address is not valid."]
    }
}
```

## Best Practices

1. **Always pass `this` to `ToActionResult` in MVC controllers**  
   Required for proper HTTP context access.

2. **Use async variants for async operations**  
   Use `ToActionResultAsync` and `ToHttpResultAsync` for async code paths.

3. **Always provide CancellationToken for async operations**  
   Enables proper request cancellation and resource cleanup.

4. **Use domain-specific errors, not generic exceptions**  
   Return `Error.NotFound()`, `Error.Validation()`, etc. instead of throwing exceptions.

5. **Keep domain logic out of controllers**  
   Controllers should orchestrate, not implement business rules.

6. **Use `Match` for custom responses**  
   Control specific HTTP responses or handle both success and failure paths.

7. **Use Result<Unit> for operations without return values**  
   Automatically returns 204 No Content on success.

8. **Use automatic validation for DTOs with multiple value objects**  
   Reduces boilerplate and ensures consistent validation.

9. **Apply `WithValueObjectValidation()` to Minimal API endpoints**  
   Required for endpoints that accept DTOs with value objects.

10. **Use the source generator for AOT/Native AOT applications**  
    Add `FunctionalDDD.Asp.Generator` for fully AOT-compatible validation.

## Resources

- [SAMPLES.md](SAMPLES.md) - Comprehensive examples and advanced patterns
- [Railway Oriented Programming](../RailwayOrientedProgramming/README.md) - Core Result<T> concepts
- [Domain-Driven Design](../DomainDrivenDesign/README.md) - Entity and value object patterns

