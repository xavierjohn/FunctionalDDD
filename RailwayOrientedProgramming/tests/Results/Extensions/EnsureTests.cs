namespace RailwayOrientedProgramming.Tests.Results.Extensions;

using FunctionalDDD;

public class EnsureTests
{
    [Fact]
    public void Ensure_source_result_is_failure_predicate_do_not_invoked_expect_is_result_failure()
    {
        var sut = Result.Failure<string, Error>(Error.Unexpected("some error"));

        var result = sut.Ensure(() => true, Error.Unexpected("can't be this error"));

        result.Should().Be(sut);
    }

    [Fact]
    public void Ensure_source_result_is_success_predicate_is_failed_expected_result_failure()
    {
        var sut = Result.Success<string, Error>("Hello");

        var result = sut.Ensure(() => false, Error.Unexpected("predicate failed"));

        result.Should().NotBe(sut);
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("predicate failed"));
    }

    [Fact]
    public void Ensure_source_result_is_success_predicate_is_passed_expected_result_success()
    {
        var sut = Result.Success<string, Error>("Hello");

        var result = sut.Ensure(() => true, Error.Unexpected("can't be this error"));

        result.Should().Be(sut);
    }


    [Fact]
    public async Task Ensure_source_result_is_failure_async_predicate_do_not_invoked_expect_is_result_failure()
    {
        var sut = Result.Failure<string, Error>(Error.Unexpected("some error"));

        var result = await sut.EnsureAsync(() => Task.FromResult(true), Error.Unexpected("can't be this error"));

        result.Should().Be(sut);
    }


    [Fact]
    public async Task Ensure_source_result_is_success_async_predicate_is_failed_expected_result_failure()
    {
        var sut = Result.Success<string, Error>("Hello");

        var result = await sut.EnsureAsync(() => Task.FromResult(false), Error.Unexpected("predicate problems"));

        result.Should().NotBe(sut);
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("predicate problems"));
    }


    [Fact]
    public async Task Ensure_source_result_is_success_async_predicate_is_passed_expected_result_success()
    {
        var sut = Result.Success<string, Error>("Hello");

        var result = await sut.EnsureAsync(() => Task.FromResult(true), Error.Unexpected("can't be this error"));

        result.Should().Be(sut);
    }


    [Fact]
    public async Task Ensure_task_source_result_is_success_predicate_is_passed_error_predicate_is_not_invoked()
    {
        var sut = Task.FromResult(Result.Success<int?, Error>(null));

        var result = await sut.EnsureAsync(value => !value.HasValue,
            value => Task.FromResult(Error.Unexpected($"should be null but found {value}")));
        result.Should().Be(sut.Result);
    }


    [Fact]
    public async Task Ensure_task_source_result_is_failure_predicate_do_not_invoked_expect_is_result_failure()
    {
        var sut = Task.FromResult(Result.Failure<string, Error>(Error.Unexpected("some error")));

        var result = await sut.EnsureAsync(() => true, Error.Unexpected("can't be this error"));

        result.Should().Be(sut.Result);
    }

    [Fact]
    public void Ensure_generic_source_result_is_failure_predicate_do_not_invoked_expect_is_error_result_failure()
    {
        var sut = Result.Failure<TimeSpan, Error>(Error.Unexpected("some error"));

        var result = sut.Ensure(time => true, Error.Unexpected("test error"));

        result.Should().Be(sut);
    }

    [Fact]
    public void Ensure_generic_source_result_is_success_predicate_is_failed_expected_error_result_failure()
    {
        var sut = Result.Success<int, Error>(10101);

        var result = sut.Ensure(i => false, Error.Unexpected("test error"));

        result.Should().NotBe(sut);
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("test error"));
    }

    [Fact]
    public void Ensure_generic_source_result_is_success_predicate_is_passed_expected_error_result_success()
    {
        var sut = Result.Success<decimal, Error>(.03m);

        var result = sut.Ensure(d => true, Error.Unexpected("test error"));

        result.Should().Be(sut);
    }


    [Fact]
    public async Task
    Ensure_generic_source_result_is_failure_async_predicate_do_not_invoked_expect_is_error_result_failure()
    {
        var sut = Result.Failure<DateTimeOffset, Error>(Error.Unexpected("test error"));

        var result = await sut.EnsureAsync(d => Task.FromResult(true), Error.Unexpected("test ensure error"));

        result.Should().Be(sut);
    }


    [Fact]
    public async Task
    Ensure_generic_source_result_is_success_async_predicate_is_failed_expected_error_result_failure()
    {
        var sut = Result.Success<int, Error>(333);

        var result = await sut.EnsureAsync(i => Task.FromResult(false), Error.Unexpected("test ensure error"));

        result.Should().NotBe(sut);
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("test ensure error"));
    }


    [Fact]
    public async Task
    Ensure_generic_source_result_is_success_async_predicate_is_passed_expected_error_result_success()
    {
        var sut = Result.Success<decimal, Error>(.33m);

        var result = await sut.EnsureAsync(d => Task.FromResult(true), Error.Unexpected("test error"));

        result.Should().Be(sut);
    }

    [Fact]
    public async Task
    Ensure_generic_task_source_result_is_failure_async_predicate_do_not_invoked_expect_is_error_result_failure()
    {
        var sut = Task.FromResult(Result.Failure<TimeSpan, Error>(Error.Unexpected("some result error")));

        var result = await sut.EnsureAsync(t => Task.FromResult(true), Error.Unexpected("test ensure error"));

        result.Should().Be(sut.Result);
    }


    [Fact]
    public async Task
    Ensure_generic_task_source_result_is_success_async_predicate_is_failed_expected_error_result_failure()
    {
        var sut = Task.FromResult(Result.Success<long, Error>(333));

        var result = await sut.EnsureAsync(l => Task.FromResult(false), Error.Unexpected("test ensure error"));

        result.Should().NotBe(sut);
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("test ensure error"));
    }


    [Fact]
    public async Task
    Ensure_generic_task_source_result_is_success_async_predicate_is_passed_expected_error_result_success()
    {
        var sut = Task.FromResult(Result.Success<double, Error>(.33));

        var result = await sut.EnsureAsync(d => Task.FromResult(true), Error.Unexpected("test error"));

        result.Should().Be(sut.Result);
    }

    [Fact]
    public async Task
    Ensure_generic_task_source_result_is_failure_predicate_do_not_invoked_expect_is_error_result_failure()
    {
        var sut = Task.FromResult(Result.Failure<TimeSpan, Error>(Error.Unexpected("some result error")));

        var result = await sut.EnsureAsync(t => true, Error.Unexpected("test ensure error"));

        result.Should().Be(sut.Result);
    }


    [Fact]
    public void Ensure_with_successInput_and_successPredicate()
    {
        var initialResult = Result.Success<string, Error>("Initial message");

        var result = initialResult.Ensure(() => Result.Success<string, Error>("Success message"));

        result.IsOk.Should().BeTrue("Initial result and predicate succeeded");
        result.Ok.Should().Be("Initial message");
    }

    [Fact]
    public void Ensure_with_successInput_and_failurePredicate()
    {
        var initialResult = Result.Success<string, Error>("Initial Result");

        var result = initialResult.Ensure(() => Result.Failure<string, Error>(Error.Unexpected("Error message")));

        result.IsOk.Should().BeFalse("Predicate is failure result");
        result.Error.Should().Be(Error.Unexpected("Error message"));
    }

    [Fact]
    public void Ensure_with_failureInput_and_successPredicate()
    {
        var initialResult = Result.Failure<string, Error>(Error.Unexpected("Initial Error message"));

        var result = initialResult.Ensure(() => Result.Success<string, Error>("Success message"));

        result.IsOk.Should().BeFalse("Initial result is failure result");
        result.Error.Should().Be(Error.Unexpected("Initial Error message"));
    }

    [Fact]
    public void Ensure_with_failureInput_and_failurePredicate()
    {
        var initialResult = Result.Failure<string, Error>(Error.Unexpected("Initial Error message"));

        var result = initialResult.Ensure(() => Result.Failure<string, Error>(Error.Unexpected("Error message")));

        result.IsOk.Should().BeFalse("Initial result is failure result");
        result.Error.Should().Be(Error.Unexpected("Initial Error message"));
    }

    [Fact]
    public void Ensure_with_successInput_and_parameterisedFailurePredicate()
    {
        var initialResult = Result.Success<string, Error>("Initial Success message");

        var result = initialResult.Ensure(_ => Result.Failure<string, Error>(Error.Unexpected("Error Message")));

        result.IsOk.Should().BeFalse("Predicate is failure result");
        result.Error.Should().Be(Error.Unexpected("Error Message"));
    }

    [Fact]
    public void Ensure_with_successInput_and_parameterisedSuccessPredicate()
    {
        var initialResult = Result.Success<string, Error>("Initial Success message");

        var result = initialResult.Ensure(_ => Result.Success<string, Error>("Success Message"));

        result.IsOk.Should().BeTrue("Initial result and predicate succeeded");
        ;
        result.Ok.Should().Be("Initial Success message");
    }
}
