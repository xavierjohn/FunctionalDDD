namespace RailwayOrientedProgramming.Tests.Functional.Results.Extensions;

using FunctionalDDD;

public class EnsureTests_Task_Left
{
    [Fact]
    public async Task Ensure_Task_Left_with_errorPredicate_does_not_throw_when_given_result_failure()
    {
        var result = Task.FromResult(Result.Failure<string>(Err.Conflict("initial error message")));
        Func<Task> ensure = () => result.EnsureAsync(
            x => x != "",
            x => Err.Validation("new error message: string should not be empty"));

        await ensure.Should().NotThrowAsync<Exception>("passing in a Result.Failure is a valid use case");
    }

    [Fact]
    public async Task Ensure_Task_Left_with_errorPredicate_initial_result_has_failure_state()
    {
        var tResult = Task.FromResult(Result.Failure<string>(Err.Conflict("initial error message")));

        var result = await tResult.EnsureAsync(x => x != "",
            x => Err.Validation("new error message: string should not be empty"));

        result.IsSuccess.Should().BeFalse("Input Result.Failure should be returned");
        result.Error.Should().Be(Err.Conflict("initial error message"));
    }

    [Fact]
    public async Task Ensure_Task_Left_with_errorPredicate_predicate_passes()
    {
        var tResult = Task.FromResult(Result.Success("initial ok"));

        var result = await tResult.EnsureAsync(x => x != "",
            x => Err.Validation("new error message: string should not be empty"));

        result.IsSuccess.Should().BeTrue("Input Result passes predicate condition");
        result.Ok.Should().Be("initial ok");
    }

    [Fact]
    public async Task Ensure_Task_Left_with_errorPredicate_does_not_execute_error_predicate_when_predicate_passes()
    {
        var tResult = Task.FromResult(Result.Success<int?>(null));

        Result<int?> result = await tResult.EnsureAsync(value => !value.HasValue, value => Err.Validation($"should be null but found {value}"));

        result.Should().Be(tResult.Result);
    }

    [Fact]
    public async Task Ensure_Task_Left_with_errorPredicate_using_errorPredicate()
    {
        var tResult = Task.FromResult(Result.Success(""));

        var result = await tResult.EnsureAsync(x => x != "",
            x => Err.Validation("new error message: string should not be empty"));

        result.IsSuccess.Should().BeFalse("Input Result fails predicate condition");
        result.Error.Should().Be(Err.Validation("new error message: string should not be empty"));
    }

    [Fact]
    public async Task Ensure_Task_Left_with_successInput_and_successPredicate()
    {
        var initialResult = Task.FromResult(Result.Success("Initial message"));

        var result = await initialResult.EnsureAsync(() => Result.Success("Success message"));

        result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded");
        result.Ok.Should().Be("Initial message");
    }

    [Fact]
    public async Task Ensure_Task_Left_with_successInput_and_failurePredicate()
    {
        var initialResult = Task.FromResult(Result.Success("Initial Result"));

        var result = await initialResult.EnsureAsync(() => Result.Failure<string>(Err.Unexpected("Error message")));

        result.IsSuccess.Should().BeFalse("Predicate is failure result");
        result.Error.Should().Be(Err.Unexpected("Error message"));
    }

    [Fact]
    public async Task Ensure_Task_Left_with_failureInput_and_successPredicate()
    {
        var initialResult = Task.FromResult(Result.Failure<string>(Err.Unexpected("Initial Error message")));

        var result = await initialResult.EnsureAsync(() => Result.Success("Success message"));

        result.IsSuccess.Should().BeFalse("Initial result is failure result");
        result.Error.Should().Be(Err.Unexpected("Initial Error message"));
    }

    [Fact]
    public async Task Ensure_Task_Left_with_failureInput_and_failurePredicate()
    {
        var initialResult = Task.FromResult(Result.Failure<string>(Err.Unexpected("Initial Error message")));

        var result = await initialResult.EnsureAsync(() => Result.Failure<string>(Err.Unexpected("Error message")));

        result.IsSuccess.Should().BeFalse("Initial result is failure result");
        result.Error.Should().Be(Err.Unexpected("Initial Error message"));
    }

    [Fact]
    public async Task Ensure_Task_Left_with_successInput_and_parameterisedFailurePredicate()
    {
        var initialResult = Task.FromResult(Result.Success("Initial Success message"));

        var result = await initialResult.EnsureAsync(_ => Result.Failure<string>(Err.Unexpected("Error Message")));

        result.IsSuccess.Should().BeFalse("Predicate is failure result");
        result.Error.Should().Be(Err.Unexpected("Error Message"));
    }

    [Fact]
    public async Task Ensure_Task_Left_with_successInput_and_parameterisedSuccessPredicate()
    {
        var initialResult = Task.FromResult(Result.Success("Initial Success message"));

        var result = await initialResult.EnsureAsync(_ => Result.Success("Success Message"));

        result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded"); ;
        result.Ok.Should().Be("Initial Success message");
    }

    [Fact]
    public async Task Ensure_Task_Left_with_failureInput_and_parameterisedSuccessPredicate()
    {
        var initialResult = Task.FromResult(Result.Failure<string>(Err.Unexpected("Initial Error message")));

        var result = await initialResult.EnsureAsync(_ => Result.Success("Success Message"));

        result.IsSuccess.Should().BeFalse("Initial result is failure result"); ;
        result.Error.Should().Be(Err.Unexpected("Initial Error message"));
    }

    [Fact]
    public async Task Ensure_Task_Left_with_failureInput_and_parameterisedFailurePredicate()
    {
        var initialResult = Task.FromResult(Result.Failure<string>(Err.Unexpected("Initial Error message")));

        var result = await initialResult.EnsureAsync(_ => Result.Failure<string>(Err.NotFound("Success Message")));

        result.IsSuccess.Should().BeFalse("Initial result and predicate is failure result"); ;
        result.Error.Should().Be(Err.Unexpected("Initial Error message"));
    }
}
