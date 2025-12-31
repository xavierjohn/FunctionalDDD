# Performance

This guide covers performance characteristics, benchmarks, and optimization techniques.

## Table of Contents

- [Overview](#overview)
- [Key Metrics](#key-metrics)
- [Benchmark Results](#benchmark-results)
- [Real-World Context](#real-world-context)
- [Optimization Tips](#optimization-tips)
- [Running Your Own Benchmarks](#running-your-own-benchmarks)

## Overview

Benchmarks on **.NET 10** show railway-oriented programming adds only **~11-16 nanoseconds** overhead compared to imperative code—less than **0.002%** of typical I/O operations.

**Test Environment:**
- **CPU**: Intel Core i7-1185G7 @ 3.00GHz
- **OS**: Windows 11
- **.NET**: 10.0.1

## Key Metrics

| Metric | Finding |
|--------|---------|
| **Overhead** | 11-16 nanoseconds (~12-13% vs imperative) |
| **Memory** | Identical allocations to imperative code |
| **Success Path** | Highly optimized, minimal allocations |
| **Error Path** | Efficient with short-circuit optimization |
| **Combine Operations** | 7-58 ns for 2-5 results |
| **Bind Operations** | 9-63 ns for 1-5 chains |
| **Map Operations** | 4.6-44.5 ns for 1-5 transforms |

## Benchmark Results

### ROP vs Imperative Style

Direct comparison of ROP versus traditional if-style code:

| Method | Mean | Allocated |
|--------|------|-----------|
| **ROP Happy Path** | 147 ns | 144 B |
| **Imperative Happy Path** | 131 ns | 144 B |
| **ROP Error Path** | 99 ns | 184 B |
| **Imperative Error Path** | 88 ns | 184 B |

**Key Takeaways:**
- ROP adds **~16 ns** on success path (12% overhead)
- ROP adds **~11 ns** on error path (13% overhead)
- **Identical memory allocations** between approaches
- Error paths are faster due to short-circuit optimization

### Core Operation Benchmarks

#### Combine Operations

Aggregating multiple Result objects:

| Results Combined | Mean Time | Allocated |
|-----------------|-----------|-----------|
| 2 results | 7 ns | 0 B |
| 3 results | 30 ns | 0 B |
| 5 results | 58 ns | 0 B |

**Zero allocations** - highly efficient for validation scenarios.

#### Bind Operations

Chaining operations that return Results:

| Chain Length | Mean Time | Allocated |
|-------------|-----------|-----------|
| 1 bind | 9 ns | 0 B |
| 3 binds | 35 ns | 0 B |
| 5 binds | 63 ns | 0 B |

**Linear scaling** with excellent performance characteristics.

#### Map Operations

Transforming successful values:

| Transforms | Mean Time | Allocated |
|-----------|-----------|-----------|
| 1 map | 4.6 ns | 0 B |
| 3 maps | 21 ns | 0 B |
| 5 maps | 44.5 ns | 0 B |

**Fastest operation** with zero allocations on success path.

#### Tap Operations

Executing side effects:

| Taps | Mean Time | Allocated |
|------|-----------|-----------|
| 1 tap | 3 ns | 0 B |
| 3 taps | 18 ns | 32 B |
| 5 taps | 37.4 ns | 64 B |

**Minimal overhead** for logging and side effects.

#### Ensure Operations

Adding validation checks:

| Checks | Mean Time | Allocated |
|--------|-----------|-----------|
| 1 ensure | 22.5 ns | 152 B |
| 3 ensures | 89 ns | 456 B |
| 5 ensures | 175 ns | 760 B |

**Note**: Allocations include error object creation for failed validations.

### Async Operations

Async operations have similar performance characteristics with additional Task overhead:

```csharp
// Async overhead is from Task machinery, not ROP
await GetUserAsync(id)           // ~1,000,000 ns (database call)
    .BindAsync(ProcessUserAsync)  // + 50 ns (ROP overhead)
    .TapAsync(LogUserAsync);      // + 20 ns (ROP overhead)
```

The ROP overhead is **less than 0.01%** of typical async I/O operations.

## Real-World Context

To put these numbers in perspective:

```
Database Query:    1,000,000 ns (1 ms)
HTTP Request:     10,000,000 ns (10 ms)
File Read:         5,000,000 ns (5 ms)
ROP Chain (5 ops):        150 ns (0.00015 ms)
                          ↑
                     0.015% of a database query
```

**The 16ns ROP overhead is 1/62,500th of a single database query!**

### Performance in Web Applications

In a typical ASP.NET Core request:

```csharp
// Typical web request processing
app.MapPost("/orders", async (CreateOrderRequest request, CancellationToken ct) =>
{
    return await ValidateRequest(request)              // ~50 ns
        .BindAsync((req, ct) => CreateOrderAsync(req, ct), ct)  // ~1-5 ms (DB write)
        .TapAsync((order, ct) => PublishEventAsync(order, ct), ct)  // ~10-50 ms (message queue)
        .MatchAsync(
            onSuccess: order => Results.Created($"/orders/{order.Id}", order),
            onFailure: error => error.ToHttpResult()
        );
});

// Total ROP overhead: ~150 ns
// Total request time: ~15-60 ms
// ROP percentage: 0.0003%
```

## Optimization Tips

### 1. Prefer Struct-Based Value Objects

```csharp
// ✅ Good - Struct, no heap allocation
public readonly struct UserId
{
    private readonly Guid _value;
    public UserId(Guid value) => _value = value;
}

// ❌ Worse - Class, heap allocation
public class UserId
{
    public Guid Value { get; }
    public UserId(Guid value) => Value = value;
}
```

### 2. Use ValueTask for Hot Paths

```csharp
// ✅ Good - ValueTask for potentially synchronous completions
public ValueTask<Result<User>> GetUserFromCacheAsync(UserId id)
{
    if (_cache.TryGetValue(id, out var user))
        return ValueTask.FromResult(Result.Success(user));
    
    return new ValueTask<Result<User>>(FetchFromDbAsync(id));
}

// ❌ Allocates Task even when cached
public Task<Result<User>> GetUserFromCacheAsync(UserId id)
{
    if (_cache.TryGetValue(id, out var user))
        return Task.FromResult(Result.Success(user));
    
    return FetchFromDbAsync(id);
}
```

### 3. Combine Before Bind

```csharp
// ✅ Good - Validate all at once
var result = Email.TryCreate(email)
    .Combine(FirstName.TryCreate(firstName))
    .Combine(LastName.TryCreate(lastName))
    .Bind((e, f, l) => CreateUser(e, f, l));

// ❌ Less efficient - Sequential validation
var result = Email.TryCreate(email)
    .Bind(e => FirstName.TryCreate(firstName)
        .Bind(f => LastName.TryCreate(lastName)
            .Bind(l => CreateUser(e, f, l))));
```

### 4. Minimize Allocations in Hot Paths

```csharp
// ✅ Good - Reuse error instances
private static readonly Error InvalidAgeError = 
    Error.Validation("Age must be between 0 and 120");

public Result<Age> ValidateAge(int age)
{
    return age is >= 0 and <= 120
        ? Result.Success(new Age(age))
        : InvalidAgeError;
}

// ❌ Allocates error on every failure
public Result<Age> ValidateAge(int age)
{
    return age is >= 0 and <= 120
        ? Result.Success(new Age(age))
        : Error.Validation("Age must be between 0 and 120");  // New allocation
}
```

### 5. Use ConfigureAwait in Libraries

```csharp
// ✅ Good - For library code
public async Task<Result<User>> GetUserAsync(UserId id)
{
    var user = await _repository.GetByIdAsync(id).ConfigureAwait(false);
    return user.ToResult(Error.NotFound($"User {id} not found"));
}

// ✅ Also fine - For application code (ASP.NET Core)
public async Task<Result<User>> GetUserAsync(UserId id)
{
    var user = await _repository.GetByIdAsync(id);
    return user.ToResult(Error.NotFound($"User {id} not found"));
}
```

### 6. Avoid Excessive Logging in Hot Paths

```csharp
// ❌ Bad - Logs on every success
.Tap(user => _logger.LogDebug("Got user {Id}", user.Id))

// ✅ Good - Log only on failures or important events
.TapError(error => _logger.LogWarning("Failed to get user: {Error}", error))

// ✅ Good - Use structured logging with guards
.Tap(user => 
{
    if (_logger.IsEnabled(LogLevel.Debug))
        _logger.LogDebug("Got user {Id}", user.Id);
})
```

## Benefits Without Sacrifice

Despite the minimal overhead, you get significant benefits:

✅ **Same Memory Usage** - No additional allocations vs imperative code  
⚡ **Blazing Fast** - Single-digit to low double-digit nanosecond overhead  
✅ **Better Code** - Cleaner, more testable, and maintainable  
✅ **Explicit Errors** - Clear error propagation and aggregation  
✅ **Composable** - Chain operations naturally  
✅ **Type Safe** - Compiler-enforced error handling  

## Running Your Own Benchmarks

### Install BenchmarkDotNet

```bash
dotnet add package BenchmarkDotNet
```

### Create a Benchmark

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using FunctionalDdd;

[MemoryDiagnoser]
[ShortRunJob]
public class MyBenchmarks
{
    [Benchmark]
    public Result<int> RopStyle()
    {
        return Result.Success(5)
            .Map(x => x * 2)
            .Ensure(x => x > 0, Error.Validation("Must be positive"))
            .Map(x => x + 10);
    }

    [Benchmark(Baseline = true)]
    public int ImperativeStyle()
    {
        var x = 5;
        x = x * 2;
        if (x <= 0) throw new InvalidOperationException();
        return x + 10;
    }
}

// Run benchmarks
class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<MyBenchmarks>();
    }
}
```

### Run the Benchmark

```bash
dotnet run -c Release --project YourBenchmarkProject
```

### View Full Project Benchmarks

```bash
cd FunctionalDDD
dotnet run --project Benchmark/Benchmark.csproj -c Release
```

## Performance FAQs

### Q: Is ROP slower than exceptions?

**A:** For the error path, ROP is typically **faster** than exceptions:
- Exception throw: ~1,000-10,000 ns
- ROP error return: ~90-150 ns

### Q: Should I worry about the 16ns overhead?

**A:** No, unless you're in a **tight CPU-bound loop**. For typical web applications with database/HTTP calls, the overhead is **0.002%** or less.

### Q: What about memory pressure?

**A:** ROP has **identical** memory allocations to imperative code. The Result struct is stack-allocated in most cases.

### Q: How does async affect performance?

**A:** Async overhead comes from the Task machinery (~50-100 ns), not ROP. ROP adds the same ~15 ns overhead on top.

### Q: Can I use ROP in high-performance scenarios?

**A:** Yes! The overhead is minimal. Many high-throughput systems use ROP successfully. Profile your specific use case if concerned.
