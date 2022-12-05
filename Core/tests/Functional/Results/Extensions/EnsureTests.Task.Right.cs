﻿namespace FunctionalDDD.Core.Tests.Functional.Results.Extensions;

public class EnsureTests_Task_Right
{
    [Fact]
    public async Task Ensure_Task_Right_with_successInput_and_successPredicate()
    {
        var initialResult = Result.Success("Initial message");

        var result = await initialResult.EnsureAsync(() => Task.FromResult(Result.Success("Success message")));

        result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded");
        result.Value.Should().Be("Initial message");
    }

    [Fact]
    public async Task Ensure_Task_Right_with_successInput_and_failurePredicate()
    {
        var initialResult = Result.Success("Initial Result");

        var result = await initialResult.EnsureAsync(() => Task.FromResult(Result.Failure<string>(Error.Unauthorized("Error message"))));

        result.IsSuccess.Should().BeFalse("Predicate is failure result");
        result.Error.Should().Be(Error.Unauthorized("Error message"));
    }

    [Fact]
    public async Task Ensure_Task_Right_with_failureInput_and_successPredicate()
    {
        var initialResult = Result.Failure<string>(Error.Conflict("Initial Error message"));

        var result = await initialResult.EnsureAsync(() => Task.FromResult(Result.Success("Success message")));

        result.IsSuccess.Should().BeFalse("Initial result is failure result");
        result.Error.Should().Be(Error.Conflict("Initial Error message"));
    }

    [Fact]
    public async Task Ensure_Task_Right_with_failureInput_and_failurePredicate()
    {
        var initialResult = Result.Failure<string>(Error.NotFound("Initial Error message"));

        var result = await initialResult.EnsureAsync(() => Task.FromResult(Result.Failure<string>(Error.Unauthorized("Error message"))));

        result.IsSuccess.Should().BeFalse("Initial result is failure result");
        result.Error.Should().Be(Error.NotFound("Initial Error message"));
    }

    [Fact]
    public async Task Ensure_Task_Right_with_successInput_and_parameterisedFailurePredicate()
    {
        var initialResult = Result.Success("Initial Success message");

        var result = await initialResult.EnsureAsync(_ => Task.FromResult(Result.Failure<string>(Error.Conflict("Error Message"))));

        result.IsSuccess.Should().BeFalse("Predicate is failure result");
        result.Error.Should().Be(Error.Conflict("Error Message"));
    }

    [Fact]
    public async Task Ensure_Task_Right_with_successInput_and_parameterisedSuccessPredicate()
    {
        var initialResult = Result.Success("Initial Success message");

        var result = await initialResult.EnsureAsync(_ => Task.FromResult(Result.Success("Success Message")));

        result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded"); ;
        result.Value.Should().Be("Initial Success message");
    }

    [Fact]
    public async Task Ensure_Task_Right_with_failureInput_and_parameterisedSuccessPredicate()
    {
        var initialResult = Result.Failure<string>(Error.Unexpected("Initial Error message"));

        var result = await initialResult.EnsureAsync(_ => Task.FromResult(Result.Success("Success Message")));

        result.IsSuccess.Should().BeFalse("Initial result is failure result"); ;
        result.Error.Should().Be(Error.Unexpected("Initial Error message"));
    }

    [Fact]
    public async Task Ensure_Task_Right_with_failureInput_and_parameterisedFailurePredicate()
    {
        var initialResult = Result.Failure<string>(Error.Unexpected("Initial Error message"));

        var result = await initialResult.EnsureAsync(_ => Task.FromResult(Result.Failure<string>(Error.Unexpected("Success Message"))));

        result.IsSuccess.Should().BeFalse("Initial result and predicate is failure result"); ;
        result.Error.Should().Be(Error.Unexpected("Initial Error message"));
    }
}
