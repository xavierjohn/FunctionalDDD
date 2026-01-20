# Primitive Value Objects

[![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.PrimitiveValueObjects.svg)](https://www.nuget.org/packages/FunctionalDDD.PrimitiveValueObjects)

This library provides infrastructure and ready-to-use implementations for primitive value objects with source code generation, eliminating boilerplate code and primitive obsession in domain-driven design applications.

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
  - [RequiredString](#requiredstring)
  - [RequiredGuid](#requiredguid)
  - [EmailAddress](#emailaddress)
- [Core Concepts](#core-concepts)
- [Best Practices](#best-practices)
- [Resources](#resources)

## Installation

Install both packages via NuGet:

```bash
dotnet add package FunctionalDDD.PrimitiveValueObjects
dotnet add package FunctionalDDD.PrimitiveValueObjectGenerator
```

**Important:** Both packages are required:
- `FunctionalDDD.PrimitiveValueObjects` - Provides base classes (`RequiredString`, `RequiredGuid`) and ready-to-use `EmailAddress` value object
- `FunctionalDDD.PrimitiveValueObjectGenerator` - Source generator that creates implementations for `RequiredString` and `RequiredGuid` derived classes

## Quick Start

### RequiredString

Create strongly-typed string value objects using source code generation:

```csharp
public partial class TrackingId : RequiredString<TrackingId>
{
}

// The source generator automatically creates:
// - IScalarValueObject<TrackingId, string> interface implementation
// - TryCreate(string) -> Result<TrackingId> (required by IScalarValueObject)
// - TryCreate(string?, string? fieldName = null) -> Result<TrackingId>
// - Parse(string, IFormatProvider?) -> TrackingId
// - TryParse(string?, IFormatProvider?, out TrackingId) -> bool
// - explicit operator TrackingId(string)

var result = TrackingId.TryCreate("TRK-12345");
if (result.IsSuccess)
{
    var trackingId = result.Value;
    Console.WriteLine(trackingId); // Outputs: TRK-12345
}

// With custom field name for validation errors
var result2 = TrackingId.TryCreate(input, "shipment.trackingId");
// Error field will be "shipment.trackingId" instead of default "trackingId"

// Supports IParsable<T>
var parsed = TrackingId.Parse("TRK-12345", null);

// Explicit cast operator (throws on failure)
var trackingId = (TrackingId)"TRK-12345";
```

### RequiredGuid

Create strongly-typed GUID value objects:

```csharp
public partial class EmployeeId : RequiredGuid<EmployeeId>
{
}

// The source generator automatically creates:
// - IScalarValueObject<EmployeeId, Guid> interface implementation
// - NewUnique() -> EmployeeId
// - TryCreate(Guid) -> Result<EmployeeId> (required by IScalarValueObject)
// - TryCreate(Guid?, string? fieldName = null) -> Result<EmployeeId>
// - TryCreate(string?, string? fieldName = null) -> Result<EmployeeId>
// - Parse(string, IFormatProvider?) -> EmployeeId
// - TryParse(string?, IFormatProvider?, out EmployeeId) -> bool
// - explicit operator EmployeeId(Guid)

var employeeId = EmployeeId.NewUnique(); // Create new GUID
var result = EmployeeId.TryCreate(guid);
var result2 = EmployeeId.TryCreate("550e8400-e29b-41d4-a716-446655440000");

// With custom field name for validation errors
var result3 = EmployeeId.TryCreate(input, "employee.id");

// Supports IParsable<T>
var parsed = EmployeeId.Parse("550e8400-e29b-41d4-a716-446655440000", null);

// Explicit cast operator (throws on failure)
var employeeId = (EmployeeId)Guid.NewGuid();
```

### EmailAddress

Pre-built email validation value object with RFC 5322 compliant validation:

```csharp
var result = EmailAddress.TryCreate("user@example.com");
if (result.IsSuccess)
{
    var email = result.Value;
    Console.WriteLine(email); // Outputs: user@example.com
}

// With custom field name for validation errors
var result2 = EmailAddress.TryCreate(input, "contact.email");

// Validation errors
var invalid = EmailAddress.TryCreate("not-an-email");
// Returns: Error.Validation("Email address is not valid.", "email")

// Supports IParsable<T>
var parsed = EmailAddress.Parse("user@example.com", null);

if (EmailAddress.TryParse("user@example.com", null, out var email))
{
    Console.WriteLine($"Valid: {email.Value}");
}
```

### ASP.NET Core Integration

Value objects implementing `IScalarValueObject` work seamlessly with ASP.NET Core for automatic validation:

```csharp
// 1. Register in Program.cs
builder.Services
    .AddControllers()
    .AddScalarValueObjectValidation(); // Enable automatic validation!

// 2. Define your value objects (source generator adds IScalarValueObject automatically)
public partial class FirstName : RequiredString<FirstName> { }
public partial class CustomerId : RequiredGuid<CustomerId> { }

// 3. Use in DTOs
public record CreateUserDto
{
    public FirstName FirstName { get; init; } = null!;
    public EmailAddress Email { get; init; } = null!;
}

// 4. Controllers get automatic validation - no manual Result.Combine needed!
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    [HttpPost]
    public IActionResult Create(CreateUserDto dto)
    {
        // If we reach here, dto is FULLY validated!
        // Model binding validated all value objects automatically
        var user = new User(dto.FirstName, dto.Email);
        return Ok(user);
    }

    [HttpGet("{id}")]
    public IActionResult Get(CustomerId id) // Route parameter validated automatically!
    {
        var user = _repository.GetById(id);
        return Ok(user);
    }
}

// Invalid requests automatically return 400 Bad Request with validation errors
```

**Benefits:**
- ✅ No manual `Result.Combine()` calls in controllers
- ✅ Works with route parameters, query strings, form data, and JSON bodies
- ✅ Validation errors automatically flow into `ModelState`
- ✅ Standard ASP.NET Core validation infrastructure
- ✅ Works with `[ApiController]` attribute for automatic 400 responses

## Core Concepts

| Value Object | Base Class | Purpose | Key Features |
|-------------|-----------|----------|-------------|
| **RequiredString** | Primitive wrapper | Non-empty strings | Source generation, IScalarValueObject, IParsable, ASP.NET validation |
| **RequiredGuid** | Primitive wrapper | Non-default GUIDs | Source generation, IScalarValueObject, NewUnique(), ASP.NET validation |
| **EmailAddress** | Domain primitive | Email validation | RFC 5322 compliant, IScalarValueObject, IParsable, ASP.NET validation |

**What are Primitive Value Objects?**

Primitive value objects wrap single primitive types (`string`, `Guid`, etc.) to provide:
- **Type safety**: Prevents mixing semantically different values
- **Domain semantics**: Makes code self-documenting and expressive
- **Validation**: Encapsulates validation rules at creation time
- **Immutability**: Ensures values cannot change after creation

**Generated Code Features:**
- `TryCreate` methods returning `Result<T>` with optional `fieldName` parameter
- `IParsable<T>` implementation (`Parse`/`TryParse`)
- Explicit cast operators
- Validation with descriptive error messages
- Property name inference for error messages (class name converted to camelCase)
- JSON serialization via `ParsableJsonConverter<T>`
- OpenTelemetry activity tracing support

## Best Practices

1. **Use partial classes**  
   Required for source code generation to work correctly.

2. **Leverage generated methods**  
   Use `TryCreate` for safe parsing that returns `Result<T>`.

3. **Use the fieldName parameter**  
   Pass custom field names for better validation error messages in APIs.

4. **Compose with other value objects**  
   Combine multiple value objects using `Combine` for validation.

5. **Use meaningful names**  
   Class name becomes part of the error message (e.g., "Employee Id cannot be empty.").

6. **Prefer specific types over primitives**  
   `EmployeeId` is more expressive than `Guid` or `string` - eliminates primitive obsession.

## Resources

- [SAMPLES.md](SAMPLES.md) - Comprehensive examples and patterns
- [Railway Oriented Programming](../RailwayOrientedProgramming/README.md) - Core Result<T> concepts
- [Domain-Driven Design](../DomainDrivenDesign/README.md) - Entity and value object patterns
