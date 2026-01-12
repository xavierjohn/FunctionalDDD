# ASP Extension

[![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.Asp.svg)](https://www.nuget.org/packages/FunctionalDDD.Asp)

This library converts Railway Oriented Programming `Result` types to ASP.NET Core HTTP responses, providing seamless integration between your functional domain layer and web API layer.

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
  - [Automatic Value Object Binding (Recommended)](#automatic-value-object-binding-recommended)
  - [MVC Controllers](#mvc-controllers)
  - [Minimal API](#minimal-api)
- [Core Concepts](#core-concepts)
- [Best Practices](#best-practices)
- [Resources](#resources)

## Installation

Install via NuGet:

```bash
dotnet add package FunctionalDDD.Asp
```

## Quick Start

### Automatic Value Object Binding

**Enable automatic binding** for value objects:

#### Option 1: Route/Query/Form Parameters Only (Recommended for most use cases)

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.AddValueObjectModelBinding();  // ✅ Enable for route/query/form params
});

var app = builder.Build();
app.MapControllers();
app.Run();
```

**Works for:**
- ✅ Route parameters: `[HttpGet("{id}")]`
- ✅ Query parameters: `[FromQuery] EmailAddress email`
- ✅ Form data: `[FromForm] FileName fileName`

**Doesn't work for:**
- ❌ JSON request bodies (`[FromBody]`) - see Option 2 below

---

#### Option 2: Full Support Including JSON Bodies (Uses Reflection)

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.AddValueObjectJsonInputFormatter(); // ✅ Enable JSON body validation
    options.AddValueObjectModelBinding();       // ✅ Enable route/query/form validation
});

var app = builder.Build();
app.MapControllers();
app.Run();
```

**Works for:**
- ✅ Route parameters
- ✅ Query parameters
- ✅ Form data
- ✅ **JSON request bodies** (`[FromBody]`)

**Trade-offs:**
- ✅ Full automatic validation everywhere
- ⚠️ Uses reflection (performance overhead)
- ⚠️ Requires record-style DTOs with primary constructors
- ⚠️ Not recommended for AOT scenarios

---

### Usage Examples

#### Route and Query Parameters (Both Options)

```csharp
// Route parameter
[HttpGet("{id}")]
public ActionResult<User> GetUser(UserId id) =>
    _repository.GetById(id)
        .ToResult(Error.NotFound($"User {id} not found"))
        .ToActionResult(this);

// Query parameter
[HttpGet("search")]
public ActionResult<IEnumerable<User>> SearchByEmail([FromQuery] EmailAddress email) =>
    _repository.FindByEmail(email)
        .ToActionResult(this);
```

#### JSON Request Bodies

**Option 2 (with `AddValueObjectJsonInputFormatter`):**
```csharp
// DTO with value objects (automatically validated!)
public record CreateUserRequest(
    FirstName FirstName,      // ✅ Automatically validated
    LastName LastName,        // ✅ Automatically validated
    EmailAddress Email        // ✅ Automatically validated
);

[HttpPost]
public ActionResult<User> Register([FromBody] CreateUserRequest request) =>
    ModelState.ToResult()  // ✅ Check model binding validation first
        .Bind(_ => User.TryCreate(request.FirstName, request.LastName, request.Email))
        .ToActionResult(this);

// Why ModelState.ToResult()?
// - Model binding validates FirstName, LastName, Email
// - If invalid, errors are in ModelState
// - ModelState.ToResult() converts to Result<Unit>
// - If ModelState is invalid, stays on failure track
// - User.TryCreate only called if ALL value objects are valid
```

**Option 1 (without JSON formatter) - Manual Validation:**
```csharp
// DTO with strings
public record RegisterRequest(string FirstName, string LastName, string Email);

[HttpPost]
public ActionResult<User> Register([FromBody] RegisterRequest request) =>
    FirstName.TryCreate(request.FirstName)
        .Combine(LastName.TryCreate(request.LastName))
        .Combine(EmailAddress.TryCreate(request.Email))
        .Bind((first, last, email) => User.TryCreate(first, last, email))
        .ToActionResult(this);
```

---

### Which Option Should You Choose?

| Scenario | Recommended Approach |
|----------|---------------------|
| **Route/query parameters only** | Option 1 - Simple, no overhead |
| **Prefer explicit validation** | Option 1 - Manual `Combine` chains |
| **Want automatic JSON validation** | Option 2 - Full automatic binding |
| **Building for AOT** | Option 1 - Avoid reflection |
| **High-performance APIs** | Option 1 - No reflection overhead |
| **Rapid development** | Option 2 - Less boilerplate |

---

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

## Resources

- [SAMPLES.md](SAMPLES.md) - Comprehensive examples and advanced patterns
- [Railway Oriented Programming](../RailwayOrientedProgramming/README.md) - Core Result<T> concepts
- [Domain-Driven Design](../DomainDrivenDesign/README.md) - Entity and value object patterns

