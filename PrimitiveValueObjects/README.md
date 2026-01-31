# Primitive Value Objects

[![NuGet Package](https://img.shields.io/nuget/v/FunctionalDdd.PrimitiveValueObjects.svg)](https://www.nuget.org/packages/FunctionalDdd.PrimitiveValueObjects)

This library provides infrastructure and ready-to-use implementations for primitive value objects with source code generation, eliminating boilerplate code and primitive obsession in domain-driven design applications.

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [RequiredString](#requiredstring)
- [RequiredGuid](#requiredguid)
- [RequiredUlid](#requiredulid)
- [RequiredInt and RequiredDecimal](#requiredint-and-requireddecimal)
- [EmailAddress](#emailaddress)
- [Additional Value Objects](#additional-value-objects)
- [ASP.NET Core Integration](#aspnet-core-integration)
- [Core Concepts](#core-concepts)
- [Best Practices](#best-practices)
- [Resources](#resources)

## Installation

Install both packages via NuGet:

```bash
dotnet add package FunctionalDdd.PrimitiveValueObjects
dotnet add package FunctionalDdd.PrimitiveValueObjectGenerator
```

**Important:** Both packages are required:
- `FunctionalDdd.PrimitiveValueObjects` - Provides base classes (`RequiredString`, `RequiredGuid`, `RequiredUlid`, `RequiredInt`, `RequiredDecimal`) and **11 ready-to-use value objects** (`EmailAddress`, `Url`, `PhoneNumber`, `Percentage`, `CurrencyCode`, `IpAddress`, `Hostname`, `Slug`, `CountryCode`, `LanguageCode`, `Age`)
- `FunctionalDdd.PrimitiveValueObjectGenerator` - Source generator that creates implementations for `Required*` base class derivatives

## Quick Start

### RequiredString

Create strongly-typed string value objects using source code generation:

```csharp
public partial class TrackingId : RequiredString<TrackingId>
{
}

// The source generator automatically creates:
// - IScalarValue<TrackingId, string> interface implementation
// - TryCreate(string) -> Result<TrackingId> (required by IScalarValue)
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
// - IScalarValue<EmployeeId, Guid> interface implementation
// - NewUnique() -> EmployeeId
// - TryCreate(Guid) -> Result<EmployeeId> (required by IScalarValue)
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

### RequiredUlid

Create strongly-typed ULID value objects for time-ordered, lexicographically sortable identifiers:

```csharp
public partial class OrderId : RequiredUlid<OrderId>
{
}

// The source generator automatically creates:
// - IScalarValue<OrderId, Ulid> interface implementation
// - NewUnique() -> OrderId (generates new time-ordered ULID)
// - TryCreate(Ulid) -> Result<OrderId> (required by IScalarValue)
// - TryCreate(Ulid?, string? fieldName = null) -> Result<OrderId>
// - TryCreate(string?, string? fieldName = null) -> Result<OrderId>
// - Parse(string, IFormatProvider?) -> OrderId
// - TryParse(string?, IFormatProvider?, out OrderId) -> bool
// - explicit operator OrderId(Ulid)

var orderId = OrderId.NewUnique(); // Create new time-ordered ULID
var result = OrderId.TryCreate(ulid);
var result2 = OrderId.TryCreate("01ARZ3NDEKTSV4RRFFQ69G5FAV");

// With custom field name for validation errors
var result3 = OrderId.TryCreate(input, "order.id");

// Supports IParsable<T>
var parsed = OrderId.Parse("01ARZ3NDEKTSV4RRFFQ69G5FAV", null);

// Explicit cast operator (throws on failure)
var orderId = (OrderId)Ulid.NewUlid();
```

**Why use ULID over GUID?**

| Feature | ULID | GUID |
|---------|------|------|
| **Sortable** | ✅ Lexicographically sortable by creation time | ❌ Random order |
| **Time-based** | ✅ First 48 bits encode millisecond timestamp | ❌ No time component |
| **Format** | 26-char Crockford Base32 (URL-safe) | 36-char with dashes |
| **Database Performance** | ✅ Better index performance (sequential) | ❌ Random distribution |
| **Use Case** | Event sourcing, distributed systems, logs | Legacy systems, existing APIs |

### RequiredInt and RequiredDecimal

Create strongly-typed numeric value objects:

```csharp
public partial class Quantity : RequiredInt<Quantity> { }
public partial class Price : RequiredDecimal<Price> { }

// Same features as RequiredGuid/RequiredString:
var qty = Quantity.TryCreate(10);
var price = Price.TryCreate(99.99m);

// Validates non-zero values
var invalid = Quantity.TryCreate(0);
// Returns: Error.Validation("Quantity cannot be empty.", "quantity")
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

### Additional Value Objects

#### Url
```csharp
var result = Url.TryCreate("https://example.com/path");
// Access URL components
if (result.IsSuccess)
{
    var url = result.Value;
    Console.WriteLine(url.Scheme);  // "https"
    Console.WriteLine(url.Host);    // "example.com"
    Console.WriteLine(url.Path);    // "/path"
    Console.WriteLine(url.IsSecure); // true
}

// Invalid URLs
var invalid = Url.TryCreate("not-a-url");
// Returns: Error.Validation("URL must be a valid absolute HTTP or HTTPS URL.", "url")
```

#### PhoneNumber
```csharp
var result = PhoneNumber.TryCreate("+14155551234");
if (result.IsSuccess)
{
    var phone = result.Value;
    Console.WriteLine(phone.GetCountryCode()); // "1"
}

// Normalizes input (removes spaces, dashes, parentheses)
var normalized = PhoneNumber.TryCreate("+1 (415) 555-1234");
// Stores as: "+14155551234"
```

#### Percentage
```csharp
var discount = Percentage.TryCreate(15.5m);
var fromFraction = Percentage.FromFraction(0.155m); // Also 15.5%

if (discount.IsSuccess)
{
    var pct = discount.Value;
    Console.WriteLine(pct.AsFraction());      // 0.155
    Console.WriteLine(pct.Of(100m));          // 15.5
    Console.WriteLine(pct.ToString());        // "15.5%"
}

// Parse with % suffix
var parsed = Percentage.Parse("20%", null); // Valid
```

#### CurrencyCode
```csharp
var result = CurrencyCode.TryCreate("USD");
// Stores as uppercase: "USD"

var invalid = CurrencyCode.TryCreate("US"); 
// Error: Must be 3-letter code
```

#### CountryCode and LanguageCode
```csharp
var country = CountryCode.TryCreate("US");  // Uppercase
var language = LanguageCode.TryCreate("en"); // Lowercase

// ISO standard codes only
var invalid = CountryCode.TryCreate("USA"); // Error: Must be 2 letters
```

#### IpAddress
```csharp
var ipv4 = IpAddress.TryCreate("192.168.1.1");
var ipv6 = IpAddress.TryCreate("::1");

if (ipv4.IsSuccess)
{
    var ip = ipv4.Value.ToIPAddress(); // Access System.Net.IPAddress
}
```

#### Hostname and Slug
```csharp
var hostname = Hostname.TryCreate("example.com");
// RFC 1123 validation

var slug = Slug.TryCreate("my-blog-post");
// Lowercase letters, digits, and hyphens only
// No leading/trailing hyphens, no consecutive hyphens

var invalid = Slug.TryCreate("My Blog Post!"); // Error
```

#### Age
```csharp
var age = Age.TryCreate(42);
// Range: 0-150

var tooOld = Age.TryCreate(200);
// Error: "Age is unrealistically high."
```

### ASP.NET Core Integration

Value objects implementing `IScalarValue` work seamlessly with ASP.NET Core for automatic validation:

```csharp
// 1. Register in Program.cs
builder.Services
    .AddControllers()
    .AddScalarValueValidation(); // Enable automatic scalar value validation!

// 2. Define your value objects (source generator adds IScalarValue automatically)
public partial class FirstName : RequiredString<FirstName> { }
public partial class CustomerId : RequiredGuid<CustomerId> { }

// 3. Use in DTOs with both custom and built-in value objects
public record CreateUserDto
{
    public FirstName FirstName { get; init; } = null!;
    public EmailAddress Email { get; init; } = null!;
    public PhoneNumber Phone { get; init; } = null!;
    public Age Age { get; init; } = null!;
    public CountryCode Country { get; init; } = null!;
    public Url? Website { get; init; } // Optional
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
        // All 6 value objects were automatically validated:
        // - FirstName: non-empty string
        // - Email: RFC 5322 format
        // - Phone: E.164 format
        // - Age: 0-150 range
        // - Country: ISO 3166-1 alpha-2 code
        // - Website: valid HTTP/HTTPS URL (optional)
        
        var user = new User(dto.FirstName, dto.Email, dto.Phone, dto.Age, dto.Country);
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

### Available Value Objects

This library provides both **base classes** for creating custom value objects and **ready-to-use** value objects for common scenarios:

#### Base Classes (with Source Generation)
| Value Object | Base Class | Purpose | Key Features |
|-------------|-----------|----------|-------------|
| **RequiredString** | Primitive wrapper | Non-empty strings | Source generation, IScalarValue, IParsable, ASP.NET validation |
| **RequiredGuid** | Primitive wrapper | Non-default GUIDs | Source generation, IScalarValue, NewUnique(), ASP.NET validation |
| **RequiredInt** | Primitive wrapper | Non-default integers | Source generation, IScalarValue, IParsable, ASP.NET validation |
| **RequiredDecimal** | Primitive wrapper | Non-default decimals | Source generation, IScalarValue, IParsable, ASP.NET validation |

#### Ready-to-Use Value Objects
| Value Object | Purpose | Validation Rules | Example |
|-------------|----------|-----------------|---------|
| **EmailAddress** | Email validation | RFC 5322 compliant, trimmed | `user@example.com` |
| **Url** | Web URLs | Absolute HTTP/HTTPS URIs | `https://example.com/path` |
| **PhoneNumber** | Phone numbers | E.164 format | `+14155551234` |
| **Percentage** | Percentage values | 0-100 range, supports % suffix | `15.5` or `15.5%` |
| **CurrencyCode** | Currency codes | ISO 4217 3-letter codes | `USD`, `EUR`, `GBP` |
| **IpAddress** | IP addresses | IPv4 and IPv6 | `192.168.1.1` or `::1` |
| **Hostname** | Hostnames | RFC 1123 compliant | `example.com` |
| **Slug** | URL slugs | Lowercase, digits, hyphens | `my-blog-post` |
| **CountryCode** | Country codes | ISO 3166-1 alpha-2 | `US`, `GB`, `FR` |
| **LanguageCode** | Language codes | ISO 639-1 alpha-2 | `en`, `es`, `fr` |
| **Age** | Age values | 0-150 range | `42` |

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
