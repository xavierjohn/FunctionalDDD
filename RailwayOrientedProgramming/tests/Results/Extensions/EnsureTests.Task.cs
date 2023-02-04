namespace RailwayOrientedProgramming.Tests.Functional.Results.Extensions;

using FunctionalDDD;

public class Ensure_Task_Tests
{
    [Fact]
    public async Task Ensure_Task_with_successInput_and_successPredicate()
    {
        var initialResult = Task.FromResult(Result.Success<string, Error>("Initial message"));

        var result = await initialResult.EnsureAsync(() => Task.FromResult(Result.Success<string, Error>("Success message")));

        result.IsOk.Should().BeTrue("Initial result and predicate succeeded");
        result.Ok.Should().Be("Initial message");
    }

    [Fact]
    public async Task Ensure_Task_with_successInput_and_failurePredicate()
    {
        var initialResult = Task.FromResult(Result.Success<string, Error>("Initial Result"));

        var result = await initialResult.EnsureAsync(() => Task.FromResult(Result.Failure<string, Error>(Error.Unexpected("Error message"))));

        result.IsOk.Should().BeFalse("Predicate is failure result");
        result.Error.Should().Be(Error.Unexpected("Error message"));
    }

    [Fact]
    public async Task Ensure_Task_with_failureInput_and_successPredicate()
    {
        var initialResult = Task.FromResult(Result.Failure<string, Error>(Error.Unauthorized("Initial Error message")));

        var result = await initialResult.EnsureAsync(() => Task.FromResult(Result.Success<string, Error>("Success message")));

        result.IsOk.Should().BeFalse("Initial result is failure result");
        result.Error.Should().Be(Error.Unauthorized("Initial Error message"));
    }

    [Fact]
    public async Task Ensure_Task_with_failureInput_and_failurePredicate()
    {
        var initialResult = Task.FromResult(Result.Failure<string, Error>(Error.Validation("Initial Error message")));

        var result = await initialResult.EnsureAsync(() => Task.FromResult(Result.Failure<string, Error>(Error.Unauthorized("Error message"))));

        result.IsOk.Should().BeFalse("Initial result is failure result");
        result.Error.Should().Be(Error.Validation("Initial Error message"));
    }

    [Fact]
    public async Task Ensure_Task_with_successInput_and_parameterisedSuccessPredicate()
    {
        var initialResult = Task.FromResult(Result.Success<string, Error>("Initial Success message"));

        var result = await initialResult.EnsureAsync(_ => Task.FromResult(Result.Success<string, Error>("Success Message")));

        result.IsOk.Should().BeTrue("Initial result and predicate succeeded"); ;
        result.Ok.Should().Be("Initial Success message");
    }

    [Fact]
    public async Task Ensure_Task_with_failureInput_and_parameterisedSuccessPredicate()
    {
        var initialResult = Task.FromResult(Result.Failure<string, Error>(Error.Conflict("Initial Error message")));

        var result = await initialResult.EnsureAsync(_ => Task.FromResult(Result.Success<string, Error>("Success Message")));

        result.IsOk.Should().BeFalse("Initial result is failure result"); ;
        result.Error.Should().Be(Error.Conflict("Initial Error message"));
    }

    [Fact]
    public async Task Ensure_Task_with_failureInput_and_parameterisedFailurePredicate()
    {
        var initialResult = Task.FromResult(Result.Failure<string, Error>(Error.Conflict("Initial Error message")));

        var result = await initialResult.EnsureAsync(_ => Task.FromResult(Result.Failure<string, Error>(Error.Unexpected("Success Message"))));

        result.IsOk.Should().BeFalse("Initial result and predicate is failure result"); ;
        result.Error.Should().Be(Error.Conflict("Initial Error message"));
    }
}
