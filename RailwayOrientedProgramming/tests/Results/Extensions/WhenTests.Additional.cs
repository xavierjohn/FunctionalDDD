namespace RailwayOrientedProgramming.Tests.Results.Extensions.WhenTests;

using FunctionalDdd.Testing;
using System.Diagnostics;

public class WhenAdditionalTests : TestBase
{
    #region WhenAsync Additional Tests

    [Fact]
    public async Task WhenAsync_WithBoolean_TrueCondition_ExecutesOperation()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;

        // Act
        var actual = await result.WhenAsync(
            true,
            x =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            });

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public async Task WhenAsync_WithBoolean_WithCancellationToken_Success()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;
        using var cts = new CancellationTokenSource();

        // Act
        var actual = await result.WhenAsync(
            true,
            (x, ct) =>
            {
                operationExecuted = true;
                ct.Should().Be(cts.Token);
                return Task.FromResult(Result.Success(x * 2));
            },
            cts.Token);

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public async Task WhenAsync_TaskResult_WithPredicate_ExecutesOperation()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(42));
        var operationExecuted = false;

        // Act
        var actual = await resultTask.WhenAsync(
            x => x > 40,
            x =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            });

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public async Task WhenAsync_TaskResult_WithPredicate_WithCancellationToken_Success()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(42));
        var operationExecuted = false;
        using var cts = new CancellationTokenSource();

        // Act
        var actual = await resultTask.WhenAsync(
            x => x > 40,
            (x, ct) =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            },
            cts.Token);

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public async Task WhenAsync_TaskResult_WithBoolean_ExecutesOperation()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(42));
        var operationExecuted = false;

        // Act
        var actual = await resultTask.WhenAsync(
            true,
            x =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            });

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public async Task WhenAsync_TaskResult_WithBoolean_WithCancellationToken_Success()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(42));
        var operationExecuted = false;
        using var cts = new CancellationTokenSource();

        // Act
        var actual = await resultTask.WhenAsync(
            true,
            (x, ct) =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            },
            cts.Token);

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    #endregion

    #region UnlessAsync Tests

    [Fact]
    public async Task UnlessAsync_WithPredicate_FalseCondition_ExecutesOperation()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;

        // Act
        var actual = await result.UnlessAsync(
            x => x > 50,
            x =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            });

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public async Task UnlessAsync_WithPredicate_WithCancellationToken_Success()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;
        using var cts = new CancellationTokenSource();

        // Act
        var actual = await result.UnlessAsync(
            x => x > 50,
            (x, ct) =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            },
            cts.Token);

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public async Task UnlessAsync_WithBoolean_FalseCondition_ExecutesOperation()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;

        // Act
        var actual = await result.UnlessAsync(
            false,
            x =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            });

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public async Task UnlessAsync_WithBoolean_WithCancellationToken_Success()
    {
        // Arrange
        var result = Result.Success(42);
        var operationExecuted = false;
        using var cts = new CancellationTokenSource();

        // Act
        var actual = await result.UnlessAsync(
            false,
            (x, ct) =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            },
            cts.Token);

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public async Task UnlessAsync_TaskResult_WithPredicate_ExecutesOperation()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(42));
        var operationExecuted = false;

        // Act
        var actual = await resultTask.UnlessAsync(
            x => x > 50,
            x =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            });

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public async Task UnlessAsync_TaskResult_WithPredicate_WithCancellationToken_Success()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(42));
        var operationExecuted = false;
        using var cts = new CancellationTokenSource();

        // Act
        var actual = await resultTask.UnlessAsync(
            x => x > 50,
            (x, ct) =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            },
            cts.Token);

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public async Task UnlessAsync_TaskResult_WithBoolean_ExecutesOperation()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(42));
        var operationExecuted = false;

        // Act
        var actual = await resultTask.UnlessAsync(
            false,
            x =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            });

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public async Task UnlessAsync_TaskResult_WithBoolean_WithCancellationToken_Success()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(42));
        var operationExecuted = false;
        using var cts = new CancellationTokenSource();

        // Act
        var actual = await resultTask.UnlessAsync(
            false,
            (x, ct) =>
            {
                operationExecuted = true;
                return Task.FromResult(Result.Success(x * 2));
            },
            cts.Token);

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    #endregion

    #region ValueTask Tests

    [Fact]
    public async Task WhenAsync_ValueTask_WithPredicate_ExecutesOperation()
    {
        // Arrange
        var resultTask = ValueTask.FromResult(Result.Success(42));
        var operationExecuted = false;

        // Act
        var actual = await resultTask.WhenAsync(
            x => x > 40,
            x =>
            {
                operationExecuted = true;
                return ValueTask.FromResult(Result.Success(x * 2));
            });

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public async Task WhenAsync_ValueTask_WithPredicate_WithCancellationToken_Success()
    {
        // Arrange
        var resultTask = ValueTask.FromResult(Result.Success(42));
        var operationExecuted = false;
        using var cts = new CancellationTokenSource();

        // Act
        var actual = await resultTask.WhenAsync(
            x => x > 40,
            (x, ct) =>
            {
                operationExecuted = true;
                return ValueTask.FromResult(Result.Success(x * 2));
            },
            cts.Token);

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public async Task UnlessAsync_ValueTask_WithPredicate_ExecutesOperation()
    {
        // Arrange
        var resultTask = ValueTask.FromResult(Result.Success(42));
        var operationExecuted = false;

        // Act
        var actual = await resultTask.UnlessAsync(
            x => x > 50,
            x =>
            {
                operationExecuted = true;
                return ValueTask.FromResult(Result.Success(x * 2));
            });

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    [Fact]
    public async Task UnlessAsync_ValueTask_WithPredicate_WithCancellationToken_Success()
    {
        // Arrange
        var resultTask = ValueTask.FromResult(Result.Success(42));
        var operationExecuted = false;
        using var cts = new CancellationTokenSource();

        // Act
        var actual = await resultTask.UnlessAsync(
            x => x > 50,
            (x, ct) =>
            {
                operationExecuted = true;
                return ValueTask.FromResult(Result.Success(x * 2));
            },
            cts.Token);

        // Assert
        operationExecuted.Should().BeTrue();
        actual.Should().BeSuccess()
            .Which.Should().Be(84);
    }

    #endregion

    #region Activity/Tracing Tests

    [Fact]
    public void When_CreatesActivity_WithCorrectName()
    {
        // Arrange
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => capturedActivity = activity
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success(42);

        // Act
        var actual = result.When(
            x => x > 40,
            x => Result.Success(x * 2));

        // Assert
        capturedActivity.Should().NotBeNull();
        capturedActivity!.DisplayName.Should().Be("When");
    }

    [Fact]
    public void Unless_CreatesActivity_WithCorrectName()
    {
        // Arrange
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => capturedActivity = activity
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success(42);

        // Act
        var actual = result.Unless(
            x => x > 50,
            x => Result.Success(x * 2));

        // Assert
        capturedActivity.Should().NotBeNull();
        capturedActivity!.DisplayName.Should().Be("Unless");
    }

    [Fact]
    public async Task WhenAsync_CreatesActivity_WithCorrectName()
    {
        // Arrange
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => capturedActivity = activity
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success(42);

        // Act
        var actual = await result.WhenAsync(
            x => x > 40,
            x => Task.FromResult(Result.Success(x * 2)));

        // Assert
        capturedActivity.Should().NotBeNull();
        capturedActivity!.DisplayName.Should().Be("When");
    }

    [Fact]
    public async Task UnlessAsync_CreatesActivity_WithCorrectName()
    {
        // Arrange
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => capturedActivity = activity
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success(42);

        // Act
        var actual = await result.UnlessAsync(
            x => x > 50,
            x => Task.FromResult(Result.Success(x * 2)));

        // Assert
        capturedActivity.Should().NotBeNull();
        capturedActivity!.DisplayName.Should().Be("Unless");
    }

    [Fact]
    public void When_LogsActivityStatus_OnSuccess()
    {
        // Arrange
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => capturedActivity = activity
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success(42);

        // Act
        var actual = result.When(
            x => x > 40,
            x => Result.Success(x * 2));

        // Assert
        capturedActivity.Should().NotBeNull();
        capturedActivity!.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public void When_LogsActivityStatus_OnFailure()
    {
        // Arrange
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => capturedActivity = activity
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Failure<int>(Error1);

        // Act
        var actual = result.When(
            x => x > 40,
            x => Result.Success(x * 2));

        // Assert
        capturedActivity.Should().NotBeNull();
        capturedActivity!.Status.Should().Be(ActivityStatusCode.Error);
    }

    [Fact]
    public void When_LogsActivityStatus_WhenOperationReturnsFailure()
    {
        // Arrange
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => capturedActivity = activity
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success(42);

        // Act
        var actual = result.When(
            x => x > 40,
            x => Result.Failure<int>(Error2));

        // Assert
        capturedActivity.Should().NotBeNull();
        capturedActivity!.Status.Should().Be(ActivityStatusCode.Error);
    }

    [Fact]
    public void When_LogsActivityStatus_WhenConditionNotMet()
    {
        // Arrange
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => capturedActivity = activity
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success(42);

        // Act
        var actual = result.When(
            x => x < 40,
            x => Result.Success(x * 2));

        // Assert
        capturedActivity.Should().NotBeNull();
        capturedActivity!.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public async Task WhenAsync_LogsActivityStatus_OnSuccess()
    {
        // Arrange
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => capturedActivity = activity
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success(42);

        // Act
        var actual = await result.WhenAsync(
            x => x > 40,
            x => Task.FromResult(Result.Success(x * 2)));

        // Assert
        capturedActivity.Should().NotBeNull();
        capturedActivity!.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public async Task WhenAsync_LogsActivityStatus_OnFailure()
    {
        // Arrange
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => capturedActivity = activity
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Failure<int>(Error1);

        // Act
        var actual = await result.WhenAsync(
            x => x > 40,
            x => Task.FromResult(Result.Success(x * 2)));

        // Assert
        capturedActivity.Should().NotBeNull();
        capturedActivity!.Status.Should().Be(ActivityStatusCode.Error);
    }

    [Fact]
    public void Unless_LogsActivityStatus_OnSuccess()
    {
        // Arrange
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => capturedActivity = activity
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success(42);

        // Act
        var actual = result.Unless(
            x => x > 50,
            x => Result.Success(x * 2));

        // Assert
        capturedActivity.Should().NotBeNull();
        capturedActivity!.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public async Task UnlessAsync_LogsActivityStatus_OnSuccess()
    {
        // Arrange
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => capturedActivity = activity
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success(42);

        // Act
        var actual = await result.UnlessAsync(
            x => x > 50,
            x => Task.FromResult(Result.Success(x * 2)));

        // Assert
        capturedActivity.Should().NotBeNull();
        capturedActivity!.Status.Should().Be(ActivityStatusCode.Ok);
    }

    #endregion
}
