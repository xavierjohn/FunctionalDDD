# TRLS020: Use SaveChangesResultAsync instead of SaveChangesAsync

## Cause

Code calls `SaveChanges()` or `SaveChangesAsync()` directly on a `DbContext` instance instead of using the Trellis `SaveChangesResultAsync` or `SaveChangesResultUnitAsync` extension methods.

## Rule Description

Direct `SaveChanges`/`SaveChangesAsync` calls bypass the Result pipeline and turn database errors into unhandled exceptions. The Trellis `DbContextExtensions` provide Result-returning alternatives that capture database errors as `Result<T>` failures, keeping your code on the railway.

| Extension Method | Returns | Use When |
|------------------|---------|----------|
| `SaveChangesResultUnitAsync` | `Task<Result<Unit>>` | You don't need the row count |
| `SaveChangesResultAsync` | `Task<Result<int>>` | You need the affected row count |

This rule fires as a **Warning** because the code will compile and run, but database errors will surface as unhandled exceptions rather than flowing through the Result pipeline.

## How to Fix Violations

Replace `SaveChangesAsync()` or `SaveChanges()` with the appropriate Result-returning extension method.

### Standalone calls (row count not used)

```csharp
// ❌ Bad - exceptions bypass Result pipeline
await _dbContext.SaveChangesAsync(ct);

// ✅ Good - errors flow through Result pipeline
await _dbContext.SaveChangesResultUnitAsync(ct);
```

### When the row count is needed

```csharp
// ❌ Bad - exceptions bypass Result pipeline
var count = await _dbContext.SaveChangesAsync(ct);

// ✅ Good - returns Result<int> with the row count
var countResult = await _dbContext.SaveChangesResultAsync(ct);
```

### Synchronous calls

```csharp
// ❌ Bad - sync call bypasses Result pipeline
_dbContext.SaveChanges();

// ✅ Good - switch to async Result variant
await _dbContext.SaveChangesResultUnitAsync(ct);
```

## Code Fix

The code fix automatically renames the method call:

- **Standalone `SaveChangesAsync`** → `SaveChangesResultUnitAsync`
- **`SaveChangesAsync` with return value used** → `SaveChangesResultAsync`
- **`SaveChangesAsync(bool, CancellationToken)` standalone** → `SaveChangesResultUnitAsync(bool, CancellationToken)`
- **Standalone `SaveChanges` in a void method** → `SaveChangesResultUnitAsync` (also adds `await`, `async`, and `Task` return type)
- **`SaveChanges(bool)` in a void method** → `SaveChangesResultUnitAsync(bool)` (also adds `await`, `async`, and `Task` return type)

The code fix also adds `using Trellis.EntityFrameworkCore;` and `using System.Threading.Tasks;` if not already present.

The code fix correctly handles `.ConfigureAwait(false)` chains, preserving them in the fixed code. The `acceptAllChangesOnSuccess` parameter is preserved when present.

> [!NOTE]
> The code fix is not offered when:
>
> - **Sync `SaveChanges()`**: The return value is used, or the containing method has a non-void return type
> - **Async `SaveChangesAsync` in a return statement**: e.g., `return await _dbContext.SaveChangesAsync(ct)` — the method return type would need to change from `Task<int>` to `Task<Result<int>>`
>
> These cases require manual refactoring.

## Examples

### Example 1: Repository Pattern

```csharp
// ❌ Bad
public async Task<Result<Order>> CreateOrder(Order order, CancellationToken ct)
{
    _dbContext.Orders.Add(order);
    await _dbContext.SaveChangesAsync(ct); // TRLS020
    return Result.Success(order);
}

// ✅ Good
public async Task<Result<Order>> CreateOrder(Order order, CancellationToken ct)
{
    _dbContext.Orders.Add(order);
    return await _dbContext.SaveChangesResultUnitAsync(ct)
        .Map(_ => order);
}
```

### Example 2: Checking Affected Rows

```csharp
// ❌ Bad
public async Task<Result<int>> UpdatePrices(decimal factor, CancellationToken ct)
{
    foreach (var product in await _dbContext.Products.ToListAsync(ct))
        product.Price *= factor;

    var count = await _dbContext.SaveChangesAsync(ct); // TRLS020
    return Result.Success(count);
}

// ✅ Good
public async Task<Result<int>> UpdatePrices(decimal factor, CancellationToken ct)
{
    foreach (var product in await _dbContext.Products.ToListAsync(ct))
        product.Price *= factor;

    return await _dbContext.SaveChangesResultAsync(ct);
}
```

## Related Rules

- [TRLS009](TRLS009.md) - Incorrect async Result usage (blocking)
- [TRLS001](TRLS001.md) - Result return value is not handled

## See Also

- [Entity Framework Core Integration](../integration-ef.md) - Full EF Core integration guide
