# Trellis.Analyzers — Roslyn Analyzers

[![NuGet Package](https://img.shields.io/nuget/v/Trellis.Analyzers.svg)](https://www.nuget.org/packages/Trellis.Analyzers)

20 Roslyn analyzers that enforce proper usage of `Result<T>` and `Maybe<T>` patterns at compile time — catch unsafe access, unhandled results, and anti-patterns before they reach production.

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [What It Analyzes](#what-it-analyzes)
- [Example Diagnostics](#example-diagnostics)
- [Configuration](#configuration)
- [Analyzer Rules](#analyzer-rules)
- [Related Packages](#related-packages)
- [License](#license)

## Installation

```bash
dotnet add package Trellis.Analyzers
```

The analyzers are automatically integrated into your build process and will provide warnings/errors in Visual Studio, VS Code, and during `dotnet build`.

## Quick Start

No configuration required — install the package and the analyzers run automatically. Common issues are flagged as warnings or errors in your IDE and build output:

```csharp
// TRLS003: Unsafe access to Result.Value — check IsSuccess first
var user = GetUser(id);
Console.WriteLine(user.Value.Name);

// TRLS002: Use Bind instead of Map when lambda returns Result
var result = userId.Map(id => GetOrder(id));
```

## What It Analyzes

### Result Pattern Enforcement

The analyzers help prevent common mistakes when working with `Result<T>`:

- **Unsafe value access (TRLS003, TRLS004)** - Ensures you check `IsSuccess`/`IsFailure` before accessing `Value`/`Error`
- **Unhandled results (TRLS001)** - Warns when Result types are returned but not handled
- **Double wrapping (TRLS008)** - Detects `Result<Result<T>>` patterns (usually indicates Map should be Bind)
- **Wrong method usage (TRLS002)** - Suggests using `Bind` instead of `Map` when lambda returns Result
- **Async misuse (TRLS009)** - Prevents blocking on `Task<Result<T>>` with `.Result` or `.Wait()`
- **Async lambda misuse (TRLS014)** - Detects async lambda used with sync method (Map instead of MapAsync)
- **TryCreate().Value anti-pattern (TRLS007)** - Suggests using `.Create()` for clearer errors
- **Manual result combination (TRLS012)** - Suggests using `Result.Combine()` for multiple validations
- **Ternary operator patterns (TRLS013)** - Suggests using `GetValueOrDefault()` or `Match()`
- **Throwing in Result chains (TRLS015)** - Detects `throw` statements that defeat ROP semantics
- **Null comparisons (TRLS017)** - Detects comparing Result/Maybe to null (they're structs)
- **Unsafe LINQ access (TRLS018)** - Detects `.Value` in LINQ without filtering by IsSuccess/HasValue
- **Combine chain too long (TRLS019)** - Detects Combine chains exceeding 9-element tuple limit

### Entity Framework Core Integration

- **Use SaveChangesResult (TRLS020)** - Warns when `SaveChanges`/`SaveChangesAsync` is called directly instead of `SaveChangesResultUnitAsync` or `SaveChangesResultAsync`
- **HasIndex with Maybe\<T\> (TRLS021)** - Warns when `HasIndex` lambda references a `Maybe<T>` property because Trellis EF Core maps `Maybe<T>` via nullable backing fields; prefer `HasTrellisIndex` or use the backing field name directly

### Error Handling Best Practices

- **Specific error types (TRLS010)** - Encourages using `Error.Validation()`, `Error.NotFound()` etc. instead of base `Error` class
- **Error discrimination (TRLS005)** - Suggests using `MatchError` for type-safe error handling
- **Empty error messages (TRLS016)** - Detects empty or missing error messages

### Maybe Pattern Enforcement

Similar protections for `Maybe<T>`:

- **Unsafe access (TRLS006)** - Ensures proper checking before accessing `Maybe<T>.Value`
- **Double wrapping (TRLS011)** - Detects `Maybe<Maybe<T>>` patterns

## Example Diagnostics

### TRLS003: Unsafe Result.Value Access

**Problem:** Accessing `.Value` without checking success state can throw exceptions.


```csharp
// Warning: Accessing Value without checking IsSuccess
var user = GetUser(id);
Console.WriteLine(user.Value.Name);  // TRLS003: May throw InvalidOperationException

// Option 1: Check before access
var user = GetUser(id);
if (user.IsSuccess)
    Console.WriteLine(user.Value.Name);

// Option 2: Use TryGetValue pattern
if (GetUser(id).TryGetValue(out var user))
    Console.WriteLine(user.Name);

// Option 3: Use Match (recommended)
GetUser(id).Match(
    onSuccess: u => Console.WriteLine(u.Name),
    onFailure: err => Console.WriteLine($"Error: {err.Detail}")
);
```

### TRLS002: Use Bind Instead of Map

**Problem:** Using `Map` when the lambda returns a `Result` creates double-wrapped `Result<Result<T>>`.

```csharp
// Creates Result<Result<Order>> 
var result = userId.Map(id => GetOrder(id));  // TRLS002

// Use Bind for flattening
var result = userId.Bind(id => GetOrder(id));  // Result<Order>
```

### TRLS007: TryCreate().Value Anti-Pattern

**Problem:** `TryCreate().Value` provides poor error messages when validation fails.

```csharp
// Unclear error message on failure
var email = EmailAddress.TryCreate(input).Value;  // TRLS007

// Use Create() for expected-valid values (better error message)
var email = EmailAddress.Create(input);

// Or handle the Result properly
var emailResult = EmailAddress.TryCreate(input);
if (emailResult.IsFailure)
    return BadRequest(emailResult.Error);
var email = emailResult.Value;
```

### TRLS008: Double-Wrapped Result

**Problem:** `Result<Result<T>>` is almost always unintended.

```csharp
// Creates Result<Result<User>>
Result<Result<User>> wrapped = Result.Success(GetUser(id));  // TRLS008

// Use Bind to flatten
var user = someResult.Bind(id => GetUser(id));  // Result<User>
```

### TRLS009: Blocking on Async Results

**Problem:** Blocking on `Task<Result<T>>` can cause deadlocks.

```csharp
// Blocking call - can deadlock
var user = GetUserAsync(id).Result;  // TRLS009

// Use await
var user = await GetUserAsync(id);
```

### TRLS012: Use Result.Combine

**Problem:** Manual result checking is verbose and error-prone.

```csharp
// Manual checking (stops at first error)
var emailResult = EmailAddress.TryCreate(input.Email);
if (emailResult.IsFailure) return emailResult.Error;
var nameResult = FirstName.TryCreate(input.Name);
if (nameResult.IsFailure) return nameResult.Error;

// Use Result.Combine to collect ALL errors
var result = Result.Combine(
        EmailAddress.TryCreate(input.Email),
        FirstName.TryCreate(input.Name))
    .Bind((email, name) => User.Create(email, name));
// Returns all validation errors at once
```

## Configuration

The analyzers run automatically. To suppress specific warnings, use standard .NET mechanisms:

**In code:**
```csharp
#pragma warning disable TRLS003  // Suppress unsafe value access warning
var value = result.Value;
#pragma warning restore TRLS003
```

**In .editorconfig:**
```ini
[*.cs]
# Change severity of specific rules
dotnet_diagnostic.TRLS003.severity = suggestion  # Downgrade to suggestion
dotnet_diagnostic.TRLS002.severity = none        # Disable completely
dotnet_diagnostic.TRLS007.severity = warning     # Upgrade to warning
```

**Suppress all Trellis analyzers:**
```xml
<!-- In .csproj -->
<PropertyGroup>
  <NoWarn>$(NoWarn);TRLS001;TRLS002;TRLS003;TRLS004;TRLS005;TRLS006;TRLS007;TRLS008;TRLS009;TRLS010;TRLS011;TRLS012;TRLS013;TRLS014;TRLS015;TRLS016;TRLS017;TRLS018;TRLS019;TRLS020;TRLS021</NoWarn>
</PropertyGroup>
```

## Analyzer Rules

| Rule ID | Title | Severity |
|---------|-------|----------|
| TRLS001 | Result return value is not handled | Warning |
| TRLS002 | Use Bind instead of Map when lambda returns Result | Info |
| TRLS003 | Unsafe access to Result.Value | Warning |
| TRLS004 | Unsafe access to Result.Error | Warning |
| TRLS005 | Consider using MatchError for error type discrimination | Info |
| TRLS006 | Unsafe access to Maybe.Value | Warning |
| TRLS007 | Use Create instead of TryCreate().Value | Warning |
| TRLS008 | Result is double-wrapped (Result&lt;Result&lt;T&gt;&gt;) | Warning |
| TRLS009 | Incorrect async Result usage (blocking instead of awaiting) | Warning |
| TRLS010 | Use specific error type instead of base Error class | Info |
| TRLS011 | Maybe is double-wrapped (Maybe&lt;Maybe&lt;T&gt;&gt;) | Warning |
| TRLS012 | Consider using Result.Combine | Info |
| TRLS013 | Consider using GetValueOrDefault or Match instead of ternary | Info |
| TRLS014 | Use async method variant for async lambda | Warning |
| TRLS015 | Don't throw exceptions in Result chains | Warning |
| TRLS016 | Error message should not be empty | Warning |
| TRLS017 | Don't compare Result or Maybe to null | Warning |
| TRLS018 | Unsafe access to Value in LINQ expression | Warning |
| TRLS019 | Combine chain exceeds maximum tuple size | Error |
| TRLS020 | Use SaveChangesResultAsync instead of SaveChangesAsync | Warning |
| TRLS021 | HasIndex references a Maybe&lt;T&gt; property | Warning |

## Related Packages

- [Trellis.Results](https://www.nuget.org/packages/Trellis.Results) — Core `Result<T>` and `Maybe<T>` types
- [Trellis.Asp](https://www.nuget.org/packages/Trellis.Asp) — ASP.NET Core integration

## License

MIT — see [LICENSE](../LICENSE) for details.
