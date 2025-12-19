# Common Value Objects

[![NuGet Package](https://img.shields.io/nuget/v/FunctionalDDD.CommonValueObjects.svg)](https://www.nuget.org/packages/FunctionalDDD.CommonValueObjects)

This library provides common value objects with source code generation for eliminating boilerplate code in domain-driven design applications.

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
dotnet add package FunctionalDDD.CommonValueObjects
dotnet add package FunctionalDDD.CommonValueObjectGenerator
```

**Important:** Both packages are required:
- `FunctionalDDD.CommonValueObjects` - Provides base classes and the `EmailAddress` value object
- `FunctionalDDD.CommonValueObjectGenerator` - Source generator that creates implementations for `RequiredString` and `RequiredGuid` derived classes

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
| **RequiredString** | String wrapper | Non-empty strings | Source generation, IParsable, explicit cast |
| **RequiredGuid** | Guid wrapper | Non-default GUIDs | Source generation, NewUnique(), IParsable |
| **EmailAddress** | String wrapper | Email validation | RFC 5322 compliant, case-insensitive |

**Generated Code Features:**
- `TryCreate` methods returning `Result<T>`
- IParsable<T> implementation (Parse/TryParse)
- Explicit cast operators
- Validation with descriptive error messages
- Property name inference for error messages

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
   `EmployeeId` is more expressive than `Guid` or `string`.

## Resources

- [SAMPLES.md](SAMPLES.md) - Comprehensive examples and patterns
- [Railway Oriented Programming](../RailwayOrientedProgramming/README.md) - Core Result<T> concepts
- [Domain-Driven Design](../DomainDrivenDesign/README.md) - Entity and value object patterns