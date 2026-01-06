namespace RailwayOrientedProgramming.Tests.Results.Extensions;

using FunctionalDdd.Testing;
using RailwayOrientedProgramming.Tests.Helpers;
using System.Diagnostics;

public class EnsureTracingTests
{
    #region Activity Name Tests

    [Fact]
    public void Ensure_CreatesActivity_WithCorrectName()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success(42);

        // Act
        var actual = result.Ensure(x => x > 40, Error.Validation("Value must be > 40"));

        // Assert
        var activity = activityTest.GetActivity("Ensure");
        activity.Should().NotBeNull();
        activity!.DisplayName.Should().Be("Ensure");
    }

    [Fact]
    public void EnsureNotNullOrWhiteSpace_CreatesActivity_WithCorrectName()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();

        // Act
        var actual = "test".EnsureNotNullOrWhiteSpace(Error.Validation("String is required"));

        // Assert
        var activity = activityTest.GetActivity("EnsureNotNullOrWhiteSpace");
        activity.Should().NotBeNull();
        activity!.DisplayName.Should().Be("EnsureNotNullOrWhiteSpace");
    }

    #endregion

    #region Activity Status Tests - Success Cases

    [Fact]
    public void Ensure_LogsActivityStatus_OnSuccess_WithPredicatePass()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success(42);

        // Act
        var actual = result.Ensure(x => x > 40, Error.Validation("Value must be > 40"));

        // Assert
        var activity = activityTest.GetActivity("Ensure");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Ok);
        actual.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Ensure_LogsActivityStatus_OnSuccess_WithBooleanPredicate()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success("test");

        // Act
        var actual = result.Ensure(() => true, Error.Validation("Must be true"));

        // Assert
        var activity = activityTest.GetActivity("Ensure");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Ok);
        actual.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void EnsureNotNullOrWhiteSpace_LogsActivityStatus_OnSuccess()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();

        // Act
        var actual = "test".EnsureNotNullOrWhiteSpace(Error.Validation("String is required"));

        // Assert
        var activity = activityTest.GetActivity("EnsureNotNullOrWhiteSpace");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Ok);
        actual.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Activity Status Tests - Failure Cases

    [Fact]
    public void Ensure_LogsActivityStatus_OnFailure_PredicateFails()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success(42);

        // Act
        var actual = result.Ensure(x => x < 40, Error.Validation("Value must be < 40"));

        // Assert
        var activity = activityTest.GetActivity("Ensure");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Error);
        actual.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Ensure_LogsActivityStatus_OnFailure_InputResultIsFailure()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var error = Error.NotFound("Not found");
        var result = Result.Failure<int>(error);

        // Act
        var actual = result.Ensure(x => x > 40, Error.Validation("Value must be > 40"));

        // Assert
        var activity = activityTest.GetActivity("Ensure");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Error);
        actual.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Ensure_WithErrorPredicate_LogsActivityStatus_OnFailure()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success(5);

        // Act
        var actual = result.Ensure(
            x => x > 10,
            x => Error.Validation($"Value {x} must be > 10"));

        // Assert
        var activity = activityTest.GetActivity("Ensure");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Error);
        actual.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Ensure_WithResultPredicate_LogsActivityStatus_PredicateReturnsFailure()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success(42);

        // Act
        var actual = result.Ensure(() => Result.Failure<int>(Error.Validation("Predicate failed")));

        // Assert
        var activity = activityTest.GetActivity("Ensure");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Error);
        actual.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Ensure_WithValueResultPredicate_LogsActivityStatus_PredicateReturnsFailure()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();
        var result = Result.Success(42);

        // Act
        var actual = result.Ensure(
            value => Result.Failure<int>(Error.Validation($"Value {value} is invalid")));

        // Assert
        var activity = activityTest.GetActivity("Ensure");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Error);
        actual.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void EnsureNotNullOrWhiteSpace_LogsActivityStatus_OnFailure_NullString()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();

        // Act
        string? str = null;
        var actual = str.EnsureNotNullOrWhiteSpace(Error.Validation("String is required"));

        // Assert
        var activity = activityTest.GetActivity("EnsureNotNullOrWhiteSpace");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Error);
        actual.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void EnsureNotNullOrWhiteSpace_LogsActivityStatus_OnFailure_EmptyString()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();

        // Act
        var actual = "   ".EnsureNotNullOrWhiteSpace(Error.Validation("String is required"));

        // Assert
        var activity = activityTest.GetActivity("EnsureNotNullOrWhiteSpace");
        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Error);
        actual.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Chaining Tests

    [Fact]
    public void Ensure_CanBeChainedWithOtherOperations()
    {
        // Arrange
        using var activityTest = new ActivityTestHelper();

        // Act
        var result = Result.Success(42)
            .Ensure(x => x > 0, Error.Validation("Must be positive"))
            .Ensure(x => x < 100, Error.Validation("Must be < 100"))
            .Bind(x => Result.Success(x * 2));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(84);
        
        // Verify activities were created
        // Note: When using block ends here, triggering Dispose which ensures all callbacks complete
        var activities = activityTest.GetAllActivities();
        var ensureActivities = activities.Where(a => a.DisplayName == "Ensure").ToList();
        
        // Should have at least 2 Ensure activities from the chain
        ensureActivities.Should().HaveCountGreaterOrEqualTo(2, "because we called Ensure twice in the chain");
        ensureActivities.Should().OnlyContain(a => a.Status == ActivityStatusCode.Ok, "because all operations succeeded");
    }

    #endregion
}
