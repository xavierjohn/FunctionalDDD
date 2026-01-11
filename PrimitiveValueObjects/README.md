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
public partial class TrackingId : RequiredString
{
}

// Generated methods include:
var result = TrackingId.TryCreate("TRK-12345");
if (result.IsSuccess)
{
    var trackingId = result.Value;
    Console.WriteLine(trackingId); // Outputs: TRK-12345
}

// Supports IParsable<T>
var parsed = TrackingId.Parse("TRK-12345", null);

// Explicit cast operator
var trackingId = (TrackingId)"TRK-12345";
```

### RequiredGuid

Create strongly-typed GUID value objects:

```csharp
public partial class EmployeeId : RequiredGuid
{
}

// Generated methods include:
var employeeId = EmployeeId.NewUnique(); // Create new GUID
var result = EmployeeId.TryCreate(guid);
var result2 = EmployeeId.TryCreate("550e8400-e29b-41d4-a716-446655440000");

// Supports IParsable<T>
var parsed = EmployeeId.Parse("550e8400-e29b-41d4-a716-446655440000", null);

// Explicit cast operator
var employeeId = (EmployeeId)Guid.NewGuid();
```

### EmailAddress

Pre-built email validation value object:

```csharp
var result = EmailAddress.TryCreate("user@example.com");
if (result.IsSuccess)
{
    var email = result.Value;
    Console.WriteLine(email); // Outputs: user@example.com
}

// Validation errors
var invalid = EmailAddress.TryCreate("not-an-email");
// Returns: Error.Validation("Email address is not valid.", "email")
```

## Core Concepts

| Value Object | Base Class | Purpose | Key Features |
|-------------|-----------|----------|-------------|
| **RequiredString** | Primitive wrapper | Non-empty strings | Source generation, IParsable, explicit cast |
| **RequiredGuid** | Primitive wrapper | Non-default GUIDs | Source generation, NewUnique(), IParsable |
| **EmailAddress** | Domain primitive | Email validation | RFC 5322 compliant, case-insensitive |

**What are Primitive Value Objects?**

Primitive value objects wrap single primitive types (`string`, `Guid`, etc.) to provide:
- **Type safety**: Prevents mixing semantically different values
- **Domain semantics**: Makes code self-documenting and expressive
- **Validation**: Encapsulates validation rules at creation time
- **Immutability**: Ensures values cannot change after creation

**Generated Code Features:**
- `TryCreate` methods returning `Result<T>`
- IParsable<T> implementation (Parse/TryParse)
- Explicit cast operators
- Validation with descriptive error messages
- Property name inference for error messages
- OpenTelemetry activity tracing support

## Best Practices

1. **Use partial classes**  
   Required for source code generation to work correctly.

2. **Leverage generated methods**  
   Use `TryCreate` for safe parsing that returns `Result<T>`.

3. **Compose with other value objects**  
   Combine multiple value objects using `Combine` for validation.

4. **Use meaningful names**  
   Class name becomes part of the error message (e.g., "Employee Id cannot be empty").

5. **Prefer specific types over primitives**  
   `EmployeeId` is more expressive than `Guid` or `string` - eliminates primitive obsession.

## Resources

- [SAMPLES.md](SAMPLES.md) - Comprehensive examples and patterns
- [Railway Oriented Programming](../RailwayOrientedProgramming/README.md) - Core Result<T> concepts
- [Domain-Driven Design](../DomainDrivenDesign/README.md) - Entity and value object patterns
