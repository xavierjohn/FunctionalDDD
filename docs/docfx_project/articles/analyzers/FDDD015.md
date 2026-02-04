# FDDD015: Don't throw exceptions in Result chains

## Cause

Using a `throw` statement or expression inside a lambda passed to `Bind`, `Map`, `Tap`, `Ensure`, or their async variants.

## Rule Description

Throwing exceptions inside Result chain lambdas defeats the purpose of Railway Oriented Programming. The whole point of using `Result<T>` is to represent failures as values on the "failure track" rather than throwing exceptions.

When you throw inside a Result chain:
- The exception bypasses the normal failure handling
- Error handling becomes unpredictable
- You lose the benefits of composable error handling

## How to Fix Violations

Return a `Result.Failure<T>()` instead of throwing:

```csharp
// ❌ Bad - Throwing defeats ROP
result.Bind(x =>
{
    if (x < 0) throw new ArgumentException("Must be positive");
    return Result.Success(x);
});

// ✅ Good - Return failure Result
result.Bind(x =>
    x < 0
        ? Result.Failure<int>(Error.Validation("Must be positive", "value"))
        : Result.Success(x));
```

## Examples

### Example 1: Throw Statement in Bind

```csharp
// ❌ Bad
var result = customerId
    .Bind(id =>
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Invalid customer ID");
        return GetCustomer(id);
    });

// ✅ Good
var result = customerId
    .Bind(id =>
        id == Guid.Empty
            ? Result.Failure<Customer>(Error.Validation("Invalid customer ID", "customerId"))
            : GetCustomer(id));
```

### Example 2: Throw Expression in Ternary

```csharp
// ❌ Bad
var result = amount
    .Map(a => a < 0 ? throw new InvalidOperationException() : a * 1.1m);

// ✅ Good
var result = amount
    .Ensure(a => a >= 0, Error.Validation("Amount must be non-negative"))
    .Map(a => a * 1.1m);
```

### Example 3: Throw in Tap

```csharp
// ❌ Bad
var result = order
    .Tap(o =>
    {
        if (!o.IsValid)
            throw new InvalidOperationException("Invalid order");
        logger.LogInformation("Processing order {Id}", o.Id);
    });

// ✅ Good - Use Ensure for validation, Tap for side effects
var result = order
    .Ensure(o => o.IsValid, Error.Validation("Invalid order"))
    .Tap(o => logger.LogInformation("Processing order {Id}", o.Id));
```

## Why This Matters

Railway Oriented Programming provides:
- **Predictable error handling**: Errors flow through the chain
- **Composability**: Chain operations without try/catch
- **Explicit failure paths**: Failures are values, not exceptions

Throwing exceptions breaks all of these benefits.

## Related Rules

- [FDDD002](FDDD002.md) - Use Bind instead of Map when lambda returns Result
- [FDDD003](FDDD003.md) - Unsafe access to Result.Value

## See Also

- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/)
