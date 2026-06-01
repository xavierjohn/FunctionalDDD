# TRLS009 — Use async method variant for async lambda

- **Severity:** Warning
- **Category:** Trellis

## What it detects
Flags synchronous Trellis methods such as `Map`, `Bind`, `Tap`, `Ensure`, and `TapOnFailure` when any supplied lambda or method group does async work.

## Why it matters
The sync method treats the returned `Task` or `ValueTask` as just another value. The async work does not get awaited by the Trellis pipeline.

> [!WARNING]
> This rule covers three cases: `async` lambdas, non-async lambdas whose converted return type is `Task` or `ValueTask`, and method groups returning `Task` or `ValueTask`.

## Bad example
```csharp
using System.Threading.Tasks;
using Trellis;

static class Example
{
    public static Result<Task<int>> Bad()
    {
        var result = Result.Ok("Ada");
        return result.Map(LookupLengthAsync);
    }

    static Task<int> LookupLengthAsync(string value) =>
        Task.FromResult(value.Length);
}
```

## Good example
```csharp
using System.Threading.Tasks;
using Trellis;

static class Example
{
    public static Task<Result<int>> Good()
    {
        var result = Result.Ok("Ada");
        return result.MapAsync(LookupLengthAsync);
    }

    static Task<int> LookupLengthAsync(string value) =>
        Task.FromResult(value.Length);
}
```

## Code fix available
Yes, when the enclosing scope can safely become async. The fix renames the sync API to the matching async API, awaits the rewritten call, and adds `async` to Task/ValueTask-returning methods when it can do so without changing surrounding expressions or other returns.

No fix is offered for chained calls, nested wrapper expressions, non-async lambdas, used `var` locals, synchronous `void` methods, synchronous value-returning methods, direct returns from already-async scopes, methods/local functions with `ref`/`out`/`in` parameters, or Task/ValueTask-returning scopes with other task-returning `return` statements because those require manual return-type, delegate-shape, parameter-shape, or pipeline-flow changes first.

## Configuration
Use standard Roslyn configuration if you need to suppress this rule in a specific scope.

```ini
dotnet_diagnostic.TRLS009.severity = none
```

```csharp
#pragma warning disable TRLS009
// Intentional: documented exception or test-only pattern.
#pragma warning restore TRLS009
```

> [!TIP]
> If the callback does async work, move to the matching async API immediately instead of returning a `Task` from the sync overload.

