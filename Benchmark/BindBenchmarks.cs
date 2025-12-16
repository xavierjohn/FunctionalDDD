namespace Benchmark;

using BenchmarkDotNet.Attributes;
using FunctionalDdd;

/// <summary>
/// Benchmarks for Bind operation testing various chain depths and success/failure scenarios.
/// Bind is critical for railway-oriented programming as it chains operations that can fail.
/// </summary>
[MemoryDiagnoser]
public class BindBenchmarks
{
    private Result<int> _successResult = default!;
    private Result<int> _failureResult = default!;

    [GlobalSetup]
    public void Setup()
    {
        _successResult = Result.Success(42);
        _failureResult = Result.Failure<int>(Error.Validation("Test error"));
    }

    [Benchmark(Baseline = true)]
    public Result<int> Bind_SingleChain_Success()
    {
        return _successResult
            .Bind(x => Result.Success(x * 2));
    }

    [Benchmark]
    public Result<int> Bind_SingleChain_Failure()
    {
        return _failureResult
            .Bind(x => Result.Success(x * 2));
    }

    [Benchmark]
    public Result<int> Bind_ThreeChains_AllSuccess()
    {
        return _successResult
            .Bind(x => Result.Success(x * 2))
            .Bind(x => Result.Success(x + 10))
            .Bind(x => Result.Success(x - 5));
    }

    [Benchmark]
    public Result<int> Bind_ThreeChains_FailAtFirst()
    {
        return _failureResult
            .Bind(x => Result.Success(x * 2))
            .Bind(x => Result.Success(x + 10))
            .Bind(x => Result.Success(x - 5));
    }

    [Benchmark]
    public Result<int> Bind_ThreeChains_FailAtSecond()
    {
        return _successResult
            .Bind(x => Result.Failure<int>(Error.Validation("Failed at step 2")))
            .Bind(x => Result.Success(x + 10))
            .Bind(x => Result.Success(x - 5));
    }

    [Benchmark]
    public Result<string> Bind_TypeTransformation()
    {
        return _successResult
            .Bind(x => Result.Success(x.ToString()))
            .Bind(s => Result.Success($"Value: {s}"));
    }

    [Benchmark]
    public Result<int> Bind_FiveChains_Success()
    {
        return _successResult
            .Bind(x => Result.Success(x + 1))
            .Bind(x => Result.Success(x * 2))
            .Bind(x => Result.Success(x + 10))
            .Bind(x => Result.Success(x - 5))
            .Bind(x => Result.Success(x / 2));
    }

    [Benchmark]
    public Result<int> Bind_WithComplexOperation_Success()
    {
        return _successResult
            .Bind(x => ComputeComplexOperation(x))
            .Bind(x => ValidateResult(x))
            .Bind(x => TransformResult(x));
    }

    [Benchmark]
    public Result<int> Bind_WithComplexOperation_Failure()
    {
        return _failureResult
            .Bind(x => ComputeComplexOperation(x))
            .Bind(x => ValidateResult(x))
            .Bind(x => TransformResult(x));
    }

    private static Result<int> ComputeComplexOperation(int value)
    {
        var result = value * value + value - 10;
        return Result.Success(result);
    }

    private static Result<int> ValidateResult(int value)
    {
        return value > 0 
            ? Result.Success(value) 
            : Result.Failure<int>(Error.Validation("Value must be positive"));
    }

    private static Result<int> TransformResult(int value)
    {
        return Result.Success(value % 100);
    }
}
