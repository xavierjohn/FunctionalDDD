namespace Example;

using FunctionalDdd;
using FunctionalDdd.Aliases;

/// <summary>
/// Examples demonstrating the C#-friendly alias methods (Then, Peek, OrElse, Require)
/// for developers new to functional programming.
/// 
/// These aliases provide the same functionality as the standard FP operations:
/// - Then = Bind (chain operations that can fail)
/// - Peek = Tap (side effects without changing the value)
/// - OrElse = Compensate (fallback on failure)
/// - Require = Ensure (validation)
/// </summary>
public class AliasExamples
{
    #region Value Object

    public record TodoTitle
    {
        public string Value { get; }

        private TodoTitle(string value) => Value = value;

        public static Result<TodoTitle> TryCreate(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Error.Validation("Todo title cannot be empty", nameof(title));

            if (title.Length > 100)
                return Error.Validation("Todo title cannot exceed 100 characters", nameof(title));

            return new TodoTitle(title);
        }

        public override string ToString() => Value;
    }

    #endregion

    #region Example 1: Then - Chain operations that can fail

    [Fact]
    public void Then_ChainsOperations_ReturnsSuccessWhenAllValid()
    {
        // Arrange
        var title = "Buy groceries";

        // Act - Using Then instead of Bind
        var result = TodoTitle.TryCreate(title)
            .Then(t => Result.Success(t.Value.ToUpperInvariant()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("BUY GROCERIES");
    }

    [Fact]
    public void Then_ChainValidations_StopsAtFirstFailure()
    {
        // Arrange
        var title = ""; // Invalid - empty

        // Act - Chain stops at first failure
        var result = TodoTitle.TryCreate(title)
            .Then(t => Result.Success(t.Value.ToUpperInvariant()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error.Detail.Should().Contain("Todo title cannot be empty");
    }

    #endregion

    #region Example 2: Peek - Side effects without changing the value

    [Fact]
    public void Peek_LogsWithoutChangingValue()
    {
        // Arrange
        var title = "Complete project";
        var logMessages = new List<string>();

        // Act - Using Peek instead of Tap
        var result = TodoTitle.TryCreate(title)
            .Peek(t => logMessages.Add($"Created: {t.Value}"))
            .Peek(t => logMessages.Add($"Length: {t.Value.Length}"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(title);
        logMessages.Should().HaveCount(2);
        logMessages[0].Should().Be("Created: Complete project");
        logMessages[1].Should().Be("Length: 16");
    }

    [Fact]
    public void Peek_NotExecutedOnFailure()
    {
        // Arrange
        var title = ""; // Invalid
        var wasExecuted = false;

        // Act - Peek not called when previous step fails
        var result = TodoTitle.TryCreate(title)
            .Peek(_ => wasExecuted = true);

        // Assert
        result.IsFailure.Should().BeTrue();
        wasExecuted.Should().BeFalse(); // Peek was skipped
    }

    #endregion

    #region Example 3: Require - Validate conditions

    [Fact]
    public void Require_ValidatesBusinessRules()
    {
        // Arrange
        var title = "Buy milk";

        // Act - Using Require instead of Ensure
        var result = TodoTitle.TryCreate(title)
            .Require(
                t => t.Value.Length >= 3,
                Error.Validation("Title too short"))
            .Require(
                t => t.Value.Contains("milk"),
                Error.Validation("Must contain 'milk'"));

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Require_FailsWhenConditionNotMet()
    {
        // Arrange
        var title = "No";  // Too short

        // Act - Require check fails
        var result = TodoTitle.TryCreate(title)
            .Require(
                t => t.Value.Length >= 3,
                Error.Validation("Title too short"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error.Detail.Should().Contain("Title too short");
    }

    #endregion

    #region Example 4: OrElse - Fallback on failure

    [Fact]
    public void OrElse_ProvidesDefaultOnFailure()
    {
        // Arrange
        var invalidTitle = ""; // Will fail validation

        // Act - Using OrElse instead of Compensate
        var result = TodoTitle.TryCreate(invalidTitle)
            .OrElse(() => TodoTitle.TryCreate("Untitled Todo"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Untitled Todo");
    }

    [Fact]
    public void OrElse_SkippedOnSuccess()
    {
        // Arrange
        var validTitle = "Valid title";
        var fallbackCalled = false;

        // Act - OrElse not called when first succeeds
        var result = TodoTitle.TryCreate(validTitle)
            .OrElse(() =>
            {
                fallbackCalled = true;
                return TodoTitle.TryCreate("Fallback");
            });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(validTitle);
        fallbackCalled.Should().BeFalse(); // Fallback was skipped
    }

    [Fact]
    public void OrElse_ChainMultipleFallbacks()
    {
        // Arrange
        var invalidTitle = "";

        // Act - Chain multiple fallbacks
        var result = TodoTitle.TryCreate(invalidTitle)
            .OrElse(() => TodoTitle.TryCreate("")) // Also fails
            .OrElse(() => TodoTitle.TryCreate("Default Todo")); // Succeeds

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Default Todo");
    }

    #endregion

    #region Example 5: Complete Workflow Using All Aliases

    [Fact]
    public void CompleteWorkflow_UsingAllAliases()
    {
        // Arrange
        var title = "Write documentation";
        var events = new List<string>();

        // Act - Complete workflow using all alias methods
        var result = TodoTitle.TryCreate(title)
            .Peek(t => events.Add($"Todo created: {t.Value}"))
            .Require(
                t => t.Value.Length >= 5,
                Error.Validation("Title must be at least 5 characters"))
            .Then(t => Result.Success(t.Value.ToUpperInvariant()))
            .Peek(upper => events.Add($"Todo processed: {upper}"))
            .OrElse(() => Result.Success("DEFAULT TODO"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("WRITE DOCUMENTATION");
        events.Should().HaveCount(2);
        events[0].Should().Be("Todo created: Write documentation");
        events[1].Should().Be("Todo processed: WRITE DOCUMENTATION");
    }

    [Fact]
    public void CompleteWorkflow_FailsAndUsesFallback()
    {
        // Arrange
        var title = "X"; // Too short - will fail Require
        var events = new List<string>();

        // Act
        var result = TodoTitle.TryCreate(title)
            .Peek(t => events.Add($"Todo created: {t.Value}"))
            .Require(
                t => t.Value.Length >= 5,
                Error.Validation("Title must be at least 5 characters"))
            .OrElse(() =>
            {
                events.Add("Using default due to validation failure");
                return TodoTitle.TryCreate("Default Todo");
            });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Default Todo");
        events.Should().HaveCount(2);
        events[0].Should().Be("Todo created: X");
        events[1].Should().Be("Using default due to validation failure");
    }

    #endregion

    #region Comparison: Aliases vs Standard FP Names

    [Fact]
    public void SideBySideComparison_AliasesVsStandardNames()
    {
        var title = "Buy groceries";
        var logs = new List<string>();

        // Using ALIASES (C# friendly)
        var resultWithAliases = TodoTitle.TryCreate(title)
            .Peek(t => logs.Add($"Created: {t.Value}"))
            .Require(t => t.Value.Length >= 5, Error.Validation("Too short"))
            .Then(t => Result.Success(t.Value.ToUpperInvariant()))
            .OrElse(() => Result.Success("FALLBACK"));

        logs.Clear();

        // Using STANDARD FP names (same functionality)
        var resultWithStandard = TodoTitle.TryCreate(title)
            .Tap(t => logs.Add($"Created: {t.Value}"))
            .Ensure(t => t.Value.Length >= 5, Error.Validation("Too short"))
            .Bind(t => Result.Success(t.Value.ToUpperInvariant()))
            .Compensate(() => Result.Success("FALLBACK"));

        // Both produce identical results
        resultWithAliases.IsSuccess.Should().Be(resultWithStandard.IsSuccess);
        resultWithAliases.Value.Should().Be(resultWithStandard.Value);
    }

    #endregion

    #region Async Examples

    [Fact]
    public async Task ThenAsync_ChainsAsyncOperations()
    {
        // Arrange
        var title = "Schedule meeting";

        // Act - Using ThenAsync instead of BindAsync
        var result = await TodoTitle.TryCreate(title)
            .ThenAsync(async t =>
            {
                await Task.Delay(10); // Simulate async work
                return Result.Success(t.Value.ToUpperInvariant());
            });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("SCHEDULE MEETING");
    }

    [Fact]
    public async Task PeekAsync_ExecutesAsyncSideEffects()
    {
        // Arrange
        var title = "Send email";
        var notifications = new List<string>();

        // Act - Using PeekAsync instead of TapAsync
        var result = await TodoTitle.TryCreate(title)
            .PeekAsync(async t =>
            {
                await Task.Delay(10); // Simulate async notification
                notifications.Add($"Email sent: {t.Value}");
            });

        // Assert
        result.IsSuccess.Should().BeTrue();
        notifications.Should().HaveCount(1);
        notifications[0].Should().Be("Email sent: Send email");
    }

    [Fact]
    public async Task OrElseAsync_AsyncFallback()
    {
        // Arrange
        var invalidTitle = "";

        // Act - Using OrElseAsync instead of CompensateAsync
        var result = await TodoTitle.TryCreate(invalidTitle)
            .OrElseAsync(async () =>
            {
                await Task.Delay(10); // Simulate fetching default from database
                return TodoTitle.TryCreate("Default from DB");
            });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Default from DB");
    }

    [Fact]
    public async Task CompleteAsyncWorkflow()
    {
        // Arrange
        var title = "Process order";
        var events = new List<string>();

        // Act - Complete async workflow with alias methods
        var intermediate = await TodoTitle.TryCreate(title)
            .PeekAsync(async t =>
            {
                await Task.Delay(5);
                events.Add($"Started: {t.Value}");
            })
            .ThenAsync(async t =>
            {
                // Validate length asynchronously
                await Task.Delay(5);
                if (t.Value.Length < 5)
                    return Result.Failure<string>(Error.Validation("Title too short"));

                await Task.Delay(5); // Simulate async processing
                return Result.Success(t.Value.ToUpperInvariant());
            });

        var result = await (await intermediate
            .PeekAsync(async upper =>
            {
                await Task.Delay(5);
                events.Add($"Completed: {upper}");
            }))
            .OrElseAsync(async () =>
            {
                await Task.Delay(5);
                return Result.Success("FALLBACK");
            });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("PROCESS ORDER");
        events.Should().HaveCount(2);
        events[0].Should().Be("Started: Process order");
        events[1].Should().Be("Completed: PROCESS ORDER");
    }

    #endregion
}
