# FunctionalDDD.Asp.Generator

A Roslyn source generator that enables **AOT-compatible** JSON validation for value objects implementing `ITryCreatable<T>`.

## Overview

This source generator scans your project and all referenced assemblies for types implementing `ITryCreatable<T>`, then generates a module initializer that pre-registers JSON converters at compile time. This eliminates the need for reflection at runtime, making the validation system fully compatible with:

- ✅ **Native AOT** (.NET 7+)
- ✅ **Assembly Trimming**
- ✅ **Single-file deployments**
- ✅ **Faster startup** (no reflection overhead)

## Installation

Add a project reference with `OutputItemType="Analyzer"`:

```xml
<ProjectReference Include="path\to\Asp.Generator.csproj" 
                  OutputItemType="Analyzer" 
                  ReferenceOutputAssembly="false" />
```

Or when published as a NuGet package:

```xml
<PackageReference Include="FunctionalDDD.Asp.Generator" 
                  OutputItemType="Analyzer" 
                  ReferenceOutputAssembly="false" />
```

## How It Works

### 1. Compile-Time Scanning

The generator discovers all types implementing `ITryCreatable<T>` in:
- Your current project
- All referenced assemblies (including NuGet packages)

### 2. Code Generation

It generates a module initializer that registers converters at application startup:

```csharp
// Auto-generated: ValidatingConverterRegistration.g.cs
namespace FunctionalDdd.Generated;

using System.Runtime.CompilerServices;
using FunctionalDdd;

internal static class ValidatingConverterRegistration
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Class value objects
        ValidatingConverterRegistry.Register<EmailAddress>();
        ValidatingConverterRegistry.Register<FirstName>();
        ValidatingConverterRegistry.Register<LastName>();
        
        // Struct value objects
        ValidatingConverterRegistry.RegisterStruct<Amount>();
        ValidatingConverterRegistry.RegisterStruct<Quantity>();
    }
}
```

### 3. Runtime Usage

The `ValidatingJsonConverterFactory` automatically uses pre-registered converters:

```csharp
public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
{
    // ✅ AOT-safe path: Check registry first
    var converter = ValidatingConverterRegistry.GetConverter(typeToConvert);
    if (converter is not null)
        return converter;

    // Fallback: Reflection-based creation (for types not discovered at compile time)
    return CreateConverterWithReflection(typeToConvert);
}
```

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Your Application                          │
│  ┌─────────────────┐  ┌─────────────────┐                   │
│  │  EmailAddress   │  │   FirstName     │  (ITryCreatable)  │
│  └─────────────────┘  └─────────────────┘                   │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                  Asp.Generator                               │
│  ┌─────────────────────────────────────────────────────────┐│
│  │  IIncrementalGenerator                                  ││
│  │  • Scans compilation for ITryCreatable<T>               ││
│  │  • Scans referenced assemblies                          ││
│  │  • Generates registration code                          ││
│  └─────────────────────────────────────────────────────────┘│
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│              Generated Code                                  │
│  ValidatingConverterRegistration.g.cs                       │
│  [ModuleInitializer] → Runs at assembly load                │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│           ValidatingConverterRegistry                        │
│  ConcurrentDictionary<Type, JsonConverter>                  │
│  • Pre-instantiated converters                              │
│  • Thread-safe lookup                                       │
│  • No reflection at runtime                                 │
└─────────────────────────────────────────────────────────────┘
```

## Generated Output Location

The generated file is placed in:
```
obj/Debug/net10.0/generated/FunctionalDdd.Asp.Generator/
    FunctionalDdd.Generators.ValidatingConverterGenerator/
        ValidatingConverterRegistration.g.cs
```

To view generated code, add to your project file:
```xml
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</PropertyGroup>
```

## Supported Types

The generator discovers and registers converters for:

| Type | Converter | Registration Method |
|------|-----------|---------------------|
| `class : ITryCreatable<T>` | `ValidatingJsonConverter<T>` | `Register<T>()` |
| `struct : ITryCreatable<T>` | `ValidatingStructJsonConverter<T>` | `RegisterStruct<T>()` |

## Requirements

- **.NET Standard 2.0** (generator runs in compiler)
- **Target project**: .NET 6.0+ (for `[ModuleInitializer]` support)

## Troubleshooting

### Generator Not Running

1. Ensure `OutputItemType="Analyzer"` is set
2. Rebuild the solution
3. Check for analyzer errors in the Error List

### Types Not Discovered

The generator only finds types that:
- Implement `ITryCreatable<T>` where `T` is the type itself
- Are non-abstract
- Are accessible (public or internal)

### Viewing Generated Code

Add to your `.csproj`:
```xml
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</PropertyGroup>
```

Then look in `obj/Debug/net*/generated/`.

## Performance Comparison

| Scenario | Without Generator | With Generator |
|----------|-------------------|----------------|
| First converter lookup | ~50μs (reflection) | ~0.1μs (dictionary) |
| Subsequent lookups | ~0.1μs (cached) | ~0.1μs (cached) |
| Memory at startup | Higher (type metadata) | Lower (pre-compiled) |
| AOT compilation | ❌ Fails | ✅ Works |
| Assembly trimming | ❌ Breaks | ✅ Works |

## Related

- [FunctionalDDD.Asp](../src/README.md) - ASP.NET Core integration
- [ITryCreatable<T>](../../PrimitiveValueObjects/README.md) - Value object interface
- [ValidatingJsonConverter](../src/Validation/ValidatingJsonConverter.cs) - JSON converter implementation
