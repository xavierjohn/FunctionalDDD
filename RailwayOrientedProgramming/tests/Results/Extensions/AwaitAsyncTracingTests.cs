namespace RailwayOrientedProgramming.Tests.Results.Extensions;

using FunctionalDdd.Testing;
using RailwayOrientedProgramming.Tests.Helpers;
using System.Diagnostics;

/// <summary>
/// Tests for Activity tracing in AwaitAsync operations.
/// Verifies that AwaitAsync creates proper distributed tracing spans for observability.
/// </summary>
public class AwaitAsyncTracingTests : TestBase
{
    #region Activity Creation Tests

    [Fact]
    public async Task AwaitAsync_2Tuple_CreatesActivity_WithCorrectName()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var task1 = Task.FromResult(Result.Success(1));
        var task2 = Task.FromResult(Result.Success(2));

        // Act
        await (task1, task2).AwaitAsync();

        // Assert
        var activity = activityTest.AssertActivityCaptured("AwaitAsync");
        activity.DisplayName.Should().Be("AwaitAsync");
    }

    [Fact]
    public async Task AwaitAsync_3Tuple_CreatesActivity_WithCorrectName()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var task1 = Task.FromResult(Result.Success(1));
        var task2 = Task.FromResult(Result.Success(2));
        var task3 = Task.FromResult(Result.Success(3));

        // Act
        await (task1, task2, task3).AwaitAsync();

        // Assert
        var activity = activityTest.AssertActivityCaptured("AwaitAsync");
        activity.DisplayName.Should().Be("AwaitAsync");
    }

    [Fact]
    public async Task AwaitAsync_9Tuple_CreatesActivity_WithCorrectName()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var task1 = Task.FromResult(Result.Success(1));
        var task2 = Task.FromResult(Result.Success(2));
        var task3 = Task.FromResult(Result.Success(3));
        var task4 = Task.FromResult(Result.Success(4));
        var task5 = Task.FromResult(Result.Success(5));
        var task6 = Task.FromResult(Result.Success(6));
        var task7 = Task.FromResult(Result.Success(7));
        var task8 = Task.FromResult(Result.Success(8));
        var task9 = Task.FromResult(Result.Success(9));

        // Act
        await (task1, task2, task3, task4, task5, task6, task7, task8, task9).AwaitAsync();

        // Assert
        var activity = activityTest.AssertActivityCaptured("AwaitAsync");
        activity.DisplayName.Should().Be("AwaitAsync");
    }

    #endregion

    #region Activity Status Tests - Success

    [Fact]
    public async Task AwaitAsync_2Tuple_AllSuccess_LogsOkStatus()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var task1 = Task.FromResult(Result.Success(1));
        var task2 = Task.FromResult(Result.Success(2));

        // Act
        await (task1, task2).AwaitAsync();

        // Assert
        activityTest.AssertActivityCapturedWithStatus("AwaitAsync", ActivityStatusCode.Ok);
    }

    [Fact]
    public async Task AwaitAsync_3Tuple_AllSuccess_LogsOkStatus()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var task1 = Task.FromResult(Result.Success(1));
        var task2 = Task.FromResult(Result.Success(2));
        var task3 = Task.FromResult(Result.Success(3));

        // Act
        await (task1, task2, task3).AwaitAsync();

        // Assert
        activityTest.AssertActivityCapturedWithStatus("AwaitAsync", ActivityStatusCode.Ok);
    }

    [Fact]
    public async Task AwaitAsync_9Tuple_AllSuccess_LogsOkStatus()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var task1 = Task.FromResult(Result.Success(1));
        var task2 = Task.FromResult(Result.Success(2));
        var task3 = Task.FromResult(Result.Success(3));
        var task4 = Task.FromResult(Result.Success(4));
        var task5 = Task.FromResult(Result.Success(5));
        var task6 = Task.FromResult(Result.Success(6));
        var task7 = Task.FromResult(Result.Success(7));
        var task8 = Task.FromResult(Result.Success(8));
        var task9 = Task.FromResult(Result.Success(9));

        // Act
        await (task1, task2, task3, task4, task5, task6, task7, task8, task9).AwaitAsync();

        // Assert
        activityTest.AssertActivityCapturedWithStatus("AwaitAsync", ActivityStatusCode.Ok);
    }

    #endregion

    #region Activity Status Tests - Failure

    [Fact]
    public async Task AwaitAsync_2Tuple_FirstFails_LogsErrorStatus()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var task1 = Task.FromResult(Result.Failure<int>(Error.NotFound("Not found")));
        var task2 = Task.FromResult(Result.Success(2));

        // Act
        await (task1, task2).AwaitAsync();

        // Assert
        activityTest.AssertActivityCapturedWithStatus("AwaitAsync", ActivityStatusCode.Error);
    }

    [Fact]
    public async Task AwaitAsync_3Tuple_OneFails_LogsErrorStatus()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var task1 = Task.FromResult(Result.Success(1));
        var task2 = Task.FromResult(Result.Failure<int>(Error.Validation("Invalid")));
        var task3 = Task.FromResult(Result.Success(3));

        // Act
        await (task1, task2, task3).AwaitAsync();

        // Assert
        activityTest.AssertActivityCapturedWithStatus("AwaitAsync", ActivityStatusCode.Error);
    }

    [Fact]
    public async Task AwaitAsync_3Tuple_AllFail_LogsErrorStatus()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var task1 = Task.FromResult(Result.Failure<int>(Error.Validation("Error 1")));
        var task2 = Task.FromResult(Result.Failure<int>(Error.Validation("Error 2")));
        var task3 = Task.FromResult(Result.Failure<int>(Error.Validation("Error 3")));

        // Act
        await (task1, task2, task3).AwaitAsync();

        // Assert
        activityTest.AssertActivityCapturedWithStatus("AwaitAsync", ActivityStatusCode.Error);
    }

    [Fact]
    public async Task AwaitAsync_9Tuple_MultipleFail_LogsErrorStatus()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var task1 = Task.FromResult(Result.Success(1));
        var task2 = Task.FromResult(Result.Failure<int>(Error.Validation("Error 2")));
        var task3 = Task.FromResult(Result.Success(3));
        var task4 = Task.FromResult(Result.Success(4));
        var task5 = Task.FromResult(Result.Failure<int>(Error.NotFound("Error 5")));
        var task6 = Task.FromResult(Result.Success(6));
        var task7 = Task.FromResult(Result.Success(7));
        var task8 = Task.FromResult(Result.Failure<int>(Error.Conflict("Error 8")));
        var task9 = Task.FromResult(Result.Success(9));

        // Act
        await (task1, task2, task3, task4, task5, task6, task7, task8, task9).AwaitAsync();

        // Assert
        activityTest.AssertActivityCapturedWithStatus("AwaitAsync", ActivityStatusCode.Error);
    }

    #endregion

    #region Chaining Tests

    [Fact]
    public async Task AwaitAsync_3Tuple_ChainedWithBind_CreatesMultipleActivities()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var task1 = Task.FromResult(Result.Success(10));
        var task2 = Task.FromResult(Result.Success(20));
        var task3 = Task.FromResult(Result.Success(30));

        // Act
        await (task1, task2, task3)
            .AwaitAsync()
            .BindAsync((a, b, c) => Result.Success(a + b + c));

        // Assert
        // Should have both AwaitAsync and Bind activities
        activityTest.AssertActivityCaptured("AwaitAsync", 1);
        activityTest.AssertActivityCaptured("Bind", 1);
    }

    #endregion
}