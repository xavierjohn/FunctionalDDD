namespace RailwayOrientedProgramming.Tests.Results.Extensions;

using System.Diagnostics;

public class DebugTests : TestBase
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Debug_returns_same_result(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);

        var returned = result.Debug();

        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Debug_with_message_returns_same_result(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);

        var returned = result.Debug("Test message");

        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DebugDetailed_returns_same_result(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);

        var returned = result.DebugDetailed();

        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DebugDetailed_with_message_returns_same_result(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);

        var returned = result.DebugDetailed("Test message");

        returned.Should().Be(result);
    }

    [Fact]
    public void DebugDetailed_with_validation_error_returns_same_result()
    {
        var validationError = Error.Validation("Test error", "TestField");
        Result<T> result = Result.Failure<T>(validationError);

        var returned = result.DebugDetailed("Validation test");

        returned.Should().Be(result);
    }

    [Fact]
    public void DebugDetailed_with_aggregate_error_returns_same_result()
    {
        var aggregateError = new AggregateError([Error1, Error2]);
        Result<T> result = Result.Failure<T>(aggregateError);

        var returned = result.DebugDetailed("Aggregate test");

        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DebugWithStack_returns_same_result(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);

        var returned = result.DebugWithStack();

        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DebugWithStack_with_message_returns_same_result(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);

        var returned = result.DebugWithStack("Test message");

        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DebugWithStack_without_stack_trace_returns_same_result(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);

        var returned = result.DebugWithStack("Test message", includeStackTrace: false);

        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DebugOnSuccess_executes_action_on_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);
        var actionExecuted = false;

        var returned = result.DebugOnSuccess(_ => actionExecuted = true);

#if DEBUG
        actionExecuted.Should().Be(isSuccess);
#else
        actionExecuted.Should().BeFalse();
#endif
        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DebugOnFailure_executes_action_on_failure_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);
        var actionExecuted = false;

        var returned = result.DebugOnFailure(_ => actionExecuted = true);

#if DEBUG
        actionExecuted.Should().Be(!isSuccess);
#else
        actionExecuted.Should().BeFalse();
#endif
        returned.Should().Be(result);
    }

    [Fact]
    public void Debug_can_be_chained_with_other_operations()
    {
        var result = Result.Success("Hello")
            .Debug("Initial")
            .Bind(s => Result.Success(s + " World"))
            .Debug("After Bind")
            .Tap(s => s.Should().Be("Hello World"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello World");
    }

    #region Async Tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DebugAsync_returns_same_result(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);

        var returned = await result.AsTask().DebugAsync();

        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DebugAsync_with_message_returns_same_result(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);

        var returned = await result.AsTask().DebugAsync("Test message");

        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DebugDetailedAsync_returns_same_result(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);

        var returned = await result.AsTask().DebugDetailedAsync();

        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DebugDetailedAsync_with_message_returns_same_result(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);

        var returned = await result.AsTask().DebugDetailedAsync("Test message");

        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DebugWithStackAsync_returns_same_result(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);

        var returned = await result.AsTask().DebugWithStackAsync();

        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DebugWithStackAsync_with_message_returns_same_result(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);

        var returned = await result.AsTask().DebugWithStackAsync("Test message");

        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DebugWithStackAsync_without_stack_trace_returns_same_result(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);

        var returned = await result.AsTask().DebugWithStackAsync("Test message", includeStackTrace: false);

        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DebugOnSuccessAsync_with_sync_action_executes_on_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);
        var actionExecuted = false;

        var returned = await result.AsTask().DebugOnSuccessAsync(_ => actionExecuted = true);

#if DEBUG
        actionExecuted.Should().Be(isSuccess);
#else
        actionExecuted.Should().BeFalse();
#endif
        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DebugOnSuccessAsync_with_async_action_executes_on_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);
        var actionExecuted = false;

        var returned = await result.AsTask().DebugOnSuccessAsync(_ =>
        {
            actionExecuted = true;
            return Task.CompletedTask;
        });

#if DEBUG
        actionExecuted.Should().Be(isSuccess);
#else
        actionExecuted.Should().BeFalse();
#endif
        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DebugOnFailureAsync_with_sync_action_executes_on_failure_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);
        var actionExecuted = false;

        var returned = await result.AsTask().DebugOnFailureAsync(_ => actionExecuted = true);

#if DEBUG
        actionExecuted.Should().Be(!isSuccess);
#else
        actionExecuted.Should().BeFalse();
#endif
        returned.Should().Be(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DebugOnFailureAsync_with_async_action_executes_on_failure_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value1, Error1);
        var actionExecuted = false;

        var returned = await result.AsTask().DebugOnFailureAsync(_ =>
        {
            actionExecuted = true;
            return Task.CompletedTask;
        });

#if DEBUG
        actionExecuted.Should().Be(!isSuccess);
#else
        actionExecuted.Should().BeFalse();
#endif
        returned.Should().Be(result);
    }

    [Fact]
    public async Task DebugAsync_can_be_chained_with_other_async_operations()
    {
        var result = await Task.FromResult(Result.Success("Hello"))
            .DebugAsync("Initial")
            .BindAsync(s => Task.FromResult(Result.Success(s + " World")))
            .DebugAsync("After Bind")
            .TapAsync(s => s.Should().Be("Hello World"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello World");
    }

    #endregion

    #region Activity Tests (DEBUG only)

#if DEBUG
    [Fact]
    public void Debug_creates_activity_with_success_result()
    {
        var debugActivityCreated = false;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity =>
            {
                if (activity.DisplayName.StartsWith("Debug:", StringComparison.Ordinal) || activity.DisplayName == "Debug")
                    debugActivityCreated = true;
            }
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success("Test value");
        result.Debug("Test message");

        debugActivityCreated.Should().BeTrue();
    }

    [Fact]
    public void Debug_creates_activity_with_failure_result()
    {
        var debugActivityCreated = false;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity =>
            {
                if (activity.DisplayName.StartsWith("Debug:", StringComparison.Ordinal) || activity.DisplayName == "Debug")
                    debugActivityCreated = true;
            }
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Failure<string>(Error.Unexpected("Test error"));
        result.Debug("Test message");

        debugActivityCreated.Should().BeTrue();
    }

    [Fact]
    public void DebugOnSuccess_creates_activity_when_result_is_success()
    {
        var activityStarted = false;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity =>
            {
                if (activity.DisplayName == "Debug: OnSuccess")
                    activityStarted = true;
            }
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success("Test value");
        result.DebugOnSuccess(_ => { });

        activityStarted.Should().BeTrue();
    }

    [Fact]
    public void DebugOnFailure_creates_activity_when_result_is_failure()
    {
        var activityStarted = false;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity =>
            {
                if (activity.DisplayName == "Debug: OnFailure")
                    activityStarted = true;
            }
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Failure<string>(Error.Unexpected("Test error"));
        result.DebugOnFailure(_ => { });

        activityStarted.Should().BeTrue();
    }

    [Fact]
    public void Debug_creates_activity_with_correct_name_and_tags_for_success()
    {
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity =>
            {
                if (activity.DisplayName == "Debug: Test message")
                    capturedActivity = activity;
            }
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success("Test value");
        result.Debug("Test message");

        capturedActivity.Should().NotBeNull();
        capturedActivity!.DisplayName.Should().Be("Debug: Test message");
        capturedActivity.Tags.Should().Contain(t => t.Key == "debug.result.status" && t.Value == "Success");
        capturedActivity.Tags.Should().Contain(t => t.Key == "debug.result.value");
        capturedActivity.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public void Debug_creates_activity_with_correct_name_and_tags_for_failure()
    {
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity =>
            {
                if (activity.DisplayName == "Debug: Error test")
                    capturedActivity = activity;
            }
        };
        ActivitySource.AddActivityListener(listener);

        var error = Error.NotFound("User not found");
        var result = Result.Failure<string>(error);
        result.Debug("Error test");

        capturedActivity.Should().NotBeNull();
        capturedActivity!.DisplayName.Should().Be("Debug: Error test");
        capturedActivity.Tags.Should().Contain(t => t.Key == "debug.result.status" && t.Value == "Failure");
        capturedActivity.Tags.Should().Contain(t => t.Key == "debug.error.code" && t.Value == "not.found.error");
        capturedActivity.Tags.Should().Contain(t => t.Key == "debug.error.detail" && t.Value == "User not found");
        capturedActivity.Status.Should().Be(ActivityStatusCode.Error);
    }

    [Fact]
    public void DebugDetailed_includes_type_information_for_success()
    {
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity =>
            {
                if (activity.DisplayName == "Debug: Detailed test (Detailed)")
                    capturedActivity = activity;
            }
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success(42);
        result.DebugDetailed("Detailed test");

        capturedActivity.Should().NotBeNull();
        capturedActivity!.Tags.Should().Contain(t => t.Key == "debug.result.type" && t.Value == "Int32");
        capturedActivity.Tags.Should().Contain(t => t.Key == "debug.result.value" && t.Value == "42");
    }

    [Fact]
    public void DebugDetailed_includes_validation_error_field_details()
    {
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity =>
            {
                if (activity.DisplayName.Contains("Validation error test", StringComparison.Ordinal))
                    capturedActivity = activity;
            },
            ActivityStopped = activity =>
            {
                if (activity.DisplayName.Contains("Validation error test", StringComparison.Ordinal))
                    capturedActivity = activity;
            }
        };
        ActivitySource.AddActivityListener(listener);

        var validationError = Error.Validation("Email is required", "email")
            .And("password", "Password too short");
        var result = Result.Failure<string>(validationError);
        result.DebugDetailed("Validation error test");

        capturedActivity.Should().NotBeNull();
        capturedActivity!.Tags.Should().Contain(t => t.Key == "debug.error.type" && t.Value == "ValidationError");
        capturedActivity.Tags.Should().Contain(t => t.Key == "debug.error.validation.field[0].name" && t.Value == "email");
        capturedActivity.Tags.Should().Contain(t => t.Key == "debug.error.validation.field[1].name" && t.Value == "password");
    }

    [Fact]
    public void DebugDetailed_includes_aggregate_error_details()
    {
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity =>
            {
                if (activity.DisplayName.Contains("Aggregate error test", StringComparison.Ordinal))
                    capturedActivity = activity;
            },
            ActivityStopped = activity =>
            {
                if (activity.DisplayName.Contains("Aggregate error test", StringComparison.Ordinal))
                    capturedActivity = activity;
            }
        };
        ActivitySource.AddActivityListener(listener);

        var error1 = Error.NotFound("User not found");
        var error2 = Error.Unauthorized("Not authorized");
        var aggregateError = new AggregateError([error1, error2]);
        var result = Result.Failure<string>(aggregateError);
        result.DebugDetailed("Aggregate error test");

        capturedActivity.Should().NotBeNull();
        capturedActivity!.Tags.Should().Contain(t => t.Key == "debug.error.type" && t.Value == "AggregateError");
        capturedActivity.Tags.Should().Contain(t => t.Key == "debug.error.aggregate[0].code" && t.Value == "not.found.error");
        capturedActivity.Tags.Should().Contain(t => t.Key == "debug.error.aggregate[1].code" && t.Value == "unauthorized.error");
    }

    [Fact]
    public void DebugWithStack_includes_stack_trace_information()
    {
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity =>
            {
                if (activity.DisplayName.StartsWith("Debug:", StringComparison.Ordinal) && 
                    activity.DisplayName.Contains("(with stack)", StringComparison.Ordinal))
                    capturedActivity = activity;
            }
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success("Test");
        result.DebugWithStack("Stack test");

        capturedActivity.Should().NotBeNull();
        capturedActivity!.Tags.Should().Contain(t => t.Key.StartsWith("debug.stack[", StringComparison.Ordinal));
        capturedActivity.Tags.Should().Contain(t => t.Key == "debug.stack[0].method");
    }

    [Fact]
    public void DebugWithStack_excludes_stack_trace_when_disabled()
    {
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity =>
            {
                if (activity.DisplayName.StartsWith("Debug:", StringComparison.Ordinal) && 
                    activity.DisplayName.Contains("(with stack)", StringComparison.Ordinal))
                    capturedActivity = activity;
            }
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success("Test");
        result.DebugWithStack("No stack test", includeStackTrace: false);

        capturedActivity.Should().NotBeNull();
        capturedActivity!.Tags.Should().NotContain(t => t.Key.StartsWith("debug.stack[", StringComparison.Ordinal));
    }

    [Fact]
    public void DebugOnSuccess_creates_activity_with_correct_status()
    {
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity =>
            {
                if (activity.DisplayName == "Debug: OnSuccess")
                    capturedActivity = activity;
            }
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success("Test value");
        result.DebugOnSuccess(_ => { });

        capturedActivity.Should().NotBeNull();
        capturedActivity!.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public void DebugOnFailure_creates_activity_with_error_status_and_tags()
    {
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity =>
            {
                if (activity.DisplayName == "Debug: OnFailure")
                    capturedActivity = activity;
            }
        };
        ActivitySource.AddActivityListener(listener);

        var error = Error.BadRequest("Invalid request");
        var result = Result.Failure<string>(error);
        result.DebugOnFailure(_ => { });

        capturedActivity.Should().NotBeNull();
        capturedActivity!.Status.Should().Be(ActivityStatusCode.Error);
        capturedActivity.Tags.Should().Contain(t => t.Key == "debug.error.code" && t.Value == "bad.request.error");
    }

    [Fact]
    public void Debug_without_message_creates_activity_with_default_name()
    {
        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Functional DDD ROP",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity =>
            {
                if (activity.DisplayName == "Debug")
                    capturedActivity = activity;
            }
        };
        ActivitySource.AddActivityListener(listener);

        var result = Result.Success("Test");
        result.Debug();

        capturedActivity.Should().NotBeNull();
        capturedActivity!.DisplayName.Should().Be("Debug");
    }
#endif

    #endregion
}
