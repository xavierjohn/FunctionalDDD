# FDDD006: Unsafe access to Maybe.Value

## Cause

Accessing `Maybe.Value` without first checking `HasValue` or using proper guards.

## Rule Description

`Maybe.Value` throws an `InvalidOperationException` if the `Maybe` has no value. Accessing it without checking `HasValue` first can cause runtime exceptions.

## How to Fix Violations

### Option 1: Check HasValue First
```csharp
// ❌ Bad - Unsafe access
var customer = maybe.Value;

// ✅ Good - Guarded access
if (maybe.HasValue)
{
    var customer = maybe.Value;
    // Use customer...
}
```

### Option 2: Use TryGetValue
```csharp
// ✅ Good - Safe extraction
if (maybe.TryGetValue(out var customer))
{
    // Use customer...
}
```

### Option 3: Use GetValueOrDefault
```csharp
// ✅ Good - With fallback
var customer = maybe.GetValueOrDefault(defaultCustomer);
```

### Option 4: Convert to Result
```csharp
// ✅ Good - Convert to Result for better composability
return maybe.ToResult(Error.NotFound("Customer not found"))
    .Map(customer => customer.ToDto());
```

## Code Fix

This diagnostic offers an automatic code fix that wraps the unsafe access in an `if (maybe.HasValue)` guard.

### Example Code Fix Transformation

**Before:**
```csharp
var maybe = FindCustomer(id);
var customer = maybe.Value;
customer.UpdateEmail(newEmail);
```

**After (automatic):**
```csharp
var maybe = FindCustomer(id);
if (maybe.HasValue)
{
    var customer = maybe.Value;
    customer.UpdateEmail(newEmail);
}
```

## When to Suppress Warnings

This warning can be suppressed in test code where you're explicitly testing the "has value" scenario.

```csharp
[Fact]
public void Should_FindExistingCustomer()
{
    var maybe = repository.FindById(customerId);
    maybe.HasValue.Should().BeTrue();
    #pragma warning disable FDDD006
    maybe.Value.Name.Should().Be("John");
    #pragma warning restore FDDD006
}
```

## Best Practices

Consider using `Result<T>` instead of `Maybe<T>` when you want to provide error information:

```csharp
// Maybe provides no error context
public Maybe<Customer> FindCustomer(Guid id) { ... }

// Result provides clear error information
public Result<Customer> FindCustomer(Guid id)
{
    var maybe = repository.FindById(id);
    return maybe.ToResult(Error.NotFound($"Customer {id} not found"));
}
```

## Related Rules

- [FDDD003](FDDD003.md) - Unsafe access to Result.Value
