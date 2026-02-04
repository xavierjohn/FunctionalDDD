# FunctionalDdd.Analyzers

Roslyn analyzers that enforce proper usage of Result and Maybe patterns at compile time.

## Overview

This package provides compile-time analysis to help you write safer, more maintainable code with FunctionalDdd's Railway Oriented Programming patterns.

## Installation

```bash
dotnet add package FunctionalDdd.Analyzers
```

The analyzers are automatically integrated into your build process and will provide warnings/errors in Visual Studio, VS Code, and during `dotnet build`.

## What It Analyzes

### Result Pattern Enforcement

The analyzers help prevent common mistakes when working with `Result<T>`:

- **Unsafe value access (FDDD003, FDDD004)** - Ensures you check `IsSuccess`/`IsFailure` before accessing `Value`/`Error`
- **Unhandled results (FDDD001)** - Warns when Result types are returned but not handled
- **Double wrapping (FDDD008)** - Detects `Result<Result<T>>` patterns (usually indicates Map should be Bind)
- **Wrong method usage (FDDD002)** - Suggests using `Bind` instead of `Map` when lambda returns Result
- **Async misuse (FDDD009)** - Prevents blocking on `Task<Result<T>>` with `.Result` or `.Wait()`
- **Async lambda misuse (FDDD014)** - Detects async lambda used with sync method (Map instead of MapAsync)
- **TryCreate().Value anti-pattern (FDDD007)** - Suggests using `.Create()` for clearer errors
- **Manual result combination (FDDD012)** - Suggests using `Result.Combine()` for multiple validations
- **Ternary operator patterns (FDDD013)** - Suggests using `GetValueOrDefault()` or `Match()`

### Error Handling Best Practices

- **Specific error types (FDDD010)** - Encourages using `Error.Validation()`, `Error.NotFound()` etc. instead of base `Error` class
- **Error discrimination (FDDD005)** - Suggests using `MatchError` for type-safe error handling

### Maybe Pattern Enforcement

Similar protections for `Maybe<T>`:

- **Unsafe access (FDDD006)** - Ensures proper checking before accessing `Maybe<T>.Value`
- **Double wrapping (FDDD011)** - Detects `Maybe<Maybe<T>>` patterns

## Example Diagnostics

### FDDD003: Unsafe Result.Value Access

**Problem:** Accessing `.Value` without checking success state can throw exceptions.

```csharp
// ❌ Warning: Accessing Value without checking IsSuccess
var user = GetUser(id);
Console.WriteLine(user.Value.Name);  // FDDD003: May throw InvalidOperationException

// ✅ Option 1: Check before access
var user = GetUser(id);
if (user.IsSuccess)
    Console.WriteLine(user.Value.Name);

// ✅ Option 2: Use TryGetValue pattern
if (GetUser(id).TryGetValue(out var user))
    Console.WriteLine(user.Name);

// ✅ Option 3: Use Match (recommended)
GetUser(id).Match(
    onSuccess: u => Console.WriteLine(u.Name),
    onFailure: err => Console.WriteLine($"Error: {err.Detail}")
);
```

### FDDD002: Use Bind Instead of Map

**Problem:** Using `Map` when the lambda returns a `Result` creates double-wrapped `Result<Result<T>>`.

```csharp
// ❌ Creates Result<Result<Order>> 
var result = userId.Map(id => GetOrder(id));  // FDDD002

// ✅ Use Bind for flattening
var result = userId.Bind(id => GetOrder(id));  // Result<Order>
```

### FDDD007: TryCreate().Value Anti-Pattern

**Problem:** `TryCreate().Value` provides poor error messages when validation fails.

```csharp
// ❌ Unclear error message on failure
var email = EmailAddress.TryCreate(input).Value;  // FDDD007

// ✅ Use Create() for expected-valid values (better error message)
var email = EmailAddress.Create(input);

// ✅ Or handle the Result properly
var emailResult = EmailAddress.TryCreate(input);
if (emailResult.IsFailure)
    return BadRequest(emailResult.Error);
var email = emailResult.Value;
```

### FDDD008: Double-Wrapped Result

**Problem:** `Result<Result<T>>` is almost always unintended.

```csharp
// ❌ Creates Result<Result<User>>
Result<Result<User>> wrapped = Result.Success(GetUser(id));  // FDDD008

// ✅ Use Bind to flatten
var user = someResult.Bind(id => GetUser(id));  // Result<User>
```

### FDDD009: Blocking on Async Results

**Problem:** Blocking on `Task<Result<T>>` can cause deadlocks.

```csharp
// ❌ Blocking call - can deadlock
var user = GetUserAsync(id).Result;  // FDDD009

// ✅ Use await
var user = await GetUserAsync(id);
```

### FDDD012: Use Result.Combine

**Problem:** Manual result checking is verbose and error-prone.

```csharp
// ❌ Manual checking (stops at first error)
var emailResult = EmailAddress.TryCreate(input.Email);
if (emailResult.IsFailure) return emailResult.Error;
var nameResult = FirstName.TryCreate(input.Name);
if (nameResult.IsFailure) return nameResult.Error;

// ✅ Use Combine to collect ALL errors
var result = EmailAddress.TryCreate(input.Email)
    .Combine(FirstName.TryCreate(input.Name))
    .Bind((email, name) => User.Create(email, name));
// Returns all validation errors at once
```

## Configuration

The analyzers run automatically. To suppress specific warnings, use standard .NET mechanisms:

**In code:**
```csharp
#pragma warning disable FDDD003  // Suppress unsafe value access warning
var value = result.Value;
#pragma warning restore FDDD003
```

**In .editorconfig:**
```ini
[*.cs]
# Change severity of specific rules
dotnet_diagnostic.FDDD003.severity = suggestion  # Downgrade to suggestion
dotnet_diagnostic.FDDD002.severity = none        # Disable completely
dotnet_diagnostic.FDDD007.severity = warning     # Upgrade to warning
```

**Suppress all FunctionalDDD analyzers:**
```xml
<!-- In .csproj -->
<PropertyGroup>
  <NoWarn>$(NoWarn);FDDD001;FDDD002;FDDD003;FDDD004;FDDD005;FDDD006;FDDD007;FDDD008;FDDD009;FDDD010;FDDD011;FDDD012;FDDD013;FDDD014</NoWarn>
</PropertyGroup>
```

## Analyzer Rules

| Rule ID | Title | Severity |
|---------|-------|----------|
| FDDD001 | Result return value is not handled | Warning |
| FDDD002 | Use Bind instead of Map when lambda returns Result | Info |
| FDDD003 | Unsafe access to Result.Value | Warning |
| FDDD004 | Unsafe access to Result.Error | Warning |
| FDDD005 | Consider using MatchError for error type discrimination | Info |
| FDDD006 | Unsafe access to Maybe.Value | Warning |
| FDDD007 | Use Create instead of TryCreate().Value | Warning |
| FDDD008 | Result is double-wrapped (Result&lt;Result&lt;T&gt;&gt;) | Warning |
| FDDD009 | Incorrect async Result usage (blocking instead of awaiting) | Warning |
| FDDD010 | Use specific error type instead of base Error class | Info |
| FDDD011 | Maybe is double-wrapped (Maybe&lt;Maybe&lt;T&gt;&gt;) | Warning |
| FDDD012 | Consider using Result.Combine | Info |
| FDDD013 | Consider using GetValueOrDefault or Match instead of ternary | Info |
| FDDD014 | Use async method variant for async lambda | Warning |

## Requirements

- .NET Standard 2.0 or later
- Roslyn 4.0+ (Visual Studio 2022 or later, or .NET 6+ SDK)

## Feedback

Found a false positive or have suggestions? [Open an issue on GitHub](https://github.com/xavierjohn/FunctionalDDD/issues).

## Related Packages


- [FunctionalDdd.RailwayOrientedProgramming](https://www.nuget.org/packages/FunctionalDdd.RailwayOrientedProgramming) - Core Result/Maybe types
- [FunctionalDdd.Asp](https://www.nuget.org/packages/FunctionalDdd.Asp) - ASP.NET Core integration
