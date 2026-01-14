# ASP Extension

[![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.Asp.svg)](https://www.nuget.org/packages/FunctionalDDD.Asp)

This library converts Railway Oriented Programming `Result` types to ASP.NET Core HTTP responses, providing seamless integration between your functional domain layer and web API layer.

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
  - [MVC Controllers](#mvc-controllers)
  - [Minimal API](#minimal-api)
- [Automatic Value Object Validation](#automatic-value-object-validation)
- [Core Concepts](#core-concepts)
- [Best Practices](#best-practices)
- [Resources](#resources)

## Installation

Install via NuGet:

```bash
dotnet add package FunctionalDDD.Asp
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

// ✅ After: Clean controller action
[HttpPost]
public ActionResult<User> Register([FromBody] CreateUserRequest request) =>
    User.TryCreate(request.FirstName, request.LastName, request.Email, request.Password)
        .ToActionResult(this);
```

### Setup

1. **Add value object validation services** in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddValueObjectValidation();  // Add this line

var app = builder.Build();

app.UseValueObjectValidation();  // Add this before routing
app.MapControllers();

app.Run();
```

2. **Create DTOs with value objects**:

```csharp
public record CreateUserRequest(
    FirstName FirstName,
    LastName LastName,
    EmailAddress Email,
    string Password  // Mix value objects and primitives as needed
);
```

3. **Use in controllers** - validation happens automatically:

```csharp
[HttpPost]
public ActionResult<User> Register([FromBody] CreateUserRequest request) =>
    // If we get here, FirstName, LastName, and Email are already validated!
    User.TryCreate(request.FirstName, request.LastName, request.Email, request.Password)
        .ToActionResult(this);
```

### How It Works

1. **Middleware** (`UseValueObjectValidation`) creates a validation scope for each request
2. **JSON Converter** deserializes value objects using `TryCreate`, collecting validation errors
3. **Action Filter** checks for collected errors before the action executes
4. If errors exist, returns **400 Bad Request** with validation problem details

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

### Limitations

- **Not AOT-compatible** - Uses reflection for JSON converter factory
- **Requires ITryCreatable** - Value objects must implement `ITryCreatable<T>`

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

## Resources

- [SAMPLES.md](SAMPLES.md) - Comprehensive examples and advanced patterns
- [Railway Oriented Programming](../RailwayOrientedProgramming/README.md) - Core Result<T> concepts
- [Domain-Driven Design](../DomainDrivenDesign/README.md) - Entity and value object patterns

