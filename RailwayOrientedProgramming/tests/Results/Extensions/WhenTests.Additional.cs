namespace RailwayOrientedProgramming.Tests.Results.Extensions.WhenTests;

using FunctionalDdd.Testing;
using RailwayOrientedProgramming.Tests.Helpers;
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
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success(42);

        // Act
        var actual = result.When(
            x => x > 40,
            x => Result.Success(x * 2));

        // Assert
        activityTest.WaitForActivityCount(1);
        var activity = activityTest.WaitForActivity("When");
        activity.Should().NotBeNull();
        activity!.DisplayName.Should().Be("When");
    }

    [Fact]
    public void Unless_CreatesActivity_WithCorrectName()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success(42);

        // Act
        var actual = result.Unless(
            x => x > 50,
            x => Result.Success(x * 2));

        // Assert
        activityTest.WaitForActivityCount(1);
        var activity = activityTest.WaitForActivity("Unless");
        activity.Should().NotBeNull();
        activity!.DisplayName.Should().Be("Unless");
    }

    [Fact]
    public async Task WhenAsync_CreatesActivity_WithCorrectName()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success(42);

        // Act
        var actual = await result.WhenAsync(
            x => x > 40,
            x => Task.FromResult(Result.Success(x * 2)));

        // Assert
        activityTest.WaitForActivityCount(1);
        var activity = activityTest.WaitForActivity("When");
        activity.Should().NotBeNull();
        activity!.DisplayName.Should().Be("When");
    }

    [Fact]
    public async Task UnlessAsync_CreatesActivity_WithCorrectName()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success(42);

        // Act
        var actual = await result.UnlessAsync(
            x => x > 50,
            x => Task.FromResult(Result.Success(x * 2)));

        // Assert
        activityTest.WaitForActivityCount(1);
        var activity = activityTest.WaitForActivity("Unless");
        activity.Should().NotBeNull();
        activity!.DisplayName.Should().Be("Unless");
    }

    [Fact]
    public void When_LogsActivityStatus_OnSuccess()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success(42);

        // Act
        var actual = result.When(
            x => x > 40,
            x => Result.Success(x * 2));

        // Assert
        activityTest.WaitForActivityCount(1);
        var activity = activityTest.WaitForActivity("When");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public void When_LogsActivityStatus_OnFailure()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Failure<int>(Error1);

        // Act
        var actual = result.When(
            x => x > 40,
            x => Result.Success(x * 2));

        // Assert
        activityTest.WaitForActivityCount(1);
        var activity = activityTest.WaitForActivity("When");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Error);
    }

    [Fact]
    public void When_LogsActivityStatus_WhenOperationReturnsFailure()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success(42);

        // Act
        var actual = result.When(
            x => x > 40,
            x => Result.Failure<int>(Error2));

        // Assert
        activityTest.WaitForActivityCount(1);
        var activity = activityTest.WaitForActivity("When");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Error);
    }

    [Fact]
    public void When_LogsActivityStatus_WhenConditionNotMet()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success(42);

        // Act
        var actual = result.When(
            x => x < 40,
            x => Result.Success(x * 2));

        // Assert
        activityTest.WaitForActivityCount(1);
        var activity = activityTest.WaitForActivity("When");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public async Task WhenAsync_LogsActivityStatus_OnSuccess()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success(42);

        // Act
        var actual = await result.WhenAsync(
            x => x > 40,
            x => Task.FromResult(Result.Success(x * 2)));

        // Assert
        activityTest.WaitForActivityCount(1);
        var activity = activityTest.WaitForActivity("When");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public async Task WhenAsync_LogsActivityStatus_OnFailure()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Failure<int>(Error1);

        // Act
        var actual = await result.WhenAsync(
            x => x > 40,
            x => Task.FromResult(Result.Success(x * 2)));

        // Assert
        activityTest.WaitForActivityCount(1);
        var activity = activityTest.WaitForActivity("When");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Error);
    }

    [Fact]
    public void Unless_LogsActivityStatus_OnSuccess()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success(42);

        // Act
        var actual = result.Unless(
            x => x > 50,
            x => Result.Success(x * 2));

        // Assert
        activityTest.WaitForActivityCount(1);
        var activity = activityTest.WaitForActivity("Unless");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public async Task UnlessAsync_LogsActivityStatus_OnSuccess()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success(42);

        // Act
        var actual = await result.UnlessAsync(
            x => x > 50,
            x => Task.FromResult(Result.Success(x * 2)));

        // Assert
        activityTest.WaitForActivityCount(1);
        var activity = activityTest.WaitForActivity("Unless");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Ok);
    }

    #endregion
}
