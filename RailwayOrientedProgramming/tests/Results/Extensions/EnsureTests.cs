namespace RailwayOrientedProgramming.Tests.Results.Extensions;

using FunctionalDDD;

public class EnsureTests
{
    [Fact]
    public void Ensure_source_result_is_failure_predicate_do_not_invoked_expect_is_result_failure()
    {
        var sut = Result.Failure<string>(Error.Unexpected("some error"));

        var result = sut.Ensure(() => true, Error.Unexpected("can't be this error"));

        result.Should().Be(sut);
    }

    [Fact]
    public void Ensure_source_result_is_success_predicate_is_failed_expected_result_failure()
    {
        var sut = Result.Success("Hello");

        var result = sut.Ensure(() => false, Error.Unexpected("predicate failed"));

        result.Should().NotBe(sut);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("predicate failed"));
    }

    [Fact]
    public void Ensure_source_result_is_success_predicate_is_passed_expected_result_success()
    {
        var sut = Result.Success("Hello");

        var result = sut.Ensure(() => true, Error.Unexpected("can't be this error"));

        result.Should().Be(sut);
    }


    [Fact]
    public async Task Ensure_source_result_is_failure_async_predicate_do_not_invoked_expect_is_result_failure()
    {
        var sut = Result.Failure<string>(Error.Unexpected("some error"));

        var result = await sut.EnsureAsync(() => Task.FromResult(true), Error.Unexpected("can't be this error"));

        result.Should().Be(sut);
    }


    [Fact]
    public async Task Ensure_source_result_is_success_async_predicate_is_failed_expected_result_failure()
    {
        var sut = Result.Success("Hello");

        var result = await sut.EnsureAsync(() => Task.FromResult(false), Error.Unexpected("predicate problems"));

        result.Should().NotBe(sut);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("predicate problems"));
    }


    [Fact]
    public async Task Ensure_source_result_is_success_async_predicate_is_passed_expected_result_success()
    {
        var sut = Result.Success("Hello");

        var result = await sut.EnsureAsync(() => Task.FromResult(true), Error.Unexpected("can't be this error"));

        result.Should().Be(sut);
    }


    [Fact]
    public async Task Ensure_task_source_result_is_success_predicate_is_passed_error_predicate_is_not_invoked()
    {
        var sut = Task.FromResult(Result.Success<int?>(default(int)));

        var result = await sut.EnsureAsync(value => !value.HasValue,
            value => Task.FromResult((Error)Error.Unexpected($"should be null but found {value}")));
        result.Should().Be(sut.Result);
    }


    [Fact]
    public async Task Ensure_task_source_result_is_failure_predicate_do_not_invoked_expect_is_result_failure()
    {
        var sut = Task.FromResult(Result.Failure<string>(Error.Unexpected("some error")));

        var result = await sut.EnsureAsync(() => true, Error.Unexpected("can't be this error"));

        result.Should().Be(sut.Result);
    }

    [Fact]
    public void Ensure_generic_source_result_is_failure_predicate_do_not_invoked_expect_is_error_result_failure()
    {
        var sut = Result.Failure<TimeSpan>(Error.Unexpected("some error"));

        var result = sut.Ensure(time => true, Error.Unexpected("test error"));

        result.Should().Be(sut);
    }

    [Fact]
    public void Ensure_generic_source_result_is_success_predicate_is_failed_expected_error_result_failure()
    {
        var sut = Result.Success<int>(10101);

        var result = sut.Ensure(i => false, Error.Unexpected("test error"));

        result.Should().NotBe(sut);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("test error"));
    }

    [Fact]
    public void Ensure_generic_source_result_is_success_predicate_is_passed_expected_error_result_success()
    {
        var sut = Result.Success<decimal>(.03m);

        var result = sut.Ensure(d => true, Error.Unexpected("test error"));

        result.Should().Be(sut);
    }


    [Fact]
    public async Task
    Ensure_generic_source_result_is_failure_async_predicate_do_not_invoked_expect_is_error_result_failure()
    {
        var sut = Result.Failure<DateTimeOffset>(Error.Unexpected("test error"));

        var result = await sut.EnsureAsync(d => Task.FromResult(true), Error.Unexpected("test ensure error"));

        result.Should().Be(sut);
    }


    [Fact]
    public async Task
    Ensure_generic_source_result_is_success_async_predicate_is_failed_expected_error_result_failure()
    {
        var sut = Result.Success<int>(333);

        var result = await sut.EnsureAsync(i => Task.FromResult(false), Error.Unexpected("test ensure error"));

        result.Should().NotBe(sut);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("test ensure error"));
    }


    [Fact]
    public async Task
    Ensure_generic_source_result_is_success_async_predicate_is_passed_expected_error_result_success()
    {
        var sut = Result.Success<decimal>(.33m);

        var result = await sut.EnsureAsync(d => Task.FromResult(true), Error.Unexpected("test error"));

        result.Should().Be(sut);
    }

    [Fact]
    public async Task
    Ensure_generic_task_source_result_is_failure_async_predicate_do_not_invoked_expect_is_error_result_failure()
    {
        var sut = Task.FromResult(Result.Failure<TimeSpan>(Error.Unexpected("some result error")));

        var result = await sut.EnsureAsync(t => Task.FromResult(true), Error.Unexpected("test ensure error"));

        result.Should().Be(sut.Result);
    }


    [Fact]
    public async Task
    Ensure_generic_task_source_result_is_success_async_predicate_is_failed_expected_error_result_failure()
    {
        var sut = Task.FromResult(Result.Success<long>(333));

        var result = await sut.EnsureAsync(l => Task.FromResult(false), Error.Unexpected("test ensure error"));

        result.Should().NotBe(sut);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("test ensure error"));
    }


    [Fact]
    public async Task
    Ensure_generic_task_source_result_is_success_async_predicate_is_passed_expected_error_result_success()
    {
        var sut = Task.FromResult(Result.Success<double>(.33));

        var result = await sut.EnsureAsync(d => Task.FromResult(true), Error.Unexpected("test error"));

        result.Should().Be(sut.Result);
    }

    [Fact]
    public async Task
    Ensure_generic_task_source_result_is_failure_predicate_do_not_invoked_expect_is_error_result_failure()
    {
        var sut = Task.FromResult(Result.Failure<TimeSpan>(Error.Unexpected("some result error")));

        var result = await sut.EnsureAsync(t => true, Error.Unexpected("test ensure error"));

        result.Should().Be(sut.Result);
    }


    [Fact]
    public void Ensure_with_successInput_and_successPredicate()
    {
        var initialResult = Result.Success("Initial message");

        var result = initialResult.Ensure(() => Result.Success<string>("Success message"));

        result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded");
        result.Value.Should().Be("Initial message");
    }

    [Fact]
    public void Ensure_with_successInput_and_failurePredicate()
    {
        var initialResult = Result.Success("Initial Result");

        var result = initialResult.Ensure(() => Result.Failure<string>(Error.Unexpected("Error message")));

        result.IsSuccess.Should().BeFalse("Predicate is failure result");
        result.Error.Should().Be(Error.Unexpected("Error message"));
    }

    [Fact]
    public void Ensure_with_failureInput_and_successPredicate()
    {
        var initialResult = Result.Failure<string>(Error.Unexpected("Initial Error message"));

        var result = initialResult.Ensure(() => Result.Success<string>("Success message"));

        result.IsSuccess.Should().BeFalse("Initial result is failure result");
        result.Error.Should().Be(Error.Unexpected("Initial Error message"));
    }

    [Fact]
    public void Ensure_with_failureInput_and_failurePredicate()
    {
        var initialResult = Result.Failure<string>(Error.Unexpected("Initial Error message"));

        var result = initialResult.Ensure(() => Result.Failure<string>(Error.Unexpected("Error message")));

        result.IsSuccess.Should().BeFalse("Initial result is failure result");
        result.Error.Should().Be(Error.Unexpected("Initial Error message"));
    }

    [Fact]
    public void Ensure_with_successInput_and_parameterisedFailurePredicate()
    {
        var initialResult = Result.Success("Initial Success message");

        var result = initialResult.Ensure(_ => Result.Failure<string>(Error.Unexpected("Error Message")));

        result.IsSuccess.Should().BeFalse("Predicate is failure result");
        result.Error.Should().Be(Error.Unexpected("Error Message"));
    }

    [Fact]
    public void Ensure_with_successInput_and_parameterisedSuccessPredicate()
    {
        var initialResult = Result.Success("Initial Success message");

        var result = initialResult.Ensure(_ => Result.Success<string>("Success Message"));

        result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded");
        ;
        result.Value.Should().Be("Initial Success message");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public void Ensure_string_is_not_whitespace(string? str)
    {
        var result = str.EnsureNotNullOrWhiteSpace(Error.Unexpected("test error"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Unexpected("test error"));
    }
}
