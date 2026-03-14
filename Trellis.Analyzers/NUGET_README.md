# Roslyn Analyzers

[![NuGet Package](https://img.shields.io/nuget/v/Trellis.Analyzers.svg)](https://www.nuget.org/packages/Trellis.Analyzers)

20 Roslyn analyzers that enforce proper usage of `Result<T>` and `Maybe<T>` patterns at compile time — catch unsafe access, unhandled results, and anti-patterns before they reach production.

## Installation

```bash
dotnet add package Trellis.Analyzers
```

Analyzers integrate automatically into your build and provide warnings in Visual Studio, VS Code, and `dotnet build`.

## What It Catches

### Unsafe Access

```csharp
// TRLS003: Unsafe Result.Value access
var user = GetUser(id);
Console.WriteLine(user.Value.Name);  // May throw

// Fix: Check first, or use Match
if (user.IsSuccess)
    Console.WriteLine(user.Value.Name);
```

### Wrong Method Usage

```csharp
// TRLS002: Map returns Result<Result<T>> — use Bind
var result = userId.Map(id => GetOrder(id));   // Double-wrapped

// Fix: Bind flattens
var result = userId.Bind(id => GetOrder(id));  // Result<Order>
```

### Common Anti-Patterns

```csharp
// TRLS007: TryCreate().Value — use Create() instead
var email = EmailAddress.TryCreate(input).Value;  // Poor error message

// Fix
var email = EmailAddress.Create(input);
```

## All Rules

| Rule | Title | Severity |
|------|-------|----------|
| TRLS001 | Result return value is not handled | Warning |
| TRLS002 | Use Bind instead of Map when lambda returns Result | Info |
| TRLS003 | Unsafe access to Result.Value | Warning |
| TRLS004 | Unsafe access to Result.Error | Warning |
| TRLS005 | Consider using MatchError for error type discrimination | Info |
| TRLS006 | Unsafe access to Maybe.Value | Warning |
| TRLS007 | Use Create instead of TryCreate().Value | Warning |
| TRLS008 | Result is double-wrapped (Result\<Result\<T\>\>) | Warning |
| TRLS009 | Blocking on async Result (.Result or .Wait()) | Warning |
| TRLS010 | Use specific error type instead of base Error | Info |
| TRLS011 | Maybe is double-wrapped (Maybe\<Maybe\<T\>\>) | Warning |
| TRLS012 | Consider using Result.Combine | Info |
| TRLS013 | Consider GetValueOrDefault or Match instead of ternary | Info |
| TRLS014 | Use async method variant for async lambda | Warning |
| TRLS015 | Don't throw exceptions in Result chains | Warning |
| TRLS016 | Error message should not be empty | Warning |
| TRLS017 | Don't compare Result or Maybe to null | Warning |
| TRLS018 | Unsafe access to Value in LINQ expression | Warning |
| TRLS019 | Combine chain exceeds maximum tuple size | Error |
| TRLS020 | Use SaveChangesResultAsync instead of SaveChangesAsync | Warning |
| TRLS021 | HasIndex references a Maybe\<T\> property | Warning |

`TRLS021` recommends `HasTrellisIndex(...)` as the preferred fix for `Maybe<T>` indexes. String-based `HasIndex("_backingField")` remains valid as a fallback when you need to specify the mapped field explicitly.

## Configuration

Suppress or adjust rules using standard .NET mechanisms:

```ini
# .editorconfig
[*.cs]
dotnet_diagnostic.TRLS003.severity = suggestion
dotnet_diagnostic.TRLS012.severity = none
```

## Related Packages

- [Trellis.Results](https://www.nuget.org/packages/Trellis.Results) — Core `Result<T>` and `Maybe<T>` types
- [Trellis.Asp](https://www.nuget.org/packages/Trellis.Asp) — ASP.NET Core integration

## License

MIT — see [LICENSE](https://github.com/xavierjohn/Trellis/blob/main/LICENSE) for details.
