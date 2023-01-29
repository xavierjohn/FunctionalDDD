﻿namespace RailwayOrientedProgramming.Tests.Functional.Results.Extensions;

using FunctionalDDD;

public class Ensure_Task_Tests
{
    [Fact]
    public async Task Ensure_Task_with_successInput_and_successPredicate()
    {
        var initialResult = Task.FromResult(Result.Success("Initial message"));

        var result = await initialResult.EnsureAsync(() => Task.FromResult(Result.Success("Success message")));

        result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded");
        result.Ok.Should().Be("Initial message");
    }

    [Fact]
    public async Task Ensure_Task_with_successInput_and_failurePredicate()
    {
        var initialResult = Task.FromResult(Result.Success("Initial Result"));

        var result = await initialResult.EnsureAsync(() => Task.FromResult(Result.Failure<string>(Err.Unexpected("Error message"))));

        result.IsSuccess.Should().BeFalse("Predicate is failure result");
        result.Error.Should().Be(Err.Unexpected("Error message"));
    }

    [Fact]
    public async Task Ensure_Task_with_failureInput_and_successPredicate()
    {
        var initialResult = Task.FromResult(Result.Failure<string>(Err.Unauthorized("Initial Error message")));

        var result = await initialResult.EnsureAsync(() => Task.FromResult(Result.Success("Success message")));

        result.IsSuccess.Should().BeFalse("Initial result is failure result");
        result.Error.Should().Be(Err.Unauthorized("Initial Error message"));
    }

    [Fact]
    public async Task Ensure_Task_with_failureInput_and_failurePredicate()
    {
        var initialResult = Task.FromResult(Result.Failure<string>(Err.Validation("Initial Error message")));

        var result = await initialResult.EnsureAsync(() => Task.FromResult(Result.Failure<string>(Err.Unauthorized("Error message"))));

        result.IsSuccess.Should().BeFalse("Initial result is failure result");
        result.Error.Should().Be(Err.Validation("Initial Error message"));
    }

    [Fact]
    public async Task Ensure_Task_with_successInput_and_parameterisedSuccessPredicate()
    {
        var initialResult = Task.FromResult(Result.Success("Initial Success message"));

        var result = await initialResult.EnsureAsync(_ => Task.FromResult(Result.Success("Success Message")));

        result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded"); ;
        result.Ok.Should().Be("Initial Success message");
    }

    [Fact]
    public async Task Ensure_Task_with_failureInput_and_parameterisedSuccessPredicate()
    {
        var initialResult = Task.FromResult(Result.Failure<string>(Err.Conflict("Initial Error message")));

        var result = await initialResult.EnsureAsync(_ => Task.FromResult(Result.Success("Success Message")));

        result.IsSuccess.Should().BeFalse("Initial result is failure result"); ;
        result.Error.Should().Be(Err.Conflict("Initial Error message"));
    }

    [Fact]
    public async Task Ensure_Task_with_failureInput_and_parameterisedFailurePredicate()
    {
        var initialResult = Task.FromResult(Result.Failure<string>(Err.Conflict("Initial Error message")));

        var result = await initialResult.EnsureAsync(_ => Task.FromResult(Result.Failure<string>(Err.Unexpected("Success Message"))));

        result.IsSuccess.Should().BeFalse("Initial result and predicate is failure result"); ;
        result.Error.Should().Be(Err.Conflict("Initial Error message"));
    }
}
