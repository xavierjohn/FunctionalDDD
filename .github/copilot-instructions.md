# GitHub Copilot Instructions for FunctionalDDD

## Project Overview

This is a functional programming library for .NET that implements Railway Oriented Programming (ROP) patterns, Domain-Driven Design (DDD) primitives, and value objects.

**Target Frameworks:**
- .NET 10

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

### Single-Line Statements

Omit braces for single-line statements to improve readability and reduce vertical space:

```csharp
// ✅ Preferred - no braces for single statement
if (result.IsSuccess)
    action(result.Value);

if (value is null)
    return Error.Validation("Value cannot be null");

// ❌ Avoid - unnecessary braces
if (result.IsSuccess)
{
    action(result.Value);
}
```

**Note:** Use braces when the statement is complex or spans multiple lines for clarity.

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

## T4 Template Testing Strategy

### Overview

The RailwayOrientedProgramming library uses **T4 templates** to generate tuple overloads (2-tuple through 9-tuple) for extension methods. Since all tuple sizes are generated from the same template with identical logic, **testing every permutation is unnecessary**.

### T4-Generated Files

| Template File | Generated Source | Purpose |
|--------------|------------------|---------|
| `TapTs.g.tt` | `TapTs.g.cs` | Tap operations for tuple Results |
| `TapOnFailureTs.g.tt` | `TapOnFailureTs.g.cs` | TapOnFailure operations for tuple Results |
| `BindTs.g.tt` | `BindTs.g.cs` | Bind operations for tuple Results |
| `MatchTupleTs.g.tt` | `MatchTupleTs.g.cs` | Match operations for tuple Results |
| `CombineTs.g.tt` | `CombineTs.g.cs` | Combine operations for multiple Results |
| `AwaitTs.g.tt` | `AwaitTs.g.cs` | Await operations for parallel Results |
| `ParallelAsyncs.g.tt` | `ParallelAsyncs.g.cs` | Parallel async operations |

### Testing Strategy

**Test the 2-tuple comprehensively, validate other sizes work correctly.**

```csharp
/// <summary>
/// Functional tests for Tap operations on tuple results generated by TapTs.g.tt.
/// 
/// Since all tuple sizes (2-9) are generated from the same T4 template, we test the 2-tuple
/// permutation comprehensively and validate other sizes work correctly.
/// </summary>
public class TapTupleTests : TestBase
{
    #region 2-Tuple Tap Tests (Comprehensive Coverage)
    
    [Fact]
    public void Tap_2Tuple_Success_ExecutesAction() { /* ... */ }
    
    [Fact]
    public void Tap_2Tuple_Failure_DoesNotExecute() { /* ... */ }
    
    [Fact]
    public void Tap_2Tuple_Success_DestructuresTuple() { /* ... */ }
    
    // ... comprehensive tests for 2-tuple ...
    
    #endregion

    #region Other Tuple Sizes (Validation Tests)
    
    // Simple validation that larger tuples work - not comprehensive
    [Fact]
    public void Tap_3Tuple_Success_ExecutesAction() { /* ... */ }
    
    [Fact]
    public void Tap_9Tuple_Success_ExecutesAction() { /* ... */ }
    
    [Fact]
    public void Tap_9Tuple_Failure_DoesNotExecute() { /* ... */ }
    
    #endregion
}
```

### Test File Naming for T4-Generated Code

| Source File | Test File | Description |
|-------------|-----------|-------------|
| `TapTs.g.cs` | `TapTupleTests.cs` | Tuple Tap tests |
| `TapOnFailureTs.g.cs` | `TapOnFailureTupleTests.cs` | Tuple TapOnFailure tests |
| `BindTs.g.cs` | `BindTsTests.cs` | Tuple Bind tests |
| `MatchTupleTs.g.cs` | (tracing tests) | Tuple Match tests |
| `ParallelAsyncs.g.cs` | `ParallelAsyncTests.cs` | Parallel async tests |

### Coverage Expectations

**Expected coverage for T4-generated code is ~12-35%** because:
- Templates generate 8 tuple sizes (2-9)
- We comprehensively test only 1-2 sizes
- Each size represents ~12.5% of the generated code
- Comprehensive 2-tuple + validation tests for 3,9-tuple ≈ 25-35% coverage

**This is intentional and correct!** Don't try to achieve 100% coverage on T4-generated code.

### What to Test for T4-Generated Code

#### ✅ DO Test (2-tuple comprehensively)

- Success path executes action/function
- Failure path skips action/function
- Tuple destructuring works correctly
- Original result is preserved
- Chained operations execute in order
- Different types work (string, int, bool, etc.)
- Async variants (Task, ValueTask)
- Real-world scenarios

#### ✅ DO Test (Other sizes - validation only)

- Success path works (one test per size)
- Failure path works (one test for largest size)

#### ❌ DON'T Test

- Every async variant for every tuple size
- Every error type for every tuple size
- Edge cases for every tuple size

### Example Test Structure

```csharp
public class TapOnFailureTupleTests : TestBase
{
    #region 2-Tuple TapOnFailure Tests (Comprehensive Coverage)
    
    // ~15-20 comprehensive tests for 2-tuple
    // Cover all async variants, error types, scenarios
    
    #endregion

    #region 2-Tuple Async Tests
    
    // Async variants for 2-tuple
    
    #endregion

    #region Other Tuple Sizes (Validation Tests)
    
    // 1 test per size (3-9) just to validate generation
    
    #endregion

    #region Real-World Scenarios
    
    // Integration tests using 2 or 3 tuple
    
    #endregion
}
```

### Modifying T4 Templates

When modifying a T4 template:

1. **Run the template** to regenerate the `.g.cs` file
2. **Update 2-tuple tests** if the pattern changed
3. **Verify one larger tuple** still works (e.g., 5-tuple or 9-tuple)
4. **Don't add tests for every tuple size** - the template ensures consistency

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

## Activity Tracing and OpenTelemetry

### Setting Activity Status Correctly

When working with OpenTelemetry `Activity` tracing, **always use the local `activity` variable** returned by `StartActivity()`, not `Activity.Current`:

```csharp
// ❌ WRONG - Race condition prone
using var activity = ActivitySource.StartActivity("Operation");
Activity.Current?.SetStatus(ActivityStatusCode.Ok);  // Don't use Activity.Current!;

// ✅ CORRECT - Thread-safe and reliable  
using var activity = ActivitySource.StartActivity("Operation");
activity?.SetStatus(ActivityStatusCode.Ok);  // Use the local variable
```

**Why this matters:**
- `Activity.Current` is a static thread-local property that may not reference the activity you just created
- In concurrent scenarios, `Activity.Current` can be null or reference a different activity
- Using the local `activity` variable ensures you're always operating on the correct activity instance
- This prevents race conditions in both production and test environments

**Important: The Result constructor sets `Activity.Current`, not the local activity variable!**

### Test Isolation with AsyncLocal

When testing code that uses `ActivitySource`, use **`AsyncLocal<ActivitySource?>`** for proper test isolation:

```csharp
// Production code - use AsyncLocal for test injection
public static class PrimitiveValueObjectTrace
{
    private static readonly ActivitySource _defaultActivitySource = new("Functional DDD PVO", "1.0.0");
    
    // AsyncLocal provides isolation across async boundaries and parallel tests
    private static readonly AsyncLocal<ActivitySource?> _testActivitySource = new();
    
    public static ActivitySource ActivitySource => _testActivitySource.Value ?? _defaultActivitySource;
    
    internal static void SetTestActivitySource(ActivitySource testSource) 
        => _testActivitySource.Value = testSource;
    
    internal static void ResetTestActivitySource() 
        => _testActivitySource.Value = null;
}
```

**Why AsyncLocal is better than static fields:**
- ✅ **Isolation**: Each async context gets its own value
- ✅ **Thread-safe**: No race conditions with parallel tests
- ✅ **Async-aware**: Works across async/await boundaries
- ✅ **Parallel execution**: Tests can run in parallel without interference
- ✅ **No xUnit collections needed**: No need for `[Collection]` attributes

#### Activity Test Helper Pattern

Create test helpers that manage ActivitySource lifecycle:

```csharp
public sealed class PvoActivityTestHelper : IDisposable
{
    private readonly ActivitySource _testActivitySource;
    private readonly ActivityListener _listener;
    private readonly List<Activity> _capturedActivities = [];

    public PvoActivityTestHelper()
    {
        // Create unique ActivitySource per test instance
        _testActivitySource = new ActivitySource($"Test-PVO-{Guid.NewGuid():N}");
        
        // Configure listener to capture activities
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source == _testActivitySource,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => 
                ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity =>
            {
                lock (_capturedActivities)
                {
                    _capturedActivities.Add(activity);
                }
            }
        };
        
        ActivitySource.AddActivityListener(_listener);
        
        // Inject test source into AsyncLocal (isolated per test context)
        PrimitiveValueObjectTrace.SetTestActivitySource(_testActivitySource);
    }

    public bool WaitForActivityCount(int expectedCount, TimeSpan? timeout = null)
    {
        var maxWait = timeout ?? TimeSpan.FromSeconds(2);
        return SpinWait.SpinUntil(() => ActivityCount >= expectedCount, maxWait);
    }

    public void Dispose()
    {
        // Reset AsyncLocal for this context
        PrimitiveValueObjectTrace.ResetTestActivitySource();
        _listener.Dispose();
        _testActivitySource.Dispose();
    }
}
```

#### Using in Tests

```csharp
// No [Collection] attribute needed - AsyncLocal provides isolation!
public class PvoTracingExtensionsTests : IDisposable
{
    private readonly PvoActivityTestHelper _activityHelper = new();

    [Fact]
    public void EmailAddress_ValidationFailure_SetsErrorStatus()
    {
        // Act
        var emailResult = EmailAddress.TryCreate("invalid-email");

        // Assert
        _activityHelper.WaitForActivityCount(1).Should().BeTrue();
        var activities = _activityHelper.CapturedActivities;
        activities.Should().ContainSingle();
        var activity = activities[0];
        activity.Status.Should().Be(ActivityStatusCode.Error);
    }

    public void Dispose() => _activityHelper.Dispose();
}

// Other test classes can run in parallel - no interference!
public class EmailAddressTests
{
    [Fact]
    public void Can_create_valid_email()
    {
        var result = EmailAddress.TryCreate("test@example.com");
        result.IsSuccess.Should().BeTrue();
    }
}
```

### Summary: Activity Tracing Best Practices

| Scenario | Correct Pattern | Why |
|----------|----------------|-----|
| **Setting activity status** | `activity?.SetStatus(...)` | Avoids `Activity.Current` race conditions |
| **Test isolation** | `AsyncLocal<ActivitySource?>` | Provides per-context isolation, works with async and parallel execution |
| **Test helpers** | Unique source + inject/reset pattern | Ensures each test has isolated tracing |
| **Production code** | Use local `activity` variable | Thread-safe and reliable |

**Key Advantages of AsyncLocal:**
- ✅ **No `[Collection]` attributes needed** - Tests run in parallel by default
- ✅ **True isolation** - Each test context gets its own ActivitySource
- ✅ **Async-aware** - Works correctly across async/await boundaries
- ✅ **Faster tests** - No forced sequential execution
- ✅ **Simpler code** - No need to manage test collections

### IMPORTANT: Result Constructor Automatically Sets Activity Status

**The `Result<T>` constructor automatically sets `Activity.Current` status, but ROP extension methods create their own child activities!**

```csharp
// Result<T> constructor implementation:
internal Result(bool isFailure, TValue? ok, Error? error)
{
    // ... validation code ...
    
    // ✅ AUTOMATIC: Sets Activity.Current status
    Activity.Current?.SetStatus(IsFailure ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
    
    if (IsFailure && Activity.Current is { } act && error is not null)
    {
        act.SetTag("result.error.code", error.Code);
    }
}

// Implicit operator also triggers Result constructor:
public static implicit operator Result<TValue>(TValue value) => Result.Success(value);
```

**Activity status handling in different scenarios:**

### ✅ Value Object Factory Methods (TryCreate)

Value object `TryCreate` methods don't need manual activity status setting because:
1. No parent activity exists when TryCreate is called
2. The `using var activity` creates the root activity that becomes `Activity.Current`
3. Result constructor sets `Activity.Current` status (which is our activity)

```csharp
// ✅ CORRECT - No manual status setting needed
public static Result<EmailAddress> TryCreate(string? value, string? fieldName = null)
{
    using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity("EmailAddress.TryCreate");
    // At this point: Activity.Current == activity (no parent activity exists)
    
    if (value is not null && EmailRegEx().IsMatch(value))
    {
        // ✅ Implicit conversion → Result.Success → Result constructor sets Activity.Current
        // Since Activity.Current == activity, our activity gets the status
        return new EmailAddress(value);
    }

    // ✅ Result.Failure → Result constructor sets Activity.Current (which is our activity)
    return Result.Failure<EmailAddress>(Error.Validation("Email address is not valid.", field));
}
```

**Why it works:** When there's no parent activity, the activity we create becomes `Activity.Current`. The Result constructor sets `Activity.Current` status, which is our activity.

### ⚠️ ROP Extension Methods (Bind, Tap, Map, etc.)

ROP extension methods create **child activities** and must explicitly set their own activity status:

```csharp
// ✅ CORRECT - Explicitly sets activity status for the Tap operation
public static Result<TValue> Tap<TValue>(this Result<TValue> result, Action<TValue> action)
{
    using var activity = RopTrace.ActivitySource.StartActivity();  // Child activity
    if (result.IsSuccess)
        action(result.Value);

    result.LogActivityStatus();  // ✅ Sets the child activity status
    return result;
}

// ✅ CORRECT - Sets activity status on early return
public static Result<TResult> Bind<TValue, TResult>(this Result<TValue> result, Func<TValue, Result<TResult>> func)
{
    using var activity = RopTrace.ActivitySource.StartActivity();
    if (result.IsFailure)
    {
        result.LogActivityStatus();  // ✅ Must set child activity status before returning
        return Result.Failure<TResult>(result.Error);
    }

    var newResult = func(result.Value);
    newResult.LogActivityStatus();  // ✅ Set status for the new result too
    return newResult;
}
```

**Why ROP methods need manual status:**
- ROP methods create **child activities** with `StartActivity()`
- A parent activity already exists (from the calling code)
- The child activity is different from `Activity.Current`
- Result constructor sets `Activity.Current`, not the child activity
- Therefore, ROP methods must explicitly set their child activity status using `result.LogActivityStatus()` or `activity?.SetStatus(...)`

**Note:** `result.LogActivityStatus()` is a helper that calls `Activity.Current?.SetStatus(...)` based on the result's success/failure state.

### Summary: When to Set Activity Status

| Scenario | Manual Status Needed? | Why |
|----------|----------------------|-----|
| **Value Object `TryCreate`** | ❌ No | Result constructor sets `Activity.Current` which equals the `activity` variable (no parent activity) |
| **ROP Extensions (Bind, Map, Tap)** | ✅ **YES - REQUIRED** | Create child activities; Result constructor only sets `Activity.Current`, not the child activity created by the extension method |
| **Direct Result creation** | ❌ No | Result constructor always sets `Activity.Current` |

**Key Insight:**
- In `TryCreate` methods, the `using var activity` **IS** `Activity.Current` (no parent activity exists when called)
- In ROP methods, `using var activity` creates a **child** of `Activity.Current` (parent activity exists from calling code)
- Result constructor only sets `Activity.Current`, so child activities need explicit status setting via `result.LogActivityStatus()` or manually set the activity status.
