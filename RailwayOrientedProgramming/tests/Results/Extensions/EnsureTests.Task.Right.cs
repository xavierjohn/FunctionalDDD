﻿namespace RailwayOrientedProgramming.Tests.Functional.Results.Extensions;

using FunctionalDDD;

public class EnsureTests_Task_Right
{
    [Fact]
    public async Task Ensure_Task_Right_with_successInput_and_successPredicate()
    {
        var initialResult = Result.Success<string, Err>("Initial message");

        var result = await initialResult.EnsureAsync(() => Task.FromResult(Result.Success<string, Err>("Success message")));

        result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded");
        result.Ok.Should().Be("Initial message");
    }

    [Fact]
    public async Task Ensure_Task_Right_with_successInput_and_failurePredicate()
    {
        var initialResult = Result.Success<string, Err>("Initial Result");

        var result = await initialResult.EnsureAsync(() => Task.FromResult(Result.Failure<string, Err>(Err.Unauthorized("Error message"))));

        result.IsSuccess.Should().BeFalse("Predicate is failure result");
        result.Error.Should().Be(Err.Unauthorized("Error message"));
    }

    [Fact]
    public async Task Ensure_Task_Right_with_failureInput_and_successPredicate()
    {
        var initialResult = Result.Failure<string, Err>(Err.Conflict("Initial Error message"));

        var result = await initialResult.EnsureAsync(() => Task.FromResult(Result.Success<string, Err>("Success message")));

        result.IsSuccess.Should().BeFalse("Initial result is failure result");
        result.Error.Should().Be(Err.Conflict("Initial Error message"));
    }

    [Fact]
    public async Task Ensure_Task_Right_with_failureInput_and_failurePredicate()
    {
        var initialResult = Result.Failure<string, Err>(Err.NotFound("Initial Error message"));

        var result = await initialResult.EnsureAsync(() => Task.FromResult(Result.Failure<string, Err>(Err.Unauthorized("Error message"))));

        result.IsSuccess.Should().BeFalse("Initial result is failure result");
        result.Error.Should().Be(Err.NotFound("Initial Error message"));
    }

    [Fact]
    public async Task Ensure_Task_Right_with_successInput_and_parameterisedFailurePredicate()
    {
        var initialResult = Result.Success<string, Err>("Initial Success message");

        var result = await initialResult.EnsureAsync(_ => Task.FromResult(Result.Failure<string, Err>(Err.Conflict("Error Message"))));

        result.IsSuccess.Should().BeFalse("Predicate is failure result");
        result.Error.Should().Be(Err.Conflict("Error Message"));
    }

    [Fact]
    public async Task Ensure_Task_Right_with_successInput_and_parameterisedSuccessPredicate()
    {
        var initialResult = Result.Success<string, Err>("Initial Success message");

        var result = await initialResult.EnsureAsync(_ => Task.FromResult(Result.Success<string, Err>("Success Message")));

        result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded"); ;
        result.Ok.Should().Be("Initial Success message");
    }

    [Fact]
    public async Task Ensure_Task_Right_with_failureInput_and_parameterisedSuccessPredicate()
    {
        var initialResult = Result.Failure<string, Err>(Err.Unexpected("Initial Error message"));

        var result = await initialResult.EnsureAsync(_ => Task.FromResult(Result.Success<string, Err>("Success Message")));

        result.IsSuccess.Should().BeFalse("Initial result is failure result"); ;
        result.Error.Should().Be(Err.Unexpected("Initial Error message"));
    }

    [Fact]
    public async Task Ensure_Task_Right_with_failureInput_and_parameterisedFailurePredicate()
    {
        var initialResult = Result.Failure<string, Err>(Err.Unexpected("Initial Error message"));

        var result = await initialResult.EnsureAsync(_ => Task.FromResult(Result.Failure<string, Err>(Err.Unexpected("Success Message"))));

        result.IsSuccess.Should().BeFalse("Initial result and predicate is failure result"); ;
        result.Error.Should().Be(Err.Unexpected("Initial Error message"));
    }
}
