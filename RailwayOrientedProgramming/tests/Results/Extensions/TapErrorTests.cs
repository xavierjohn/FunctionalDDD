namespace RailwayOrientedProgramming.Tests.Results.Extensions.TapError;

using FunctionalDdd.Testing;

public class TapErrorTests : TestBase
{
    private bool _actionExecuted;
    private Error? _capturedError;

    public TapErrorTests()
    {
        _actionExecuted = false;
        _capturedError = null;
    }

    #region TapError with Action

    [Fact]
    public void TapError_WithAction_FailureResult_ExecutesAction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);

        // Act
        var actual = result.TapError(() => _actionExecuted = true);

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure()
            .Which.Should().HaveCode(Error1.Code);
    }

    [Fact]
    public void TapError_WithAction_SuccessResult_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var actual = result.TapError(() => _actionExecuted = true);

        // Assert
        _actionExecuted.Should().BeFalse();
        actual.Should().BeSuccess()
            .Which.Should().Be(42);
    }

    #endregion

    #region TapError with Action<Error>

    [Fact]
    public void TapError_WithActionError_FailureResult_ExecutesActionWithError()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);

        // Act
        var actual = result.TapError(error =>
        {
            _actionExecuted = true;
            _capturedError = error;
        });

        // Assert
        _actionExecuted.Should().BeTrue();
        Assert.NotNull(_capturedError);
        _capturedError.Code.Should().Be(Error1.Code);
        actual.Should().BeFailure()
            .Which.Should().HaveCode(Error1.Code);
    }

    [Fact]
    public void TapError_WithActionError_SuccessResult_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var actual = result.TapError(error =>
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

    #region TapErrorAsync - Task<Result<T>> with Action

    [Fact]
    public async Task TapErrorAsync_TaskResult_WithAction_FailureResult_ExecutesAction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1).AsTask();

        // Act
        var actual = await result.TapErrorAsync(() => _actionExecuted = true);

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapErrorAsync_TaskResult_WithAction_SuccessResult_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Success(42).AsTask();

        // Act
        var actual = await result.TapErrorAsync(() => _actionExecuted = true);

        // Assert
        _actionExecuted.Should().BeFalse();
        actual.Should().BeSuccess();
    }

    [Fact]
    public async Task TapErrorAsync_TaskResult_WithActionError_FailureResult_ExecutesAction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1).AsTask();

        // Act
        var actual = await result.TapErrorAsync(error =>
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

    #region TapErrorAsync - Result<T> with Func<Task>

    [Fact]
    public async Task TapErrorAsync_Result_WithFuncTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);

        // Act
        var actual = await result.TapErrorAsync(() =>
        {
            _actionExecuted = true;
            return Task.CompletedTask;
        });

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapErrorAsync_Result_WithFuncTask_SuccessResult_DoesNotExecuteFunction()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var actual = await result.TapErrorAsync(() =>
        {
            _actionExecuted = true;
            return Task.CompletedTask;
        });

        // Assert
        _actionExecuted.Should().BeFalse();
        actual.Should().BeSuccess();
    }

    [Fact]
    public async Task TapErrorAsync_Result_WithFuncErrorTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);

        // Act
        var actual = await result.TapErrorAsync(error =>
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

    #region TapErrorAsync - Result<T> with CancellationToken

    [Fact]
    public async Task TapErrorAsync_Result_WithCancellationToken_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);
        using var cts = new CancellationTokenSource();

        // Act
        var actual = await result.TapErrorAsync(ct =>
        {
            _actionExecuted = true;
            return Task.CompletedTask;
        }, cts.Token);

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapErrorAsync_Result_WithErrorAndCancellationToken_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);
        using var cts = new CancellationTokenSource();

        // Act
        var actual = await result.TapErrorAsync((error, ct) =>
        {
            _actionExecuted = true;
            _capturedError = error;
            return Task.CompletedTask;
        }, cts.Token);

        // Assert
        _actionExecuted.Should().BeTrue();
        Assert.NotNull(_capturedError);
        actual.Should().BeFailure();
    }

    #endregion

    #region TapErrorAsync - Task<Result<T>> with Func<Task>

    [Fact]
    public async Task TapErrorAsync_TaskResult_WithFuncTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1).AsTask();

        // Act
        var actual = await result.TapErrorAsync(() =>
        {
            _actionExecuted = true;
            return Task.CompletedTask;
        });

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapErrorAsync_TaskResult_WithFuncErrorTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1).AsTask();

        // Act
        var actual = await result.TapErrorAsync(error =>
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

    [Fact]
    public async Task TapErrorAsync_TaskResult_WithCancellationToken_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1).AsTask();
        using var cts = new CancellationTokenSource();

        // Act
        var actual = await result.TapErrorAsync(ct =>
        {
            _actionExecuted = true;
            return Task.CompletedTask;
        }, cts.Token);

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapErrorAsync_TaskResult_WithErrorAndCancellationToken_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1).AsTask();
        using var cts = new CancellationTokenSource();

        // Act
        var actual = await result.TapErrorAsync((error, ct) =>
        {
            _actionExecuted = true;
            _capturedError = error;
            return Task.CompletedTask;
        }, cts.Token);

        // Assert
        _actionExecuted.Should().BeTrue();
        Assert.NotNull(_capturedError);
        actual.Should().BeFailure();
    }

    #endregion

    #region TapErrorAsync - ValueTask<Result<T>> with Action

    [Fact]
    public async Task TapErrorAsync_ValueTaskResult_WithAction_FailureResult_ExecutesAction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1).AsValueTask();

        // Act
        var actual = await result.TapErrorAsync(() => _actionExecuted = true);

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapErrorAsync_ValueTaskResult_WithAction_SuccessResult_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Success(42).AsValueTask();

        // Act
        var actual = await result.TapErrorAsync(() => _actionExecuted = true);

        // Assert
        _actionExecuted.Should().BeFalse();
        actual.Should().BeSuccess();
    }

    [Fact]
    public async Task TapErrorAsync_ValueTaskResult_WithActionError_FailureResult_ExecutesAction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1).AsValueTask();

        // Act
        var actual = await result.TapErrorAsync(error =>
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

    #region TapErrorAsync - Result<T> with Func<ValueTask>

    [Fact]
    public async Task TapErrorAsync_Result_WithFuncValueTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);

        // Act
        var actual = await result.TapErrorAsync(() =>
        {
            _actionExecuted = true;
            return ValueTask.CompletedTask;
        });

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapErrorAsync_Result_WithFuncValueTask_SuccessResult_DoesNotExecuteFunction()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var actual = await result.TapErrorAsync(() =>
        {
            _actionExecuted = true;
            return ValueTask.CompletedTask;
        });

        // Assert
        _actionExecuted.Should().BeFalse();
        actual.Should().BeSuccess();
    }

    [Fact]
    public async Task TapErrorAsync_Result_WithFuncErrorValueTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);

        // Act
        var actual = await result.TapErrorAsync(error =>
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

    #region TapErrorAsync - Result<T> with ValueTask and CancellationToken

    [Fact]
    public async Task TapErrorAsync_Result_ValueTaskWithCancellation_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);
        using var cts = new CancellationTokenSource();

        Func<CancellationToken, ValueTask> action = ct =>
        {
            _actionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        var actual = await result.TapErrorAsync(action, cts.Token);

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapErrorAsync_Result_ValueTaskWithErrorAndCancellation_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1);
        using var cts = new CancellationTokenSource();

        Func<Error, CancellationToken, ValueTask> action = (error, ct) =>
        {
            _actionExecuted = true;
            _capturedError = error;
            return ValueTask.CompletedTask;
        };

        // Act
        var actual = await result.TapErrorAsync(action, cts.Token);

        // Assert
        _actionExecuted.Should().BeTrue();
        Assert.NotNull(_capturedError);
        actual.Should().BeFailure();
    }

    #endregion

    #region TapErrorAsync - ValueTask<Result<T>> with Func<ValueTask>

    [Fact]
    public async Task TapErrorAsync_ValueTaskResult_WithFuncValueTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1).AsValueTask();

        // Act
        var actual = await result.TapErrorAsync(() =>
        {
            _actionExecuted = true;
            return ValueTask.CompletedTask;
        });

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapErrorAsync_ValueTaskResult_WithFuncErrorValueTask_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1).AsValueTask();

        // Act
        var actual = await result.TapErrorAsync(error =>
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

    [Fact]
    public async Task TapErrorAsync_ValueTaskResult_WithCancellation_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1).AsValueTask();
        using var cts = new CancellationTokenSource();

        Func<CancellationToken, ValueTask> action = ct =>
        {
            _actionExecuted = true;
            return ValueTask.CompletedTask;
        };

        // Act
        var actual = await result.TapErrorAsync(action, cts.Token);

        // Assert
        _actionExecuted.Should().BeTrue();
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapErrorAsync_ValueTaskResult_WithErrorAndCancellation_FailureResult_ExecutesFunction()
    {
        // Arrange
        var result = Result.Failure<int>(Error1).AsValueTask();
        using var cts = new CancellationTokenSource();

        Func<Error, CancellationToken, ValueTask> action = (error, ct) =>
        {
            _actionExecuted = true;
            _capturedError = error;
            return ValueTask.CompletedTask;
        };

        // Act
        var actual = await result.TapErrorAsync(action, cts.Token);

        // Assert
        _actionExecuted.Should().BeTrue();
        Assert.NotNull(_capturedError);
        actual.Should().BeFailure();
    }

    #endregion

    #region Real-World Scenarios

    [Fact]
    public void TapError_LoggingErrorDetails()
    {
        // Arrange
        var result = Result.Failure<string>(Error.NotFound("User not found", "user-123"));
        var loggedMessage = string.Empty;

        // Act
#pragma warning disable IDE0053 // Multi-statement lambda cannot use expression body
        var actual = result.TapError(error =>
        {
            loggedMessage = $"Error: {error.Code} - {error.Detail} (Instance: {error.Instance})";
        });
#pragma warning restore IDE0053

        // Assert
        loggedMessage.Should().Contain("not.found");
        loggedMessage.Should().Contain("User not found");
        loggedMessage.Should().Contain("user-123");
        actual.Should().BeFailure();
    }

    [Fact]
    public async Task TapErrorAsync_IncrementErrorMetrics()
    {
        // Arrange
        var result = Result.Failure<int>(Error.ServiceUnavailable("External service down"));
        var errorCount = 0;

        // Act
        var actual = await result.TapErrorAsync(error =>
        {
            if (error is ServiceUnavailableError)
                errorCount++;
            return Task.CompletedTask;
        });

        // Assert
        errorCount.Should().Be(1);
        actual.Should().BeFailure();
    }

    [Fact]
    public void TapError_ChainedWithOtherOperations()
    {
        // Arrange
        var errorMessages = new List<string>();

        // Act
        var result = Result.Failure<int>(Error.Validation("Invalid input"))
            .TapError(error => errorMessages.Add("First handler"))
            .TapError(error => errorMessages.Add("Second handler"))
            .TapError(error => errorMessages.Add("Third handler"));

        // Assert
        errorMessages.Should().HaveCount(3);
        errorMessages.Should().Equal("First handler", "Second handler", "Third handler");
        result.Should().BeFailure();
    }

    #endregion
}
