# FDDD004: Unsafe access to Result.Error

## Cause

Accessing `Result.Error` without first checking `IsFailure` or using proper guards.

## Rule Description

`Result.Error` throws an `InvalidOperationException` if the `Result` is in a success state. Accessing it without checking `IsFailure` first can cause runtime exceptions.

## How to Fix Violations

### Option 1: Check IsFailure First
```csharp
// ❌ Bad - Unsafe access
var error = result.Error;

// ✅ Good - Guarded access
if (result.IsFailure)
{
    var error = result.Error;
    Logger.LogError(error.Detail);
}
```

### Option 2: Use Match
```csharp
// ✅ Good - Pattern matching
return result.Match(
    onSuccess: customer => Ok(customer),
    onFailure: error => BadRequest(error));
```

### Option 3: Use TryGetError
```csharp
// ✅ Good - Safe extraction
if (result.TryGetError(out var error))
{
    Logger.LogError(error.Detail);
}
```

## Code Fix

This diagnostic offers an automatic code fix that wraps the unsafe access in an `if (result.IsFailure)` guard.

### Example Code Fix Transformation

**Before:**
```csharp
var result = GetCustomer();
var error = result.Error;
Logger.LogError(error.Code);
```

**After (automatic):**
```csharp
var result = GetCustomer();
if (result.IsFailure)
{
    var error = result.Error;
    Logger.LogError(error.Code);
}
```

## When to Suppress Warnings

This warning can be suppressed in test code where you're explicitly testing failure scenarios.

```csharp
[Fact]
public void Should_FailWithInvalidEmail()
{
    var result = Customer.Create("invalid-email");
    result.IsFailure.Should().BeTrue();
    #pragma warning disable FDDD004
    result.Error.Should().BeOfType<ValidationError>();
    #pragma warning restore FDDD004
}
```

## Related Rules

- [FDDD003](FDDD003.md) - Unsafe access to Result.Value
- [FDDD005](FDDD005.md) - Consider using MatchError for error type discrimination
