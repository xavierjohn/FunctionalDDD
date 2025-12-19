namespace Benchmark;

using BenchmarkDotNet.Attributes;
using FunctionalDdd;

/// <summary>
/// Benchmarks for Compensate operation testing error recovery and fallback mechanisms.
/// Compensate is used to provide alternative paths when operations fail.
/// </summary>
[MemoryDiagnoser]
public class CompensateBenchmarks
{
    private Result<int> _successResult = default!;
    private Result<int> _failureResult = default!;
    private Result<int> _notFoundFailure = default!;
    private Result<int> _validationFailure = default!;

    [GlobalSetup]
    public void Setup()
    {
        _successResult = Result.Success(42);
        _failureResult = Result.Failure<int>(Error.Unexpected("Unexpected error"));
        _notFoundFailure = Result.Failure<int>(Error.NotFound("Resource not found"));
        _validationFailure = Result.Failure<int>(Error.Validation("Validation failed"));
    }

    [Benchmark(Baseline = true)]
    public Result<int> Compensate_OnSuccess()
    {
        return _successResult
            .Compensate(() => Result.Success(100));
    }

    [Benchmark]
    public Result<int> Compensate_OnFailure()
    {
        return _failureResult
            .Compensate(() => Result.Success(100));
    }

    [Benchmark]
    public Result<int> Compensate_OnFailure_WithErrorAccess()
    {
        return _failureResult
            .Compensate(error => Result.Success(100));
    }

    [Benchmark]
    public Result<int> Compensate_WithPredicate_Match()
    {
        return _notFoundFailure
            .Compensate(
                predicate: error => error is NotFoundError,
                func: () => Result.Success(100));
    }

    [Benchmark]
    public Result<int> Compensate_WithPredicate_NoMatch()
    {
        return _validationFailure
            .Compensate(
                predicate: error => error is NotFoundError,
                func: () => Result.Success(100));
    }

    [Benchmark]
    public Result<int> Compensate_WithPredicate_AndErrorAccess_Match()
    {
        return _notFoundFailure
            .Compensate(
                predicate: error => error is NotFoundError,
                func: error => Result.Success(100));
    }

    [Benchmark]
    public Result<int> Compensate_Chain_TwoLevels()
    {
        return _failureResult
            .Compensate(() => Result.Failure<int>(Error.NotFound("Still failing")))
            .Compensate(() => Result.Success(100));
    }

    [Benchmark]
    public Result<int> Compensate_Chain_ThreeLevels()
    {
        return _failureResult
            .Compensate(() => Result.Failure<int>(Error.NotFound("Fail 1")))
            .Compensate(() => Result.Failure<int>(Error.Validation("Fail 2")))
            .Compensate(() => Result.Success(100));
    }

    [Benchmark]
    public Result<int> Compensate_WithComplexRecovery()
    {
        return _failureResult
            .Compensate(error => RecoverFromError(error));
    }

    [Benchmark]
    public Result<int> Compensate_Multiple_DifferentErrorTypes()
    {
        return _failureResult
            .Compensate(error => error is NotFoundError, () => Result.Success(10))
            .Compensate(error => error is ValidationError, () => Result.Success(20))
            .Compensate(error => error is UnexpectedError, () => Result.Success(30));
    }

    [Benchmark]
    public Result<int> Compensate_WithExpensiveRecovery()
    {
        return _failureResult
            .Compensate(error => PerformExpensiveRecovery());
    }

    [Benchmark]
    public Result<int> Compensate_MixedWithBind_Success()
    {
        return _successResult
            .Bind(x => Result.Success(x * 2))
            .Compensate(() => Result.Success(0))
            .Bind(x => Result.Success(x + 10));
    }

    [Benchmark]
    public Result<int> Compensate_MixedWithBind_Failure()
    {
        return _failureResult
            .Bind(x => Result.Success(x * 2))
            .Compensate(() => Result.Success(0))
            .Bind(x => Result.Success(x + 10));
    }

    [Benchmark]
    public Result<int> Compensate_WithDefaultValue()
    {
        return _failureResult
            .Compensate(() => Result.Success(0));
    }

    [Benchmark]
    public Result<string> Compensate_TypeTransformation()
    {
        var failureString = Result.Failure<string>(Error.NotFound("String not found"));
        return failureString
            .Compensate(() => Result.Success("Default Value"));
    }

    [Benchmark]
    public Result<int> Compensate_NestedPredicates()
    {
        return _failureResult
            .Compensate(
                predicate: error => error is NotFoundError || error is ValidationError,
                func: () => Result.Success(50))
            .Compensate(
                predicate: error => error is UnexpectedError,
                func: () => Result.Success(100));
    }

    private static Result<int> RecoverFromError(Error error)
    {
        return error switch
        {
            NotFoundError => Result.Success(10),
            ValidationError => Result.Success(20),
            UnexpectedError => Result.Success(30),
            _ => Result.Success(0)
        };
    }

    private static Result<int> PerformExpensiveRecovery()
    {
        var sum = 0;
        for (int i = 0; i < 100; i++)
        {
            sum += i * i;
        }

        return Result.Success(sum);
    }
}
