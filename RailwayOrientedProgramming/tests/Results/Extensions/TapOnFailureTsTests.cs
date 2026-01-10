namespace RailwayOrientedProgramming.Tests.Results.Extensions.TapOnFailure;

using FunctionalDdd.Testing;

/// <summary>
/// Tests for TapOnFailure extension methods on tuple-based Result types (2-9 elements).
/// These tests verify the T4-generated code in TapOnFailureTs.g.cs.
/// </summary>
public class TapOnFailureTsTests : TestBase
{
    private bool _actionExecuted;
    private Error? _capturedError;

    public TapOnFailureTsTests()
    {
        _actionExecuted = false;
        _capturedError = null;
    }

    #region 2-Tuple Tests

    [Fact]
    public void TapOnFailure_2Tuple_WithAction_FailureResult_ExecutesAction()
    {
        // Arrange
        var result = Result.Failure<(T, K)>(Error1);

        // Act
        var actual = result.TapOnFailure(() => _actionExecuted = true);

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure()
            .Which.Should().HaveCode(Error1.Code);
    }

    [Fact]
    public void TapOnFailure_2Tuple_WithAction_SuccessResult_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Success((T.Value1, K.Value1));

        // Act
        var actual = result.TapOnFailure(() => _actionExecuted = true);

        // Assert
        _actionExecuted.Should().BeFalse();
        actual.Should().BeSuccess();
    }

    [Fact]
    public void TapOnFailure_2Tuple_WithActionError_FailureResult_ExecutesActionWithError()
    {
        // Arrange
        var result = Result.Failure<(T, K)>(Error1);

        // Act
        var actual = result.TapOnFailure(error =>
        {
            _actionExecuted = true;
            _capturedError = error;
        });

        // Assert
        _actionExecuted.Should().BeTrue();
        Assert.NotNull(_capturedError);
        _capturedError.Code.Should().Be(Error1.Code);
        actual.Should().BeFailure();
    }

    [Fact]
    public void TapOnFailure_2Tuple_WithActionError_SuccessResult_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Success((T.Value1, K.Value1));

        // Act
        var actual = result.TapOnFailure(error =>
        {
            _actionExecuted = true;
            _capturedError = error;
        });

        // Assert
        _actionExecuted.Should().BeFalse();
        Assert.Null(_capturedError);
        actual.Should().BeSuccess();
    }

    #endregion

    #region 3-Tuple Tests

    [Fact]
    public void TapOnFailure_3Tuple_WithAction_FailureResult_ExecutesAction()
    {
        // Arrange
        var result = Result.Failure<(T, K, int)>(Error2);

        // Act
        var actual = result.TapOnFailure(() => _actionExecuted = true);

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public void TapOnFailure_3Tuple_WithActionError_SuccessResult_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Success((T.Value1, K.Value1, 42));

        // Act
        var actual = result.TapOnFailure(error => _actionExecuted = true);

        // Assert
        _actionExecuted.Should().BeFalse();
        actual.Should().BeSuccess();
    }

    #endregion

    #region Async Tests - Task<Result<(T1, T2)>>

    [Fact]
    public async Task TapOnFailureAsync_2Tuple_TaskResult_WithAction_FailureResult_ExecutesAction()
    {
        // Arrange
        var result = Result.Failure<(T, K)>(Error1).AsTask();

        // Act
        var actual = await result.TapOnFailureAsync(() => _actionExecuted = true);

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapOnFailureAsync_2Tuple_TaskResult_WithAction_SuccessResult_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Success((T.Value1, K.Value1)).AsTask();

        // Act
        var actual = await result.TapOnFailureAsync(() => _actionExecuted = true);

        // Assert
        _actionExecuted.Should().BeFalse();
        actual.Should().BeSuccess();
    }

    [Fact]
    public async Task TapOnFailureAsync_2Tuple_TaskResult_WithActionError_FailureResult_ExecutesAction()
    {
        // Arrange
        var result = Result.Failure<(T, K)>(Error1).AsTask();

        // Act
        var actual = await result.TapOnFailureAsync(error =>
        {
            _actionExecuted = true;
            _capturedError = error;
        });

        // Assert
        _actionExecuted.Should().BeTrue();
        Assert.NotNull(_capturedError);
        actual.Should().BeFailure();
    }

    #endregion

    #region Async Tests - Result<(T1, T2)> with Func<Task>

    [Fact]
    public async Task TapOnFailureAsync_2Tuple_Result_WithFuncTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<(T, K)>(Error1);

        // Act
        var actual = await result.TapOnFailureAsync(() =>
        {
            _actionExecuted = true;
            return Task.CompletedTask;
        });

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapOnFailureAsync_2Tuple_Result_WithFuncTask_SuccessResult_DoesNotExecuteFunction()
    {
        // Arrange
        var result = Result.Success((T.Value1, K.Value1));

        // Act
        var actual = await result.TapOnFailureAsync(() =>
        {
            _actionExecuted = true;
            return Task.CompletedTask;
        });

        // Assert
        _actionExecuted.Should().BeFalse();
        actual.Should().BeSuccess();
    }

    [Fact]
    public async Task TapOnFailureAsync_2Tuple_Result_WithFuncErrorTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<(T, K)>(Error1);

        // Act
        var actual = await result.TapOnFailureAsync(error =>
        {
            _actionExecuted = true;
            _capturedError = error;
            return Task.CompletedTask;
        });

        // Assert
        _actionExecuted.Should().BeTrue();
        Assert.NotNull(_capturedError);
        actual.Should().BeFailure();
    }

    #endregion

    #region Async Tests - Task<Result<(T1, T2)>> with Func<Task>

    [Fact]
    public async Task TapOnFailureAsync_2Tuple_TaskResult_WithFuncTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<(T, K)>(Error1).AsTask();

        // Act
        var actual = await result.TapOnFailureAsync(() =>
        {
            _actionExecuted = true;
            return Task.CompletedTask;
        });

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapOnFailureAsync_2Tuple_TaskResult_WithFuncErrorTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<(T, K)>(Error1).AsTask();

        // Act
        var actual = await result.TapOnFailureAsync(error =>
        {
            _actionExecuted = true;
            _capturedError = error;
            return Task.CompletedTask;
        });

        // Assert
        _actionExecuted.Should().BeTrue();
        Assert.NotNull(_capturedError);
        actual.Should().BeFailure();
    }

    #endregion

    #region ValueTask Tests - ValueTask<Result<(T1, T2)>> with Action

    [Fact]
    public async Task TapOnFailureAsync_2Tuple_ValueTaskResult_WithAction_FailureResult_ExecutesAction()
    {
        // Arrange
        var result = Result.Failure<(T, K)>(Error1).AsValueTask();

        // Act
        var actual = await result.TapOnFailureAsync(() => _actionExecuted = true);

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapOnFailureAsync_2Tuple_ValueTaskResult_WithAction_SuccessResult_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Success((T.Value1, K.Value1)).AsValueTask();

        // Act
        var actual = await result.TapOnFailureAsync(() => _actionExecuted = true);

        // Assert
        _actionExecuted.Should().BeFalse();
        actual.Should().BeSuccess();
    }

    [Fact]
    public async Task TapOnFailureAsync_2Tuple_ValueTaskResult_WithActionError_FailureResult_ExecutesAction()
    {
        // Arrange
        var result = Result.Failure<(T, K)>(Error1).AsValueTask();

        // Act
        var actual = await result.TapOnFailureAsync(error =>
        {
            _actionExecuted = true;
            _capturedError = error;
        });

        // Assert
        _actionExecuted.Should().BeTrue();
        Assert.NotNull(_capturedError);
        actual.Should().BeFailure();
    }

    #endregion

    #region ValueTask Tests - Result<(T1, T2)> with Func<ValueTask>

    [Fact]
    public async Task TapOnFailureAsync_2Tuple_Result_WithFuncValueTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<(T, K)>(Error1);

        // Act
        var actual = await result.TapOnFailureAsync(() =>
        {
            _actionExecuted = true;
            return ValueTask.CompletedTask;
        });

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapOnFailureAsync_2Tuple_Result_WithFuncValueTask_SuccessResult_DoesNotExecuteFunction()
    {
        // Arrange
        var result = Result.Success((T.Value1, K.Value1));

        // Act
        var actual = await result.TapOnFailureAsync(() =>
        {
            _actionExecuted = true;
            return ValueTask.CompletedTask;
        });

        // Assert
        _actionExecuted.Should().BeFalse();
        actual.Should().BeSuccess();
    }

    [Fact]
    public async Task TapOnFailureAsync_2Tuple_Result_WithFuncErrorValueTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<(T, K)>(Error1);

        // Act
        var actual = await result.TapOnFailureAsync(error =>
        {
            _actionExecuted = true;
            _capturedError = error;
            return ValueTask.CompletedTask;
        });

        // Assert
        _actionExecuted.Should().BeTrue();
        Assert.NotNull(_capturedError);
        actual.Should().BeFailure();
    }

    #endregion

    #region ValueTask Tests - ValueTask<Result<(T1, T2)>> with Func<ValueTask>

    [Fact]
    public async Task TapOnFailureAsync_2Tuple_ValueTaskResult_WithFuncValueTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<(T, K)>(Error1).AsValueTask();

        // Act
        var actual = await result.TapOnFailureAsync(() =>
        {
            _actionExecuted = true;
            return ValueTask.CompletedTask;
        });

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapOnFailureAsync_2Tuple_ValueTaskResult_WithFuncErrorValueTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<(T, K)>(Error1).AsValueTask();

        // Act
        var actual = await result.TapOnFailureAsync(error =>
        {
            _actionExecuted = true;
            _capturedError = error;
            return ValueTask.CompletedTask;
        });

        // Assert
        _actionExecuted.Should().BeTrue();
        Assert.NotNull(_capturedError);
        actual.Should().BeFailure();
    }

    #endregion

    #region Real-World Scenario Tests

    [Fact]
    public async Task TapOnFailureAsync_CombineOperations_LogValidationErrors()
    {
        // Arrange
        var errorMessages = new List<string>();

        // Simulate EmailAddress.TryCreate and FirstName.TryCreate returning failures
        var emailResult = Result.Failure<string>(Error.Validation("Invalid email format", "email"));
        var firstNameResult = Result.Failure<string>(Error.Validation("First name is required", "firstName"));

        var combinedResult = emailResult.Combine(firstNameResult);

        // Act
        var actual = await combinedResult.TapOnFailureAsync(error =>
        {
            if (error is ValidationError validationError)
            {
                foreach (var fieldError in validationError.FieldErrors)
                {
                    foreach (var detail in fieldError.Details)
                    {
                        errorMessages.Add($"{fieldError.FieldName}: {detail}");
                    }
                }
            }

            return Task.CompletedTask;
        });

        // Assert
        errorMessages.Should().HaveCountGreaterThan(0);
        actual.Should().BeFailure();
    }

    [Fact]
    public void TapOnFailure_ChainedTupleOperations_AllHandlersExecute()
    {
        // Arrange
        var handlerCalls = new List<string>();
        var result = Result.Failure<(T, K)>(Error.NotFound("Resource not found"));

        // Act
        var actual = result
            .TapOnFailure(() => handlerCalls.Add("Handler 1"))
            .TapOnFailure(error => handlerCalls.Add($"Handler 2: {error.Code}"))
            .TapOnFailure(() => handlerCalls.Add("Handler 3"));

        // Assert
        handlerCalls.Should().HaveCount(3);
        handlerCalls.Should().Contain("Handler 1");
        handlerCalls.Should().Contain(s => s.StartsWith("Handler 2:"));
        handlerCalls.Should().Contain("Handler 3");
        actual.Should().BeFailure();
    }

    #endregion
}
