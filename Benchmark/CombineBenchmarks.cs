namespace Benchmark;

using BenchmarkDotNet.Attributes;
using FunctionalDdd;
using FunctionalDdd.PrimitiveValueObjects;
using SampleUserLibrary;

/// <summary>
/// Benchmarks for Combine operation testing parallel result aggregation.
/// Combine is critical for collecting multiple validation results before proceeding.
/// </summary>
[MemoryDiagnoser]
public class CombineBenchmarks
{
    private Result<int> _successInt1 = default!;
    private Result<int> _successInt2 = default!;
    private Result<int> _failureInt = default!;
    private Result<string> _successString = default!;
    private Result<string> _failureString = default!;

    [GlobalSetup]
    public void Setup()
    {
        _successInt1 = Result.Success(42);
        _successInt2 = Result.Success(100);
        _failureInt = Result.Failure<int>(Error.Validation("Invalid number", "number"));
        _successString = Result.Success("Hello");
        _failureString = Result.Failure<string>(Error.Validation("Invalid string", "text"));
    }

    [Benchmark(Baseline = true)]
    public Result<(int, int)> Combine_TwoResults_BothSuccess()
    {
        return _successInt1.Combine(_successInt2);
    }

    [Benchmark]
    public Result<(int, int)> Combine_TwoResults_FirstFailure()
    {
        return _failureInt.Combine(_successInt2);
    }

    [Benchmark]
    public Result<(int, int)> Combine_TwoResults_SecondFailure()
    {
        return _successInt1.Combine(_failureInt);
    }

    [Benchmark]
    public Result<(int, int)> Combine_TwoResults_BothFailure()
    {
        return _failureInt.Combine(_failureInt);
    }

    [Benchmark]
    public Result<(int, int, int)> Combine_ThreeResults_AllSuccess()
    {
        return _successInt1
            .Combine(_successInt2)
            .Combine(Result.Success(200));
    }

    [Benchmark]
    public Result<(int, int, int)> Combine_ThreeResults_OneFailure()
    {
        return _successInt1
            .Combine(_failureInt)
            .Combine(_successInt2);
    }

    [Benchmark]
    public Result<(int, int, int)> Combine_ThreeResults_TwoFailures()
    {
        return _failureInt
            .Combine(_failureInt)
            .Combine(_successInt2);
    }

    [Benchmark]
    public Result<(int, string)> Combine_DifferentTypes_BothSuccess()
    {
        return _successInt1.Combine(_successString);
    }

    [Benchmark]
    public Result<(int, string)> Combine_DifferentTypes_OneFailure()
    {
        return _successInt1.Combine(_failureString);
    }

    [Benchmark]
    public Result<string> Combine_WithBind_AllSuccess()
    {
        return _successInt1
            .Combine(_successInt2)
            .Bind((a, b) => Result.Success($"{a} + {b} = {a + b}"));
    }

    [Benchmark]
    public Result<string> Combine_WithBind_OneFailure()
    {
        return _successInt1
            .Combine(_failureInt)
            .Bind((a, b) => Result.Success($"{a} + {b} = {a + b}"));
    }

    [Benchmark]
    public Result<string> Combine_ValueObjects_AllValid()
    {
        return FirstName.TryCreate("John")
            .Combine(LastName.TryCreate("Doe"))
            .Combine(EmailAddress.TryCreate("john@example.com"))
            .Bind((first, last, email) => Result.Success($"{first} {last} <{email}>"));
    }

    [Benchmark]
    public Result<string> Combine_ValueObjects_OneInvalid()
    {
        return FirstName.TryCreate("John")
            .Combine(LastName.TryCreate(""))
            .Combine(EmailAddress.TryCreate("john@example.com"))
            .Bind((first, last, email) => Result.Success($"{first} {last} <{email}>"));
    }

    [Benchmark]
    public Result<string> Combine_ValueObjects_AllInvalid()
    {
        return FirstName.TryCreate("")
            .Combine(LastName.TryCreate(""))
            .Combine(EmailAddress.TryCreate("invalid-email"))
            .Bind((first, last, email) => Result.Success($"{first} {last} <{email}>"));
    }

    [Benchmark]
    public Result<int> Combine_FiveResults_AllSuccess()
    {
        return Result.Success(1)
            .Combine(Result.Success(2))
            .Combine(Result.Success(3))
            .Combine(Result.Success(4))
            .Combine(Result.Success(5))
            .Bind((a, b, c, d, e) => Result.Success(a + b + c + d + e));
    }

    [Benchmark]
    public Result<int> Combine_FiveResults_OneFailure()
    {
        return Result.Success(1)
            .Combine(Result.Success(2))
            .Combine(Result.Failure<int>(Error.Validation("Error at 3")))
            .Combine(Result.Success(4))
            .Combine(Result.Success(5))
            .Bind((a, b, c, d, e) => Result.Success(a + b + c + d + e));
    }

    [Benchmark]
    public Result<int> Combine_FiveResults_MultipleFailures()
    {
        return Result.Success(1)
            .Combine(Result.Failure<int>(Error.Validation("Error at 2")))
            .Combine(Result.Failure<int>(Error.Validation("Error at 3")))
            .Combine(Result.Success(4))
            .Combine(Result.Failure<int>(Error.Validation("Error at 5")))
            .Bind((a, b, c, d, e) => Result.Success(a + b + c + d + e));
    }

    [Benchmark]
    public async Task<Result<(int, int)>> CombineAsync_TwoResults_BothSuccess()
    {
        return await Task.FromResult(_successInt1)
            .CombineAsync(_successInt2);
    }

    [Benchmark]
    public async Task<Result<int>> CombineAsync_ThreeResults_AllSuccess()
    {
        var result = await Task.FromResult(_successInt1)
            .CombineAsync(_successInt2)
            .CombineAsync(Result.Success(200));

        return result.Map(tuple => tuple.Item1 + tuple.Item2 + tuple.Item3);
    }

    [Benchmark]
    public Result<Person> Combine_ComplexObject_AllValid()
    {
        return FirstName.TryCreate("John")
            .Combine(LastName.TryCreate("Doe"))
            .Combine(EmailAddress.TryCreate("john@example.com"))
            .Combine(Result.Success(30))
            .Bind((first, last, email, age) => Result.Success(new Person(
                $"{first} {last}",
                email.ToString(),
                age)));
    }

    [Benchmark]
    public Result<Person> Combine_ComplexObject_WithValidation()
    {
        return FirstName.TryCreate("John")
            .Combine(LastName.TryCreate("Doe"))
            .Combine(EmailAddress.TryCreate("john@example.com"))
            .Combine(Result.Success(30))
            .Bind((first, last, email, age) => Result.Success(new Person(
                $"{first} {last}",
                email.ToString(),
                age)))
            .Ensure(p => p.Age >= 18, Error.Validation("Must be adult"));
    }

    [Benchmark]
    public Result<Unit> Combine_WithUnit_Success()
    {
        return _successInt1
            .Combine(Result.Success())
            .Map((tuple) => new Unit());
    }

    public record Person(string Name, string Email, int Age);
}