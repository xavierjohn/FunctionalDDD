using FluentAssertions;
using Xunit;
using FunctionalDdd;
using FunctionalDdd.Testing;

namespace RailwayOrientedProgramming.Tests.Results.Extensions.TapError;

/// <summary>
/// Tests for T4-generated TapError tuple overloads (TapErrorTs.g.tt).
/// These tests ensure at least one tuple permutation is covered to catch T4 generation bugs.
/// </summary>
public class TapErrorTupleTests : TestBase
{
    #region TapError - Sync Tuple Overloads

    [Fact]
    public void TapError_Tuple2_FailureResult_ExecutesAction()
    {
        // Arrange
        var result = Result.Failure<(int, string)>(Error1);
        var actionExecuted = false;

        // Act
        var actual = result.TapError(() => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeTrue();
        actual.IsFailure.Should().BeTrue();
        actual.Error.Should().Be(Error1);
    }

    [Fact]
    public void TapError_Tuple2_SuccessResult_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Success((42, "hello"));
        var actionExecuted = false;

        // Act
        var actual = result.TapError(() => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeFalse();
        actual.IsSuccess.Should().BeTrue();
        actual.Value.Should().Be((42, "hello"));
    }

    [Fact]
    public void TapError_Tuple3_FailureResult_ExecutesActionWithError()
    {
        // Arrange
        var result = Result.Failure<(int, string, double)>(Error1);
        Error? capturedError = null;

        // Act
        var actual = result.TapError(error => capturedError = error);

        // Assert
        Assert.NotNull(capturedError);
        capturedError.Code.Should().Be(Error1.Code);
        actual.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void TapError_Tuple3_SuccessResult_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Success((1, "two", 3.0));
        Error? capturedError = null;

        // Act
        var actual = result.TapError(error => capturedError = error);

        // Assert
        Assert.Null(capturedError);
        actual.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region TapErrorAsync - Task Tuple Overloads

    [Fact]
    public async Task TapErrorAsync_Tuple2_TaskResult_FailureResult_ExecutesAction()
    {
        // Arrange
        var result = Result.Failure<(int, string)>(Error1).AsTask();
        var actionExecuted = false;

        // Act
        var actual = await result.TapErrorAsync(() => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeTrue();
        actual.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_Tuple2_TaskResult_SuccessResult_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Success((42, "hello")).AsTask();
        var actionExecuted = false;

        // Act
        var actual = await result.TapErrorAsync(() => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeFalse();
        actual.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_Tuple3_TaskResult_WithActionError_ExecutesAction()
    {
        // Arrange
        var result = Result.Failure<(int, string, double)>(Error1).AsTask();
        Error? capturedError = null;

        // Act
        var actual = await result.TapErrorAsync(error => capturedError = error);

        // Assert
        Assert.NotNull(capturedError);
        actual.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_Tuple2_Result_WithFuncTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<(int, string)>(Error1);
        var actionExecuted = false;

        // Act
        var actual = await result.TapErrorAsync(() =>
        {
            actionExecuted = true;
            return Task.CompletedTask;
        });

        // Assert
        actionExecuted.Should().BeTrue();
        actual.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_Tuple2_Result_WithFuncErrorTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<(int, string)>(Error1);
        Error? capturedError = null;

        // Act
        var actual = await result.TapErrorAsync(error =>
        {
            capturedError = error;
            return Task.CompletedTask;
        });

        // Assert
        Assert.NotNull(capturedError);
        actual.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_Tuple2_TaskResult_WithFuncTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<(int, string)>(Error1).AsTask();
        var actionExecuted = false;

        // Act
        var actual = await result.TapErrorAsync(() =>
        {
            actionExecuted = true;
            return Task.CompletedTask;
        });

        // Assert
        actionExecuted.Should().BeTrue();
        actual.IsFailure.Should().BeTrue();
    }

    #endregion

    #region TapErrorAsync - ValueTask Tuple Overloads

    [Fact]
    public async Task TapErrorAsync_Tuple2_ValueTaskResult_FailureResult_ExecutesAction()
    {
        // Arrange
        var result = Result.Failure<(int, string)>(Error1).AsValueTask();
        var actionExecuted = false;

        // Act
        var actual = await result.TapErrorAsync(() => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeTrue();
        actual.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_Tuple2_ValueTaskResult_SuccessResult_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Success((42, "hello")).AsValueTask();
        var actionExecuted = false;

        // Act
        var actual = await result.TapErrorAsync(() => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeFalse();
        actual.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_Tuple2_Result_WithFuncValueTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<(int, string)>(Error1);
        var actionExecuted = false;

        // Act
        var actual = await result.TapErrorAsync(() =>
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        });

        // Assert
        actionExecuted.Should().BeTrue();
        actual.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_Tuple2_Result_WithFuncErrorValueTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<(int, string)>(Error1);
        Error? capturedError = null;

        // Act
        var actual = await result.TapErrorAsync(error =>
        {
            capturedError = error;
            return ValueTask.CompletedTask;
        });

        // Assert
        Assert.NotNull(capturedError);
        actual.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_Tuple2_ValueTaskResult_WithFuncValueTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<(int, string)>(Error1).AsValueTask();
        var actionExecuted = false;

        // Act
        var actual = await result.TapErrorAsync(() =>
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        });

        // Assert
        actionExecuted.Should().BeTrue();
        actual.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_Tuple2_ValueTaskResult_WithFuncErrorValueTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<(int, string)>(Error1).AsValueTask();
        Error? capturedError = null;

        // Act
        var actual = await result.TapErrorAsync(error =>
        {
            capturedError = error;
            return ValueTask.CompletedTask;
        });

        // Assert
        Assert.NotNull(capturedError);
        actual.IsFailure.Should().BeTrue();
    }

    #endregion
}
