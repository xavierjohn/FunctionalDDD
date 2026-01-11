# GitHub Copilot Instructions for FunctionalDDD

## Project Overview

This is a functional programming library for .NET that implements Railway Oriented Programming (ROP) patterns, Domain-Driven Design (DDD) primitives, and value objects.

**Target Frameworks:**
- .NET 10
- .NET Standard 2.0

## Test File Organization Rules

### Async Extension Test Organization

For async extension methods (Ensure, Bind, Map, Tap, etc.), tests are organized based on which parts are async:

**Terminology:**
- **Left** = The input/source (e.g., `Task<Result<T>>`, `ValueTask<Result<T>>`, or `Result<T>`)
- **Right** = The predicates/functions passed as parameters (e.g., `Func<Task<bool>>`, `Func<ValueTask<Result>>`, etc.)

### File Naming Convention

```
[MethodName]Tests.[AsyncType].cs        - Both left AND right are async
[MethodName]Tests.[AsyncType].Left.cs   - Only left is async (input)
[MethodName]Tests.[AsyncType].Right.cs  - Only right is async (predicates/functions)
```

**Examples:**
- `EnsureTests.Task.cs` - Both input and predicates use Task
- `EnsureTests.Task.Left.cs` - Input is Task, predicates are sync
- `EnsureTests.Task.Right.cs` - Input is sync, predicates use Task
- `EnsureTests.ValueTask.cs` - Both input and predicates use ValueTask
- `EnsureTests.ValueTask.Left.cs` - Input is ValueTask, predicates are sync
- `EnsureTests.ValueTask.Right.cs` - Input is sync, predicates use ValueTask

### Organization Rules by Pattern

#### 1. **Both Async** (e.g., `EnsureTests.ValueTask.cs`)
- **Input:** `ValueTask<Result<TOk>>`
- **Predicates:** `ValueTask<bool>`, `Func<ValueTask<Result<>>>`, etc.
- **Source file:** `Ensure.ValueTask.cs`
- **When to use:** When the extension extends an async input with async predicates

```csharp
// Example: Goes in EnsureTests.ValueTask.cs
var result = await ValueTask.FromResult(Result.Success("test"))
    .EnsureAsync(
        value => ValueTask.FromResult(value.Length > 0),  // Right is async
        Error.Validation("Empty"));
```

#### 2. **Left Async Only** (e.g., `EnsureTests.ValueTask.Left.cs`)
- **Input:** `ValueTask<Result<TOk>>`
- **Predicates:** Sync `bool`, sync `Result<>`, etc.
- **Source file:** `Ensure.ValueTask.Left.cs`
- **When to use:** When the extension extends an async input with sync predicates

```csharp
// Example: Goes in EnsureTests.ValueTask.Left.cs
var result = await ValueTask.FromResult(Result.Success("test"))
    .EnsureAsync(
        value => value.Length > 0,  // Right is sync
        Error.Validation("Empty"));
```

#### 3. **Right Async Only** (e.g., `EnsureTests.ValueTask.Right.cs`)
- **Input:** `Result<TOk>` (sync)
- **Predicates:** `ValueTask<bool>`, `Func<ValueTask<Result<>>>`, etc.
- **Source file:** `Ensure.ValueTask.Right.cs`
- **When to use:** When the extension extends a sync input with async predicates

```csharp
// Example: Goes in EnsureTests.ValueTask.Right.cs
var result = await Result.Success("test")
    .EnsureAsync(
        value => ValueTask.FromResult(value.Length > 0),  // Right is async
        Error.Validation("Empty"));
```

### Pattern Applies To All Async Extensions

This organizational pattern applies to all async extension methods:

- ✅ **Ensure** - Validation extensions
- ✅ **Bind** - Transformation/binding extensions
- ✅ **Map** - Mapping extensions
- ✅ **Tap** - Side-effect extensions
- ✅ **Match** - Pattern matching extensions
- ✅ **Combine** - Combination extensions
- ✅ And any other async extension methods

**Examples:**
```
BindTests.Task.cs, BindTests.Task.Left.cs, BindTests.Task.Right.cs
MapTests.Task.cs, MapTests.Task.Left.cs, MapTests.Task.Right.cs
TapTests.ValueTask.cs, TapTests.ValueTask.Left.cs, TapTests.ValueTask.Right.cs
```

## Test Coverage Requirements

All async extension test files should include comprehensive coverage:

### Required Test Scenarios

- ✅ **Success path + valid predicate** → returns success
- ✅ **Success path + failing predicate** → returns failure
- ✅ **Failure path** → predicate/function not invoked, returns original failure
- ✅ **Error factories** (sync and async where applicable)
- ✅ **Result-returning predicates** (where applicable)
- ✅ **Edge cases:**
  - Nullable types
  - Complex types (records, classes)
  - Empty/null values
- ✅ **Chained operations** - Multiple extensions in sequence
- ✅ **Early exit behavior** - Verify functions aren't called on failure
- ✅ **Exception handling** - Verify exceptions propagate correctly
- ✅ **Integration scenarios** - Mixing different extension types

### Test Method Naming Convention

```
[MethodName]_[Variant]_[Scenario]_[Expectation]
```

**Examples:**
```csharp
EnsureAsync_ValueTask_Bool_StaticError_SuccessResult_PredicateTrue_ReturnsSuccess
EnsureAsync_Right_Result_WithParam_FailureResult_PredicateNotInvoked_ReturnsOriginalFailure
BindAsync_Task_Left_SuccessResult_FunctionReturnsSuccess_ReturnsNewValue
MapAsync_ValueTask_Right_FailureResult_MapperNotInvoked_ReturnsOriginalError
```

**Components:**
1. **MethodName**: `EnsureAsync`, `BindAsync`, `MapAsync`, etc.
2. **Variant**: `ValueTask`, `Task`, `ValueTask_Left`, `Task_Right`, etc.
3. **Scenario**: What's being tested (e.g., `Bool_StaticError`, `Result_WithParam`)
4. **Expectation**: Expected outcome (e.g., `ReturnsSuccess`, `PredicateNotInvoked`)

## Code Style Guidelines

### Avoid Task/ValueTask Ambiguities

When both `Task<T>` and `ValueTask<T>` overloads exist, explicitly use constructors:

```csharp
// ❌ Ambiguous
await result.EnsureAsync(
    value => Task.FromResult(value.Length > 0),  // Could be Task or ValueTask
    Error.Validation("Empty"));

// ✅ Explicit
await result.EnsureAsync(
    value => new ValueTask<bool>(value.Length > 0),
    Error.Validation("Empty"));
```

### Code Analysis Compliance

- ✅ **CA1847**: Use `char` overloads for `Contains()` when searching single characters

```csharp
// ❌ CA1847 violation
value.Contains("-")

// ✅ Correct
value.Contains('-')
```

- ✅ **Collection Expressions**: Use collection expressions for FluentAssertions

```csharp
// ❌ Old style
executionOrder.Should().Equal(1, 2, 3);

// ✅ Collection expression
executionOrder.Should().Equal([1, 2, 3]);
```

### Async/Await Best Practices

- ✅ Always use `ConfigureAwait(false)` in library code (source files)
- ✅ Don't use `ConfigureAwait` in test code
- ✅ Use `ValueTask<T>` for high-performance scenarios
- ✅ Use `Task<T>` for general async operations

### Test Organization

Each test file should be organized with regions:

```csharp
public class Ensure_ValueTask_Tests
{
    #region EnsureAsync with ValueTask<bool> predicate and static Error
    // Tests here...
    #endregion

    #region EnsureAsync with ValueTask<bool> predicate and Error factory
    // Tests here...
    #endregion

    #region Edge Cases and Integration Tests
    // Tests here...
    #endregion

    private record TestData(string Name, int Value);  // Helper types at bottom
}
```

## Railway Oriented Programming (ROP) Patterns

### Core Concepts

- **Result<TValue>**: Represents either success (with a value) or failure (with an error)
- **Maybe<T>**: Represents an optional value that may or may not exist
- **Error**: Base type for all errors (Validation, NotFound, Unauthorized, etc.)

### Key Methods

- **Bind**: Transform the value inside a Result (flatMap)
- **Map**: Transform the value while preserving the Result wrapper
- **Ensure**: Add validation to a Result
- **Tap**: Perform side effects without changing the Result
- **Match**: Pattern match on success/failure
- **Combine**: Combine multiple Results

### Testing Philosophy

Tests should verify:
1. **Railway track behavior**: Once on the failure track, stay there
2. **Early exit**: Don't execute functions if already failed
3. **Value preservation**: Original values preserved through transformations
4. **Error propagation**: Errors flow through the pipeline unchanged

## Common Pitfalls to Avoid

### ❌ Don't Test Both Variants in One File

```csharp
// ❌ Wrong: Testing both Task.Left and Task in same file
public class EnsureTests_Task
{
    // Mix of left-async and both-async tests - confusing!
}
```

```csharp
// ✅ Correct: Separate files
public class Ensure_Task_Tests { }           // Both async
public class Ensure_Task_Left_Tests { }      // Left async only
public class Ensure_Task_Right_Tests { }     // Right async only
```

### ❌ Don't Use Ambiguous Async Constructors

```csharp
// ❌ Ambiguous between Task and ValueTask
.EnsureAsync(v => Task.FromResult(v > 0), error)

// ✅ Explicit
.EnsureAsync(v => new ValueTask<bool>(v > 0), error)
```

### ❌ Don't Skip Early Exit Tests

```csharp
// ✅ Always verify functions aren't called on failure
var predicateInvoked = false;
var result = await Result.Failure<int>(error)
    .EnsureAsync(v => { predicateInvoked = true; return v > 0; }, error);

predicateInvoked.Should().BeFalse("predicate should not be invoked for failed results");
```

## File Location Guidelines

### Source Files
- **Core ROP**: `RailwayOrientedProgramming/src/Result/Extensions/`
- **Value Objects**: `PrimitiveValueObjects/src/`
- **DDD**: `DomainDrivenDesign/src/`
- **ASP.NET Integration**: `Asp/src/`
- **HTTP Extensions**: `Http/src/`

### Test Files
- **ROP Tests**: `RailwayOrientedProgramming/tests/Results/Extensions/`
- **Value Object Tests**: `PrimitiveValueObjects/tests/`
- **DDD Tests**: `DomainDrivenDesign/tests/`
- **ASP.NET Tests**: `Asp/tests/`
- **HTTP Tests**: `Http/tests/`

## Documentation Standards

### XML Documentation

All public APIs should have XML documentation:

```csharp
/// <summary>
/// Returns a new failure result if the predicate is false. Otherwise returns the starting result.
/// </summary>
/// <param name="result">The source result.</param>
/// <param name="predicate">The predicate to evaluate.</param>
/// <param name="error">The error to return if the predicate fails.</param>
/// <returns>The original result if successful and predicate passes; otherwise a failure.</returns>
public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(
    this ValueTask<Result<TOk>> result,
    Func<TOk, ValueTask<bool>> predicate,
    Error error)
{
    // Implementation...
}
```

### Test Documentation

Add XML summary to test classes explaining what they test:

```csharp
/// <summary>
/// Tests for Ensure.ValueTask.cs where BOTH input and predicates are async.
/// - Input: ValueTask&lt;Result&lt;TOk&gt;&gt; (left is async)
/// - Predicates: ValueTask&lt;bool&gt;, ValueTask&lt;Result&lt;&gt;&gt; (right is async)
/// </summary>
public class Ensure_ValueTask_Tests
{
    // Tests...
}
```

## Performance Considerations

- ✅ Use `ValueTask<T>` for high-frequency, potentially synchronous operations
- ✅ Use `Task<T>` for I/O-bound operations
- ✅ Avoid allocations in hot paths
- ✅ Use `ConfigureAwait(false)` in library code (not test code)
- ✅ Consider `readonly struct` for value types

## Questions?

When in doubt about test organization:
1. Ask: "Is the **input** async?" → If yes, it's a "Left" variant
2. Ask: "Are the **predicates/functions** async?" → If yes, it's a "Right" variant
3. Both? → Base variant (no Left/Right suffix)
4. Neither? → Not an async extension test

**Quick Reference:**
- Both async = `[Method]Tests.[Type].cs`
- Left async = `[Method]Tests.[Type].Left.cs`
- Right async = `[Method]Tests.[Type].Right.cs`
