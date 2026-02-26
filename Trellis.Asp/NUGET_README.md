# FunctionalDdd.Asp — ASP.NET Core Extensions

[![NuGet Package](https://img.shields.io/nuget/v/FunctionalDdd.Asp.svg)](https://www.nuget.org/packages/FunctionalDdd.Asp)

Comprehensive ASP.NET Core integration for functional domain-driven design, providing:

1. **Automatic Scalar Value Validation** — Property-aware error messages with comprehensive error collection
2. **Result-to-HTTP Conversion** — Seamless `Result<T>` to HTTP response mapping
3. **Model Binding** — Automatic binding from route/query/form/headers
4. **Optional Value Objects** — `Maybe<T>` support for JSON, model binding, and MVC validation
5. **Native AOT Support** — Optional source generator for zero-reflection overhead

## Installation

```bash
dotnet add package FunctionalDdd.Asp
```

## Scalar Value Validation

Automatically validate types implementing `IScalarValue<TSelf, TPrimitive>` during JSON deserialization and model binding with property-aware error messages.

> **Note:** This includes DDD value objects (like `ScalarValueObject<T>`) as well as any custom implementations of `IScalarValue`.

### Quick Start

**1. Define Value Objects**

```csharp
public class EmailAddress : ScalarValueObject<EmailAddress, string>,
                            IScalarValue<EmailAddress, string>
{
    private EmailAddress(string value) : base(value) { }

    public static Result<EmailAddress> TryCreate(string? value, string? fieldName = null)
    {
        var field = fieldName ?? "email";
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation("Email is required.", field);
        if (!value.Contains('@'))
            return Error.Validation("Email must contain @.", field);
        return new EmailAddress(value);
    }
}
```

**2. Use in DTOs**

```csharp
public record RegisterUserDto
{
    public EmailAddress Email { get; init; } = null!;
    public FirstName FirstName { get; init; } = null!;
    public string Password { get; init; } = null!;
}
```

**3. Setup Validation**

```csharp
var builder = WebApplication.CreateBuilder(args);

// For MVC Controllers
builder.Services
    .AddControllers()
    .AddScalarValueValidation();

// For Minimal APIs
builder.Services.AddScalarValueValidationForMinimalApi();

var app = builder.Build();
app.UseScalarValueValidation();  // Required middleware
app.Run();
```

**4. Automatic Validation**

```csharp
[HttpPost]
public IActionResult Register(RegisterUserDto dto)
{
    // If we reach here, dto is fully validated!
    return Ok(User.Create(dto.Email, dto.FirstName, dto.Password));
}
```

**Request:**
```json
{
  "email": "invalid",
  "firstName": "",
  "password": "test"
}
```

**Response (400 Bad Request):**
```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["Email must contain @."],
    "FirstName": ["Name cannot be empty."]
  }
}
```

### MVC Controllers

```csharp
builder.Services
    .AddControllers()
    .AddScalarValueValidation();  // Adds JSON validation + model binding

var app = builder.Build();
app.UseScalarValueValidation();  // Middleware
app.MapControllers();
app.Run();
```

- ✅ JSON deserialization with validation
- ✅ Model binding from route/query/form/headers
- ✅ Automatic 400 responses via `ScalarValueValidationFilter`
- ✅ Integrates with `[ApiController]` attribute

### Minimal APIs

```csharp
builder.Services.AddScalarValueValidationForMinimalApi();

var app = builder.Build();
app.UseScalarValueValidation();

app.MapPost("/users", (RegisterUserDto dto) => ...)
   .WithScalarValueValidation();  // Add filter to each endpoint

app.Run();
```

- ✅ JSON deserialization with validation
- ✅ Endpoint filter for automatic 400 responses

### Model Binding

Value objects automatically bind from various sources in MVC:

```csharp
// Route parameters
[HttpGet("{userId}")]
public IActionResult GetUser(UserId userId) => Ok(user);

// Query parameters
[HttpGet]
public IActionResult Search([FromQuery] EmailAddress email) => Ok(results);

// Headers
[HttpGet]
public IActionResult GetProfile([FromHeader(Name = "X-User-Id")] UserId userId) => Ok();
```

### Optional Value Objects with Maybe\<T\>

Use `Maybe<T>` for optional value object properties in DTOs. No additional setup is needed — `AddScalarValueValidation()` automatically registers the JSON converter, model binder, and validation suppression for `Maybe<T>` properties.

| JSON Value | Result |
|-----------|--------|
| `null` or absent | `Maybe.None` (no error) |
| Valid value | `Maybe.From(validated)` |
| Invalid value | Validation error collected |

```csharp
public record RegisterUserDto
{
    public FirstName FirstName { get; init; } = null!;   // Required
    public EmailAddress Email { get; init; } = null!;     // Required
    public Maybe<Url> Website { get; init; }              // Optional
}
```

### Native AOT Support

For Native AOT applications, add the source generator package:

```bash
dotnet add package FunctionalDdd.AspSourceGenerator
```

```csharp
[GenerateScalarValueConverters]  // ← Add this
[JsonSerializable(typeof(RegisterUserDto))]
public partial class AppJsonSerializerContext : JsonSerializerContext { }
```

The generator automatically detects all `IScalarValue` types, generates AOT-compatible converters, and adds `[JsonSerializable]` attributes.

**Note:** The source generator is **optional**. Without it, the library uses reflection (works for standard .NET).

## Result Conversion

Convert Railway Oriented Programming `Result<T>` types to HTTP responses.

### MVC Controllers

```csharp
[HttpPost]
public ActionResult<User> Register([FromBody] RegisterRequest request) =>
    FirstName.TryCreate(request.FirstName)
        .Combine(LastName.TryCreate(request.LastName))
        .Combine(EmailAddress.TryCreate(request.Email))
        .Bind((firstName, lastName, email) =>
            User.TryCreate(firstName, lastName, email, request.Password))
        .ToActionResult(this);
```

### Minimal APIs

```csharp
userApi.MapPost("/register", (RegisterUserRequest request) =>
    FirstName.TryCreate(request.FirstName)
        .Combine(LastName.TryCreate(request.LastName))
        .Combine(EmailAddress.TryCreate(request.Email))
        .Bind((firstName, lastName, email) =>
            User.TryCreate(firstName, lastName, email, request.Password))
        .ToHttpResult());
```

### HTTP Status Mapping

| Result Type | HTTP Status | Description |
|------------|-------------|-------------|
| Success | 200 OK | Success with content |
| Success (Unit) | 204 No Content | Success without content |
| ValidationError | 400 Bad Request | Validation errors with details |
| UnauthorizedError | 401 Unauthorized | Authentication required |
| ForbiddenError | 403 Forbidden | Access denied |
| NotFoundError | 404 Not Found | Resource not found |
| ConflictError | 409 Conflict | Resource conflict |
| DomainError | 422 Unprocessable Entity | Domain rule violation |
| RateLimitError | 429 Too Many Requests | Rate limit exceeded |
| UnexpectedError | 500 Internal Server Error | Unexpected error |
| ServiceUnavailableError | 503 Service Unavailable | Service unavailable |

## Property-Aware Error Messages

When the same value object type is used for multiple properties, errors correctly show property names (not type names):

```json
{
  "errors": {
    "FirstName": ["Name cannot be empty."],
    "LastName": ["Name cannot be empty."]
  }
}
```

This requires the `fieldName` parameter in `TryCreate`:

```csharp
public static Result<Name> TryCreate(string? value, string? fieldName = null)
{
    var field = fieldName ?? "name";
    if (string.IsNullOrWhiteSpace(value))
        return Error.Validation("Name cannot be empty.", field);
    return new Name(value);
}
```

## Reflection vs Source Generator

| Feature | Reflection | Source Generator |
|---------|-----------|------------------|
| **Setup** | Simple (no generator) | Requires analyzer reference |
| **Performance** | ~50μs overhead at startup | Zero overhead |
| **AOT Support** | ❌ No | ✅ Yes |
| **Trimming** | ⚠️ May break | ✅ Safe |
| **Use Case** | Prototyping, standard .NET | Production, Native AOT |

## Best Practices

1. **Always use `fieldName` parameter** — Enables property-aware errors
2. **Call validation setup in `Program.cs`** — Required for automatic validation
3. **Add `UseScalarValueValidation()` middleware** — Creates validation scope
4. **Use `[ApiController]` in MVC** — Enables automatic validation responses
5. **Use async variants for async operations** — `ToActionResultAsync`, `ToHttpResultAsync`
6. **Combine approaches** — Automatic validation for DTOs, manual `Result` chaining for business rules

## Related Packages

- [FunctionalDdd.RailwayOrientedProgramming](https://www.nuget.org/packages/FunctionalDdd.RailwayOrientedProgramming) — Core `Result<T>` type
- [FunctionalDdd.PrimitiveValueObjects](https://www.nuget.org/packages/FunctionalDdd.PrimitiveValueObjects) — Base value object types
- [FunctionalDdd.DomainDrivenDesign](https://www.nuget.org/packages/FunctionalDdd.DomainDrivenDesign) — Entity and aggregate patterns
- [FunctionalDdd.AspSourceGenerator](https://www.nuget.org/packages/FunctionalDdd.AspSourceGenerator) — AOT source generator

## License

MIT — see [LICENSE](https://github.com/xavierjohn/FunctionalDDD/blob/main/LICENSE) for details.
