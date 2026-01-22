# FunctionalDDD.Asp Source Generator

A Roslyn source generator that automatically creates AOT-compatible JSON converters and serializer context entries for `IScalarValueObject<TSelf, TPrimitive>` types.

## Features

- **AOT Compatible**: Generated code works with Native AOT compilation
- **No Reflection**: All type information is resolved at compile time
- **Faster Startup**: No runtime type scanning or assembly reflection
- **Trimming Safe**: Code won't be trimmed away since it's explicitly generated
- **Automatic Discovery**: Finds all value object types in your assembly

## Usage

1. Add a reference to the source generator package in your project.

2. Create a partial `JsonSerializerContext` and mark it with `[GenerateValueObjectConverters]`:

```csharp
using System.Text.Json.Serialization;
using FunctionalDdd;

[GenerateValueObjectConverters]
[JsonSerializable(typeof(MyDto))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
```

3. The generator will automatically:
   - Create AOT-compatible JSON converters for all `IScalarValueObject<TSelf, TPrimitive>` types
   - Add `[JsonSerializable]` attributes for all value object types to your context
   - Generate a `GeneratedValueObjectConverterFactory` you can use directly

## Generated Code

For each value object type like:

```csharp
public class CustomerId : ScalarValueObject<CustomerId, Guid>, IScalarValueObject<CustomerId, Guid>
{
    // ...
}
```

The generator creates:

1. A strongly-typed `CustomerIdJsonConverter` class
2. A `[JsonSerializable(typeof(CustomerId))]` attribute on your context
3. Entry in `GeneratedValueObjectConverterFactory` for automatic converter resolution

## Using the Generated Factory

You can use the generated factory directly:

```csharp
var options = new JsonSerializerOptions
{
    TypeInfoResolver = AppJsonSerializerContext.Default
};
options.Converters.Add(new FunctionalDdd.Generated.GeneratedValueObjectConverterFactory());
```

## Benefits Over Runtime Reflection

| Feature | Runtime Reflection | Source Generator |
|---------|-------------------|------------------|
| AOT Support | ❌ Not compatible | ✅ Full support |
| Startup Time | Slower (type scanning) | Faster (precompiled) |
| Trimming | May break | Trimming safe |
| Memory | Higher (reflection cache) | Lower |
| IDE Support | None | Full IntelliSense |

## Requirements

- .NET 6.0 or later
- C# 10.0 or later
