# FDDD014: Use async method variant for async lambda

## Cause

Using a synchronous ROP method (Map, Bind, Tap, Ensure, TapOnFailure) with an async lambda, which results in the async operation not being properly awaited.

## Rule Description

When you pass an async lambda to a synchronous method like `Map`, the result type becomes `Result<Task<T>>` instead of `Task<Result<T>>`. This means:

- The async operation may not complete before you try to use the result
- The Task is never awaited
- Exceptions in the async operation may be unobserved

Always use the async variant (`MapAsync`, `BindAsync`, etc.) when your lambda is async.

## How to Fix Violations

Replace the sync method with its async variant:

```csharp
// ❌ Bad - Map with async lambda creates Result<Task<T>>
var result = userResult.Map(async user => await SendEmailAsync(user));

// ✅ Good - MapAsync properly handles async lambda
var result = await userResult.MapAsync(async user => await SendEmailAsync(user));
```

## Examples

### Example 1: Async Map

```csharp
// ❌ Bad - Result<Task<EmailResult>> (Task never awaited!)
var result = emailAddress
    .Map(async email => await emailService.SendAsync(email));

// ✅ Good - Task<Result<EmailResult>>
var result = await emailAddress
    .MapAsync(async email => await emailService.SendAsync(email));
```

### Example 2: Async Bind

```csharp
// ❌ Bad
var result = userId
    .Bind(async id => await GetUserAsync(id));  // Returns Result<Task<Result<User>>>!

// ✅ Good
var result = await userId
    .BindAsync(async id => await GetUserAsync(id));  // Returns Task<Result<User>>
```

### Example 3: Async Tap

```csharp
// ❌ Bad - Side effect may not complete
var result = order
    .Tap(async o => await auditService.LogAsync(o));

// ✅ Good - Side effect is awaited
var result = await order
    .TapAsync(async o => await auditService.LogAsync(o));
```

### Example 4: Method Group

```csharp
// ❌ Bad - ProcessAsync returns Task
var result = items.Map(ProcessAsync);

// ✅ Good
var result = await items.MapAsync(ProcessAsync);
```

## Async Method Variants

| Sync Method | Async Variant | Use When |
|-------------|---------------|----------|
| `Map` | `MapAsync` | Lambda returns `Task<T>` |
| `Bind` | `BindAsync` | Lambda returns `Task<Result<T>>` |
| `Tap` | `TapAsync` | Lambda returns `Task` (side effect) |
| `Ensure` | `EnsureAsync` | Predicate returns `Task<bool>` |
| `TapOnFailure` | `TapOnFailureAsync` | Lambda returns `Task` (error handling) |

## Code Fix

The code fix automatically replaces the sync method with its async variant:

**Before:**
```csharp
result.Map(async x => await ProcessAsync(x))
```

**After:**
```csharp
result.MapAsync(async x => await ProcessAsync(x))
```

> **Note:** You'll still need to add `await` to the call chain if not already present.

## Why This Matters

When you use `Map` with an async lambda:

```csharp
Result<int> number = Result.Success(42);
var result = number.Map(async n => await ComputeAsync(n));
// result is Result<Task<int>>, NOT Task<Result<int>>!
```

The `Task` inside the `Result` is never awaited, leading to:
- Unobserved exceptions
- Operations that don't complete before you use the result
- Confusing debugging experiences

## Related Rules

- [FDDD009](FDDD009.md) - Incorrect async Result usage (blocking with .Result/.Wait())
- [FDDD002](FDDD002.md) - Use Bind instead of Map when lambda returns Result

## See Also

- [Async/Await Best Practices](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
