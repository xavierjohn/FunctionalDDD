# Primitive Value Objects

[![NuGet Package](https://img.shields.io/nuget/v/FunctionalDdd.PrimitiveValueObjects.svg)](https://www.nuget.org/packages/FunctionalDdd.PrimitiveValueObjects)

Infrastructure and ready-to-use implementations for primitive value objects with source code generation, eliminating boilerplate code and primitive obsession in domain-driven design applications.

## Installation

Install both packages via NuGet:

```bash
dotnet add package FunctionalDdd.PrimitiveValueObjects
dotnet add package FunctionalDdd.PrimitiveValueObjectGenerator
```

**Important:** Both packages are required:
- `FunctionalDdd.PrimitiveValueObjects` — Base classes and **11 ready-to-use value objects**
- `FunctionalDdd.PrimitiveValueObjectGenerator` — Source generator for `Required*` derivatives

## Quick Start

### RequiredString

Create strongly-typed string value objects using source code generation:

```csharp
public partial class TrackingId : RequiredString<TrackingId>
{
}

// The source generator automatically creates:
// - TryCreate(string?, string? fieldName = null) -> Result<TrackingId>
// - Parse / TryParse (IParsable<T>)
// - explicit operator TrackingId(string)

var result = TrackingId.TryCreate("TRK-12345");
if (result.IsSuccess)
    Console.WriteLine(result.Value); // TRK-12345

// With custom field name for validation errors
var result2 = TrackingId.TryCreate(input, "shipment.trackingId");
```

### RequiredGuid

```csharp
public partial class EmployeeId : RequiredGuid<EmployeeId>
{
}

var employeeId = EmployeeId.NewUnique();
var result = EmployeeId.TryCreate(guid);
var result2 = EmployeeId.TryCreate("550e8400-e29b-41d4-a716-446655440000");
```

### RequiredUlid

Time-ordered, lexicographically sortable identifiers:

```csharp
public partial class OrderId : RequiredUlid<OrderId>
{
}

var orderId = OrderId.NewUnique();
var result = OrderId.TryCreate("01ARZ3NDEKTSV4RRFFQ69G5FAV");
```

### RequiredInt and RequiredDecimal

```csharp
public partial class Quantity : RequiredInt<Quantity> { }
public partial class Price : RequiredDecimal<Price> { }

var qty = Quantity.TryCreate(10);
var price = Price.TryCreate(99.99m);
```

## Ready-to-Use Value Objects

| Value Object | Purpose | Validation | Example |
|-------------|----------|------------|---------|
| **EmailAddress** | Email validation | RFC 5322 compliant | `user@example.com` |
| **Url** | Web URLs | Absolute HTTP/HTTPS | `https://example.com` |
| **PhoneNumber** | Phone numbers | E.164 format | `+14155551234` |
| **Percentage** | Percentage values | 0–100 range | `15.5` or `15.5%` |
| **CurrencyCode** | Currency codes | ISO 4217 3-letter | `USD`, `EUR` |
| **IpAddress** | IP addresses | IPv4 and IPv6 | `192.168.1.1` |
| **Hostname** | Hostnames | RFC 1123 | `example.com` |
| **Slug** | URL slugs | Lowercase, digits, hyphens | `my-blog-post` |
| **CountryCode** | Country codes | ISO 3166-1 alpha-2 | `US`, `GB` |
| **LanguageCode** | Language codes | ISO 639-1 alpha-2 | `en`, `es` |
| **Age** | Age values | 0–150 range | `42` |

```csharp
var email = EmailAddress.TryCreate("user@example.com");
var url = Url.TryCreate("https://example.com/path");
var phone = PhoneNumber.TryCreate("+14155551234");
var pct = Percentage.TryCreate(15.5m);
```

## ASP.NET Core Integration

Value objects implementing `IScalarValue` work seamlessly with ASP.NET Core:

```csharp
// 1. Register in Program.cs
builder.Services
    .AddControllers()
    .AddScalarValueValidation();

// 2. Use in DTOs
public record CreateUserDto
{
    public FirstName FirstName { get; init; } = null!;
    public EmailAddress Email { get; init; } = null!;
    public Maybe<Url> Website { get; init; } // Optional — null → Maybe.None
}

// 3. Controllers get automatic validation
[HttpPost]
public IActionResult Create(CreateUserDto dto)
{
    // All value objects validated on deserialization!
    return Ok(dto);
}
```

See [FunctionalDdd.Asp](https://www.nuget.org/packages/FunctionalDdd.Asp) for full ASP.NET Core integration.

## Generated Code Features

- `TryCreate` methods returning `Result<T>` with optional `fieldName` parameter
- `IParsable<T>` implementation (`Parse`/`TryParse`)
- Explicit cast operators
- Property name inference for error messages (class name → camelCase)
- JSON serialization via `ParsableJsonConverter<T>`
- OpenTelemetry activity tracing support

## Best Practices

1. **Use partial classes** — Required for source code generation
2. **Leverage generated methods** — Use `TryCreate` for safe parsing
3. **Use the fieldName parameter** — Better validation error messages in APIs
4. **Prefer specific types over primitives** — `EmployeeId` is more expressive than `Guid`
5. **Use meaningful names** — Class name becomes part of error messages

## Related Packages

- [FunctionalDdd.PrimitiveValueObjectGenerator](https://www.nuget.org/packages/FunctionalDdd.PrimitiveValueObjectGenerator) — Source generator (required companion)
- [FunctionalDdd.RailwayOrientedProgramming](https://www.nuget.org/packages/FunctionalDdd.RailwayOrientedProgramming) — Core `Result<T>` type
- [FunctionalDdd.DomainDrivenDesign](https://www.nuget.org/packages/FunctionalDdd.DomainDrivenDesign) — Entity and aggregate patterns
- [FunctionalDdd.Asp](https://www.nuget.org/packages/FunctionalDdd.Asp) — ASP.NET Core integration

## License

MIT — see [LICENSE](https://github.com/xavierjohn/FunctionalDDD/blob/main/LICENSE) for details.
