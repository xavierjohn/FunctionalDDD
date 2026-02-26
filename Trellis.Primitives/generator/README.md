# Trellis.Primitives.Generator

[![NuGet](https://img.shields.io/nuget/v/Trellis.Primitives.Generator.svg)](https://www.nuget.org/packages/Trellis.Primitives.Generator)

**Roslyn source generator** for creating strongly-typed value objects with automatic validation and IParsable support.

## What It Does

Automatically generates boilerplate code for classes inheriting from `RequiredString` or `RequiredGuid`:

```csharp
// You write this:
public partial class OrderId : RequiredGuid
{
}

// Generator creates:
// - TryCreate(Guid?) method returning Result<OrderId>
// - TryCreate(string?) method returning Result<OrderId>
// - NewUniqueV4() method for new GUIDs
// - NewUniqueV7() method for time-ordered GUIDs
// - Parse(string, IFormatProvider?) method (IParsable)
// - TryParse(string?, IFormatProvider?, out OrderId) method (IParsable)
// - Explicit cast operator from Guid
// - Validation error messages
```

## Installation

This package is included automatically when you install `Trellis.Primitives`:

```bash
dotnet add package Trellis.Primitives
```

**Note:** Both packages are required - the main package provides base classes, this generator creates the implementations.

## Generated API

### For RequiredString<TSelf> Classes

```csharp
public partial class ProductName : RequiredString<ProductName>
{
}

// Generated members:
ProductName.TryCreate(string?)                           // Result<ProductName>
ProductName.Parse(string, IFormatProvider?)              // ProductName (throws)
ProductName.TryParse(string?, IFormatProvider?, out...)  // bool
(ProductName)"ABC"                                       // Explicit cast
```

### For RequiredGuid<TSelf> Classes

```csharp
public partial class UserId : RequiredGuid<UserId>
{
}

// Generated members:
UserId.NewUniqueV4()                                    // New random GUID
UserId.NewUniqueV7()                                    // New time-ordered GUID
UserId.TryCreate(Guid?)                                  // Result<UserId>
UserId.TryCreate(string?)                                // Result<UserId>
UserId.Parse(string, IFormatProvider?)                   // UserId (throws)
UserId.TryParse(string?, IFormatProvider?, out...)       // bool
(UserId)Guid.NewGuid()                                   // Explicit cast
```

## Requirements

- **.NET Standard 2.0** compatible (source generators must target netstandard2.0)
- **C# 9.0+** for partial class support
- Requires `Trellis.Primitives` package

## How It Works

1. Analyzes your code for partial classes inheriting from `RequiredString<TSelf>` or `RequiredGuid<TSelf>`
2. Generates implementation code at compile-time
3. Code appears in IntelliSense automatically
4. No runtime reflection - all compile-time

## Validation

Generated code includes automatic validation:

```csharp
var result = ProductName.TryCreate("");  
// Returns: Result.Failure("Product Name cannot be empty.")

var result = UserId.TryCreate(Guid.Empty);
// Returns: Result.Failure("User Id cannot be empty.")
```

Error messages use the class name (e.g., "Product Name" from "ProductName").

## Source Code

This is a **source generator** - it runs at compile-time and generates C# code. The generated code is visible in:

- **Visual Studio:** Project → Dependencies → Analyzers → Trellis.Primitives.Generator
- **Output:** `obj/Debug/net10.0/generated/`

## Resources

- [Main Library Documentation](../../README.md)
- [Value Objects Guide](../README.md)
- [Complete Examples](../SAMPLES.md)
- [GitHub Repository](https://github.com/xavierjohn/Trellis)

## License

MIT © Xavier John
