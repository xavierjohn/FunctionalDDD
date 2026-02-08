# GitHub Copilot Instructions for FunctionalDdd

## Project Overview

Functional programming library for .NET 10 implementing Railway Oriented Programming (ROP), Domain-Driven Design (DDD) primitives, and value objects.

## Naming Conventions

This project uses `FunctionalDdd` (lowercase 'dd') per Microsoft's .NET naming guidelines for 3+ letter acronyms.

| Context | Correct | Incorrect |
|---------|---------|-----------|
| Packages / Namespaces | `FunctionalDdd.RailwayOrientedProgramming` | `FunctionalDDD.RailwayOrientedProgramming` |
| Using statements | `using FunctionalDdd;` | `using FunctionalDDD;` |
| Assembly names | `FunctionalDdd.*` | `FunctionalDDD.*` |

"FunctionalDDD" or "Functional DDD" is acceptable in prose, documentation, and the GitHub repository name.

## Value Object Creation Patterns

All value objects provide two factory methods:

| Method | Returns | Use When |
|--------|---------|----------|
| `TryCreate` | `Result<T>` | Failure is expected (API input, user validation) |
| `Create` | `T` (throws on failure) | Values are known-valid (tests, constants, config) |

```csharp
//  TryCreate — handle errors gracefully
var result = Money.TryCreate(amount, currencyCode);
if (result.IsFailure)
    return result.Error;

//  Create — failure is exceptional
var testMoney = Money.Create(100.00m, "USD");

//  Don't use .Value on TryCreate in production
var money = Money.TryCreate(amount, currency).Value;
```

### Implementation Details

- All scalar value objects inherit `Create(T value)` from `ScalarValueObject<TSelf, T>`, which calls `TryCreate` and throws `InvalidOperationException` on failure
- Source-generated types (`RequiredGuid`, `RequiredString`, `RequiredInt`, `RequiredDecimal`) auto-generate `Create()` overloads mirroring each `TryCreate()` overload
- The FDDD007 analyzer suggests `Create()` instead of `TryCreate().Value`
- Override `Create` for multi-parameter signatures (e.g., `Money.Create(amount, currency)`)

**Generated Create overloads:**

| Type | Overloads |
|------|-----------|
| `RequiredGuid` | `Create(Guid)`, `Create(string)`, `NewUniqueV4()`, `NewUniqueV7()` |
| `RequiredString` | `Create(string?, string? fieldName)` |
| `RequiredInt` / `RequiredDecimal` | `Create(int/decimal)`, `Create(string)` |

**Custom value objects** inherit `Create` automatically:

```csharp
public class Temperature : ScalarValueObject<Temperature, decimal>,
    IScalarValue<Temperature, decimal>
{
    private Temperature(decimal value) : base(value) { }

    public static Result<Temperature> TryCreate(decimal value, string? fieldName = null) =>
        value.ToResult()
            .Ensure(v => v >= -273.15m, Error.Validation("Below absolute zero", fieldName ?? "temperature"))
            .Map(v => new Temperature(v));

    // Create is inherited from base class — no need to implement!
}
```

**Multi-parameter Create** — override explicitly when TryCreate has multiple required parameters:

```csharp
public static Money Create(decimal amount, string currencyCode)
{
    var result = TryCreate(amount, currencyCode);
    if (result.IsFailure)
        throw new InvalidOperationException($"Failed to create Money: {result.Error.Detail}");
    return result.Value;
}
```

## Code Style

### General Rules

- Omit braces for single-line `if`/`return` statements
- Use `char` overloads for single-character `Contains()` (CA1847): `value.Contains('-')` not `value.Contains("-")`
- Use collection expressions for FluentAssertions: `.Should().Equal([1, 2, 3])`
- Use `ConfigureAwait(false)` in library code (source files), never in test code
- Prefer `ValueTask<T>` for high-frequency, potentially synchronous operations; `Task<T>` for I/O-bound
- Avoid allocations in hot paths; consider `readonly struct` for value types

### Avoid Task/ValueTask Ambiguities

When both `Task<T>` and `ValueTask<T>` overloads exist, use explicit constructors:

```csharp
// ❌ Ambiguous
.EnsureAsync(v => Task.FromResult(v > 0), error)

// ✅ Explicit
.EnsureAsync(v => new ValueTask<bool>(v > 0), error)
```

## Railway Oriented Programming (ROP)

### Core Types

- **`Result<TValue>`**: Success (with value) or failure (with error)
- **`Maybe<T>`**: Domain-level optionality (`where T : notnull`). Supports `Map<TResult>`, `Match<TResult>`, `GetValueOrDefault`, `TryGetValue`, implicit operator. Use `Maybe<T>` instead of `T?` for optional value objects in DTOs (e.g., `Maybe<Url> Website`). ASP.NET Core integration: `MaybeScalarValueJsonConverter`, `MaybeModelBinder`, `MaybeSuppressChildValidationMetadataProvider` — all registered automatically by `AddScalarValueValidation()`.
- **`Error`**: Base error type (Validation, NotFound, Unauthorized, etc.)

### Key Methods

| Method | Purpose |
|--------|---------|
| **Bind** | Transform value inside Result (flatMap) |
| **Map** | Transform value, preserve Result wrapper |
| **Ensure** | Add validation to a Result |
| **Tap** | Side effects without changing Result |
| **Match** | Pattern match on success/failure |
| **Combine** | Combine multiple Results |

### Testing Philosophy

1. **Railway track behavior**: Once on failure track, stay there
2. **Early exit**: Don't execute functions if already failed
3. **Value preservation**: Original values preserved through transformations
4. **Error propagation**: Errors flow through pipeline unchanged

## Test Organization

### Async Extension File Naming

Tests are organized by which parts are async:

- **Left** = input/source (`Task<Result<T>>`, `ValueTask<Result<T>>`)
- **Right** = predicates/functions passed as parameters

| Pattern | File Name | Input | Predicates |
|---------|-----------|-------|------------|
| Both async | `[Method]Tests.[Type].cs` | async | async |
| Left only | `[Method]Tests.[Type].Left.cs` | async | sync |
| Right only | `[Method]Tests.[Type].Right.cs` | sync | async |

Applies to: Ensure, Bind, Map, Tap, Match, Combine, and all other async extensions.

```csharp
// Both async → EnsureTests.ValueTask.cs
await ValueTask.FromResult(Result.Success("test"))
    .EnsureAsync(v => ValueTask.FromResult(v.Length > 0), Error.Validation("Empty"));

// Left only → EnsureTests.ValueTask.Left.cs
await ValueTask.FromResult(Result.Success("test"))
    .EnsureAsync(v => v.Length > 0, Error.Validation("Empty"));

// Right only → EnsureTests.ValueTask.Right.cs
await Result.Success("test")
    .EnsureAsync(v => ValueTask.FromResult(v.Length > 0), Error.Validation("Empty"));
```

**Quick decision tree:** Is the input async? → Left. Are predicates async? → Right. Both? → Base (no suffix).

### Test Class Structure

- **One variant per file** — don't mix Left/Right/Both in the same file
- Organize with `#region` blocks by overload variant
- Place helper types (records, classes) at the bottom

```csharp
/// <summary>
/// Tests for Ensure.ValueTask.cs where BOTH input and predicates are async.
/// </summary>
public class Ensure_ValueTask_Tests
{
    #region EnsureAsync with ValueTask<bool> predicate and static Error
    // Tests...
    #endregion

    #region Edge Cases and Integration Tests
    // Tests...
    #endregion

    private record TestData(string Name, int Value);
}
```

### Test Method Naming

Format: `[Method]_[Variant]_[Scenario]_[Expectation]`

Example: `EnsureAsync_ValueTask_Bool_StaticError_SuccessResult_PredicateTrue_ReturnsSuccess`

### Required Test Coverage

- Success path + valid predicate → returns success
- Success path + failing predicate → returns failure
- Failure path → predicate not invoked, original failure returned
- Error factories (sync and async where applicable)
- Result-returning predicates (where applicable)
- Edge cases: nullable types, complex types, empty/null values
- Chained operations, early exit verification, exception propagation

```csharp
// ✅ Always verify early exit
var predicateInvoked = false;
var result = await Result.Failure<int>(error)
    .EnsureAsync(v => { predicateInvoked = true; return v > 0; }, error);

predicateInvoked.Should().BeFalse("predicate should not be invoked for failed results");
```

## File Location Guidelines

| Area | Source | Tests |
|------|--------|-------|
| Core ROP | `RailwayOrientedProgramming/src/Result/Extensions/` | `RailwayOrientedProgramming/tests/Results/Extensions/` |
| Value Objects | `PrimitiveValueObjects/src/` | `PrimitiveValueObjects/tests/` |
| DDD | `DomainDrivenDesign/src/` | `DomainDrivenDesign/tests/` |
| ASP.NET | `Asp/src/` | `Asp/tests/` |
| HTTP | `Http/src/` | `Http/tests/` |

## Documentation Standards

- All public APIs must have XML doc comments (`<summary>`, `<param>`, `<returns>`)
- Test classes should have `<summary>` explaining what source file and async variant they cover

```csharp
/// <summary>
/// Returns a new failure result if the predicate is false.
/// </summary>
/// <param name="result">The source result.</param>
/// <param name="predicate">The predicate to evaluate.</param>
/// <param name="error">The error to return if the predicate fails.</param>
/// <returns>The original result if successful and predicate passes; otherwise a failure.</returns>
public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(
    this ValueTask<Result<TOk>> result,
    Func<TOk, ValueTask<bool>> predicate,
    Error error)
```

## T4 Template Testing Strategy

T4 templates generate 2-tuple through 9-tuple overloads with identical logic. **Test the 2-tuple comprehensively; validate other sizes with minimal tests.**

### T4-Generated Files

| Template | Generated Source | Purpose |
|----------|-----------------|---------|
| `TapTs.g.tt` | `TapTs.g.cs` | Tap for tuple Results |
| `TapOnFailureTs.g.tt` | `TapOnFailureTs.g.cs` | TapOnFailure for tuple Results |
| `BindTs.g.tt` | `BindTs.g.cs` | Bind for tuple Results |
| `MatchTupleTs.g.tt` | `MatchTupleTs.g.cs` | Match for tuple Results |
| `CombineTs.g.tt` | `CombineTs.g.cs` | Combine for multiple Results |
| `MapTs.g.tt` | `MapTs.g.cs` | Map for tuple Results |
| `WhenAllTs.g.tt` | `WhenAllTs.g.cs` | WhenAll for parallel Results |
| `ParallelAsyncs.g.tt` | `ParallelAsyncs.g.cs` | Parallel async operations |

### Test File Naming for T4 Code

| Source File | Test File |
|-------------|-----------|
| `TapTs.g.cs` | `TapTupleTests.cs` |
| `TapOnFailureTs.g.cs` | `TapOnFailureTupleTests.cs` |
| `BindTs.g.cs` | `BindTsTests.cs` |
| `MapTs.g.cs` | `MapTsTests.cs` |
| `MatchTupleTs.g.cs` | (tracing tests) |
| `ParallelAsyncs.g.cs` | `ParallelAsyncTests.cs` |

### What to Test

| Scope | Coverage |
|-------|----------|
| **2-tuple** | Comprehensive: success/failure paths, destructuring, chaining, async variants, different types, real-world scenarios |
| **3-tuple, 9-tuple** | Validation only: one success test, one failure test for largest size |
| **Other sizes** | None — template guarantees consistency |

**Expected coverage: ~12–35%.** This is intentional. Don't aim for 100% on T4-generated code.

### Modifying T4 Templates

1. Run the template to regenerate `.g.cs`
2. Update 2-tuple tests if the pattern changed
3. Verify one larger tuple still works (5-tuple or 9-tuple)

## Activity Tracing and OpenTelemetry

### Core Rules

| Rule | Correct Pattern | Why |
|------|----------------|-----|
| Setting activity status | `activity?.SetStatus(...)` (local variable) | `Activity.Current` has race conditions in concurrent scenarios |
| Test isolation | `AsyncLocal<ActivitySource?>` with inject/reset | Per-context isolation, parallel-safe, no `[Collection]` needed |
| Test helpers | Unique `ActivitySource` per test + `ActivityListener` | Isolated activity capture per test instance |

### Activity Status: TryCreate vs ROP Methods

| Context | Manual status needed? | Reason |
|---------|----------------------|--------|
| Value object `TryCreate` | **No** | Activity is root → becomes `Activity.Current` → Result constructor sets it automatically |
| ROP extensions (Bind, Tap, Map) | **Yes** — call `result.LogActivityStatus()` | Creates child activity ≠ `Activity.Current`; Result constructor sets parent, not child |

The `Result<T>` constructor automatically sets `Activity.Current` status:

```csharp
internal Result(bool isFailure, TValue? ok, Error? error)
{
    // ... validation ...
    Activity.Current?.SetStatus(IsFailure ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
    if (IsFailure && Activity.Current is { } act && error is not null)
        act.SetTag("result.error.code", error.Code);
}
```

**TryCreate** — no manual status needed (activity IS `Activity.Current`):

```csharp
public static Result<EmailAddress> TryCreate(string? value, string? fieldName = null)
{
    using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity("EmailAddress.TryCreate");
    if (value is not null && EmailRegEx().IsMatch(value))
        return new EmailAddress(value);  // Result constructor sets Activity.Current (== activity)
    return Result.Failure<EmailAddress>(Error.Validation("Email address is not valid.", field));
}
```

**ROP extensions** — must explicitly set child activity status:

```csharp
public static Result<TValue> Tap<TValue>(this Result<TValue> result, Action<TValue> action)
{
    using var activity = RopTrace.ActivitySource.StartActivity();  // Child activity
    if (result.IsSuccess)
        action(result.Value);
    result.LogActivityStatus();  // ✅ Must set explicitly — child ≠ Activity.Current
    return result;
}
```

### Test Isolation Pattern

Use `AsyncLocal<ActivitySource?>` for parallel-safe test isolation without `[Collection]` attributes:

```csharp
public static class PrimitiveValueObjectTrace
{
    private static readonly ActivitySource _defaultActivitySource = new("Functional DDD PVO", "1.0.0");
    private static readonly AsyncLocal<ActivitySource?> _testActivitySource = new();

    public static ActivitySource ActivitySource => _testActivitySource.Value ?? _defaultActivitySource;
    internal static void SetTestActivitySource(ActivitySource s) => _testActivitySource.Value = s;
    internal static void ResetTestActivitySource() => _testActivitySource.Value = null;
}
```

Test helper pattern: create a unique `ActivitySource` per test instance, inject via `SetTestActivitySource`, capture activities via `ActivityListener`, and reset in `Dispose()`. See `PvoActivityTestHelper` for the full implementation.

## File Encoding & PowerShell

All files must be **UTF-8 with BOM**.

```powershell
# ✅ Correct — preserves all characters
$utf8Bom = New-Object System.Text.UTF8Encoding $true
[System.IO.File]::WriteAllText($path, $content, $utf8Bom)

# ❌ NEVER use Set-Content — corrupts emoji, arrows, special symbols
Set-Content $path -Value $content -NoNewline
```

When running PowerShell commands in the terminal:
- Avoid long or complex scripts — they tend to get stuck or timeout
- Use smaller, targeted file edits with the `replace_string_in_file` tool instead of large PowerShell scripts for file manipulation
