# FunctionalDDD.Asp - ASP.NET Core Extensions

[![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.Asp.svg)](https://www.nuget.org/packages/FunctionalDDD.Asp)

Comprehensive ASP.NET Core integration for functional domain-driven design, providing:

1. **Automatic Value Object Validation** - Property-aware error messages with comprehensive error collection
2. **Result-to-HTTP Conversion** - Seamless `Result<T>` to HTTP response mapping
3. **Model Binding** - Automatic binding from route/query/form/headers
4. **Native AOT Support** - Optional source generator for zero-reflection overhead

## Table of Contents

- [Installation](#installation)
- [Value Object Validation](#value-object-validation)
  - [Quick Start](#quick-start)
  - [MVC Controllers](#mvc-controllers)
  - [Minimal APIs](#minimal-apis)
  - [Model Binding](#model-binding)
  - [Native AOT](#native-aot-support)
- [Result Conversion](#result-conversion)
  - [MVC Controllers](#result-conversion-mvc)
  - [Minimal APIs](#result-conversion-minimal-api)
- [Advanced Topics](#advanced-topics)
- [Resources](#resources)

## Installation

```bash
dotnet add package FunctionalDDD.Asp
```

## Value Object Validation

Automatically validate value objects during JSON deserialization and model binding with property-aware error messages.

### Quick Start

**1. Define Value Objects**

```csharp
public class EmailAddress : ScalarValueObject<EmailAddress, string>,
                            IScalarValueObject<EmailAddress, string>
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
    .AddScalarValueObjectValidation();

// For Minimal APIs
builder.Services.AddScalarValueObjectValidationForMinimalApi();

// Or use unified method (works for both)
builder.Services.AddValueObjectValidation();

var app = builder.Build();
app.UseValueObjectValidation();  // Required middleware
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

Full integration with MVC model binding and validation:

```csharp
builder.Services
    .AddControllers()
    .AddScalarValueObjectValidation();  // Adds JSON validation + model binding

var app = builder.Build();
app.UseValueObjectValidation();  // Middleware
app.MapControllers();
app.Run();
```

**Features:**
- ✅ JSON deserialization with validation
- ✅ Model binding from route/query/form/headers
- ✅ Automatic 400 responses via `ValueObjectValidationFilter`
- ✅ Integrates with `[ApiController]` attribute

### Minimal APIs

Endpoint-specific validation with filters:

```csharp
builder.Services.AddScalarValueObjectValidationForMinimalApi();

var app = builder.Build();
app.UseValueObjectValidation();

app.MapPost("/users", (RegisterUserDto dto) => ...)
   .WithValueObjectValidation();  // Add filter to each endpoint

app.Run();
```

**Features:**
- ✅ JSON deserialization with validation
- ✅ Endpoint filter for automatic 400 responses
- ⚠️ No automatic model binding (use JSON body)

### Model Binding

Value objects automatically bind from various sources in MVC:

```csharp
// Route parameters
[HttpGet("{userId}")]
public IActionResult GetUser(UserId userId) => Ok(user);

// Query parameters
[HttpGet]
public IActionResult Search([FromQuery] EmailAddress email) => Ok(results);

// Form data
[HttpPost]
public IActionResult Login([FromForm] EmailAddress email, [FromForm] string password) => Ok();

// Headers
[HttpGet]
public IActionResult GetProfile([FromHeader(Name = "X-User-Id")] UserId userId) => Ok();
```

### Native AOT Support

For Native AOT applications, add the source generator:

**1. Add Generator Reference**

```xml
<ItemGroup>
  <ProjectReference Include="path/to/AspSourceGenerator.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

**2. Mark Your JsonSerializerContext**

```csharp
[GenerateValueObjectConverters]  // ← Add this
[JsonSerializable(typeof(RegisterUserDto))]
[JsonSerializable(typeof(User))]
public partial class AppJsonSerializerContext : JsonSerializerContext { }
```

**3. Configure**

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));
```

The generator automatically:
- Detects all value object types
- Generates AOT-compatible converters
- Adds `[JsonSerializable]` attributes
- Enables Native AOT with `<PublishAot>true</PublishAot>`

**Note:** The source generator is **optional**. Without it, the library uses reflection (works for standard .NET). See [docs/REFLECTION-FALLBACK.md](docs/REFLECTION-FALLBACK.md) for details.

## Result Conversion

Convert Railway Oriented Programming `Result<T>` types to HTTP responses.

### Result Conversion: MVC

Use `ToActionResult` to convert `Result<T>` to `ActionResult<T>`:

```csharp
[ApiController]
[Route("api/users")]
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

### Result Conversion: Minimal API

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

### HTTP Status Mapping

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

## Advanced Topics

### Property-Aware Error Messages

When the same value object type is used for multiple properties:

```csharp
public record PersonDto
{
    public Name FirstName { get; init; }  // ← Same type
    public Name LastName { get; init; }   // ← Same type
}
```

Errors correctly show property names:

```json
{
  "errors": {
    "FirstName": ["Name cannot be empty."],
    "LastName": ["Name cannot be empty."]
  }
}
```

Not type names! This requires the `fieldName` parameter in `TryCreate`:

```csharp
public static Result<Name> TryCreate(string? value, string? fieldName = null)
{
    var field = fieldName ?? "name";  // ← Use fieldName
    if (string.IsNullOrWhiteSpace(value))
        return Error.Validation("Name cannot be empty.", field);
    return new Name(value);
}
```

### Combining Validation Approaches

You can use **both** automatic validation and manual Result chaining:

```csharp
[HttpPost]
public ActionResult<User> Register(RegisterUserDto dto)
{
    // dto.Email and dto.FirstName are already validated!
    // Now validate business rules:
    return UserService.CheckEmailNotTaken(dto.Email)
        .Bind(() => User.TryCreate(dto.Email, dto.FirstName, dto.Password))
        .ToActionResult(this);
}
```

This combines:
1. **Automatic validation** - DTO properties validated on deserialization
2. **Manual validation** - Business rules in domain layer

### Custom Validation Responses

For Minimal APIs, customize the response:

```csharp
app.MapPost("/users", (RegisterUserDto dto, HttpContext httpContext) =>
{
    var validationError = ValidationErrorsContext.GetValidationError();
    if (validationError is not null)
    {
        return Results.Json(
            new { success = false, errors = validationError.ToDictionary() },
            statusCode: 422);  // Custom status
    }

    var user = userService.Create(dto);
    return Results.Ok(user);
});
```

### Reflection vs Source Generator

| Feature | Reflection | Source Generator |
|---------|-----------|------------------|
| **Setup** | Simple (no generator) | Requires analyzer reference |
| **Performance** | ~50μs overhead at startup | Zero overhead |
| **AOT Support** | ❌ No | ✅ Yes |
| **Trimming** | ⚠️ May break | ✅ Safe |
| **Use Case** | Prototyping, standard .NET | Production, Native AOT |

See [docs/REFLECTION-FALLBACK.md](docs/REFLECTION-FALLBACK.md) for comprehensive comparison.

## Best Practices

### Value Object Validation

1. **Always use `fieldName` parameter** - Enables property-aware errors
2. **Call validation setup in `Program.cs`** - Required for automatic validation
3. **Add `UseValueObjectValidation()` middleware** - Creates validation scope
4. **Use `[ApiController]` in MVC** - Enables automatic validation responses

### Result Conversion

1. **Always pass `this` to `ToActionResult`** - Required for HTTP context
2. **Use async variants for async operations** - `ToActionResultAsync`, `ToHttpResultAsync`
3. **Always provide CancellationToken** - Enables proper cancellation
4. **Use domain-specific errors** - `Error.NotFound()`, not exceptions
5. **Keep domain logic in domain layer** - Controllers orchestrate, not implement

### General

1. **Combine approaches wisely** - Automatic validation for DTOs, manual for business rules
2. **Use source generator for production** - Better performance, AOT support
3. **Test validation thoroughly** - Unit test value objects, integration test endpoints

## Resources

- **[docs/REFLECTION-FALLBACK.md](docs/REFLECTION-FALLBACK.md)** - AOT vs reflection comparison
- **[generator/README.md](generator/README.md)** - Source generator details
- **[SAMPLES.md](SAMPLES.md)** - Comprehensive examples and patterns
- **[Railway Oriented Programming](../RailwayOrientedProgramming/README.md)** - Core `Result<T>` concepts
- **[Domain-Driven Design](../DomainDrivenDesign/README.md)** - Entity and value object patterns
- **[PrimitiveValueObjects](../PrimitiveValueObjects/README.md)** - Base value object types

## Examples

- **[SampleMinimalApi](../Examples/SampleMinimalApi/)** - Minimal API with Native AOT
- **[SampleWebApplication](../Examples/SampleWebApplication/)** - MVC controllers with validation
- **[SampleUserLibrary](../Examples/SampleUserLibrary/)** - Shared value objects

## License

Part of the FunctionalDDD library. See [LICENSE](../LICENSE) for details.
