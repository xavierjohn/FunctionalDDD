namespace RailwayOrientedProgramming.Tests.Results.Extensions;

public class CompensateTests
{
    bool _compensatingFunctionCalled;

    #region Compensate_Sync
    /// <summary>
    /// Given a successful result
    /// When Compensate is called
    /// Then the compensating function is not called.
    /// </summary>
    [Fact]
    public void Compensate_function_does_not_get_executed_for_successful_result()
    {
        Result<string> input = Result.Success("Success");
        Result<string> output = input.Compensate(GetErrorResult);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Success");
    }

    /// <summary>
    /// Given a failed result
    /// When Compensate is called with a compensating function that succeeds
    /// Then the compensating function is called and returns success result.
    /// </summary>
    [Fact]
    public void Compensate_function_gets_executed_for_failed_result_and_returns_success()
    {
        Result<string> input = Result.Failure<string>(Error.Unexpected("Error"));
        Result<string> output = input.Compensate(GetSuccessResult);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Compensated Success");
    }

    /// <summary>
    /// Given a failed result
    /// When Compensate is called with a compensating function that fails
    /// Then the compensating function is called and returns failed result.
    /// </summary>
    [Fact]
    public void Compensate_function_gets_executed_for_failed_result_and_returns_failure()
    {
        Result<string> input = Result.Failure<string>(Error.Unexpected("Error"));
        Result<string> output = input.Compensate(GetErrorResult);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsFailure.Should().BeTrue();
        output.Error.Should().Be(Error.Unexpected("Compensated Error"));
    }

    /// <summary>
    /// Given a failed result
    /// When Compensate is called with a compensating function that fails
    /// Then the compensating function with the Error is called and returns success.
    /// </summary>
    [Fact]
    public void Compensate_function_gets_executed_with_ErrorParam_for_failed_result_and_returns_success()
    {
        Result<string> input = Result.Failure<string>(Error.Unexpected("Error"));
        Result<string> output = input.Compensate(e => 
        {
            e.Should().Be(Error.Unexpected("Error"));
            return GetSuccessResult();
        });

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Compensated Success");
    }

    #endregion

    #region Compensate_async_Result_sync_func
    [Fact]
    public async Task Task_Result_success_and_compensate_func_does_not_execute()
    {
        Task<Result<string>> input = Task.FromResult(Result.Success("Success"));
        Result<string> output = await input.CompensateAsync(GetErrorResult);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_func_does_execute_and_succeed()
    {
        Task<Result<string>> input = Task.FromResult(Result.Failure<string>(Error.Unexpected("Error")));
        Result<string> output = await input.CompensateAsync(GetSuccessResult);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Compensated Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_func_does_execute_and_fail()
    {
        Task<Result<string>> input = Task.FromResult(Result.Failure<string>(Error.Unexpected("Error")));
        Result<string> output = await input.CompensateAsync(GetErrorResult);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsFailure.Should().BeTrue();
        output.Error.Should().Be(Error.Unexpected("Compensated Error"));
    }

    [Fact]
    public async Task Task_Compensate_function_gets_executed_with_ErrorParam_for_failed_result_and_returns_success()
    {
        Task<Result<string>> input = Task.FromResult(Result.Failure<string>(Error.Unexpected("Error")));
        Result<string> output = await input.CompensateAsync(e =>
        {
            e.Should().Be(Error.Unexpected("Error"));
            return GetSuccessResult();
        });

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Compensated Success");
    }
    #endregion

    #region Compensate_sync_Result_async_func
    [Fact]
    public async Task Result_success_and_compensate_async_func_does_not_execute()
    {
        Result<string> input = Result.Success("Success");
        Result<string> output = await input.CompensateAsync(GetErrorResultAsync);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Success");
    }

    [Fact]
    public async Task Result_failure_and_compensate_async_func_does_execute_and_succeed()
    {
        Result<string> input = Result.Failure<string>(Error.Unexpected("Error"));
        Result<string> output = await input.CompensateAsync(GetSuccessResultAsync);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Compensated Success");
    }

    [Fact]
    public async Task Result_failure_and_compensate_async_func_does_execute_and_fail()
    {
        Result<string> input = Result.Failure<string>(Error.Unexpected("Error"));
        Result<string> output = await input.CompensateAsync(GetErrorResultAsync);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsFailure.Should().BeTrue();
        output.Error.Should().Be(Error.Unexpected("Compensated Error"));
    }

    [Fact]
    public async Task Compensate_async_function_gets_executed_with_ErrorParam_for_failed_result_and_returns_success()
    {
        Result<string> input = Result.Failure<string>(Error.Unexpected("Error"));
        Result<string> output = await input.CompensateAsync(async e =>
        {
            e.Should().Be(Error.Unexpected("Error"));
            return await GetSuccessResultAsync();
        });

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Compensated Success");
    }

    #endregion

    #region Compensate_async_Result_async_func
    [Fact]
    public async Task Task_Result_success_and_compensate_async_func_does_not_execute()
    {
        Task<Result<string>> input = Task.FromResult(Result.Success("Success"));
        Result<string> output = await input.CompensateAsync(GetErrorResultAsync);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_async_func_does_execute_and_succeed()
    {
        Task<Result<string>> input = Task.FromResult(Result.Failure<string>(Error.Unexpected("Error")));
        Result<string> output = await input.CompensateAsync(GetSuccessResultAsync);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Compensated Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_async_func_does_execute_and_fail()
    {
        Task<Result<string>> input = Task.FromResult(Result.Failure<string>(Error.Unexpected("Error")));
        Result<string> output = await input.CompensateAsync(GetErrorResultAsync);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsFailure.Should().BeTrue();
        output.Error.Should().Be(Error.Unexpected("Compensated Error"));
    }

    [Fact]
    public async Task Task_Compensate_async_function_gets_executed_with_ErrorParam_for_failed_result_and_returns_success()
    {
        Task<Result<string>> input = Task.FromResult(Result.Failure<string>(Error.Unexpected("Error")));
        Result<string> output = await input.CompensateAsync(async e =>
        {
            e.Should().Be(Error.Unexpected("Error"));
            return await GetSuccessResultAsync();
        });

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Compensated Success");
    }
    #endregion

    #region Compensate_with_predicate_sync
    /// <summary>
    /// Given a successful result
    /// When Compensate with predicate is called
    /// Then the compensating function is not called.
    /// </summary>
    [Fact]
    public void Compensate_with_predicate_does_not_execute_for_successful_result()
    {
        Result<string> input = Result.Success("Success");
        Result<string> output = input.Compensate(e => e is NotFoundError, GetSuccessResult);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Success");
    }

    /// <summary>
    /// Given a failed result with NotFound error
    /// When Compensate is called with a predicate that checks for NotFound
    /// Then the compensating function is called and returns success result.
    /// </summary>
    [Fact]
    public void Compensate_with_predicate_executes_when_predicate_matches_NotFound()
    {
        Result<string> input = Result.Failure<string>(Error.NotFound("Resource not found"));
        Result<string> output = input.Compensate(e => e is NotFoundError, GetSuccessResult);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Compensated Success");
    }

    /// <summary>
    /// Given a failed result with Unexpected error
    /// When Compensate is called with a predicate that checks for NotFound
    /// Then the compensating function is not called and the original error is preserved.
    /// </summary>
    [Fact]
    public void Compensate_with_predicate_does_not_execute_when_predicate_does_not_match()
    {
        Result<string> input = Result.Failure<string>(Error.Unexpected("Unexpected error"));
        Result<string> output = input.Compensate(e => e is NotFoundError, GetSuccessResult);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsFailure.Should().BeTrue();
        output.Error.Should().Be(Error.Unexpected("Unexpected error"));
    }

    /// <summary>
    /// Given a failed result with NotFound error
    /// When Compensate is called with a predicate that checks for NotFound code
    /// Then the compensating function is called and returns success result.
    /// </summary>
    [Fact]
    public void Compensate_with_predicate_executes_when_predicate_matches_error_code()
    {
        Result<string> input = Result.Failure<string>(Error.NotFound("Resource not found"));
        Result<string> output = input.Compensate(e => e.Code == "not.found.error", GetSuccessResult);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Compensated Success");
    }

    #endregion

    #region Compensate_with_predicate_async_Result_sync_func
    [Fact]
    public async Task Task_Result_success_and_compensate_with_predicate_func_does_not_execute()
    {
        Task<Result<string>> input = Task.FromResult(Result.Success("Success"));
        Result<string> output = await input.CompensateAsync(e => e is NotFoundError, GetSuccessResult);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_with_predicate_func_does_execute_when_matches()
    {
        Task<Result<string>> input = Task.FromResult(Result.Failure<string>(Error.NotFound("Not found")));
        Result<string> output = await input.CompensateAsync(e => e is NotFoundError, GetSuccessResult);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Compensated Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_with_predicate_func_does_not_execute_when_not_matches()
    {
        Task<Result<string>> input = Task.FromResult(Result.Failure<string>(Error.Unexpected("Error")));
        Result<string> output = await input.CompensateAsync(e => e is NotFoundError, GetSuccessResult);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsFailure.Should().BeTrue();
        output.Error.Should().Be(Error.Unexpected("Error"));
    }
    #endregion

    #region Compensate_with_predicate_sync_Result_async_func
    [Fact]
    public async Task Result_success_and_compensate_with_predicate_async_func_does_not_execute()
    {
        Result<string> input = Result.Success("Success");
        Result<string> output = await input.CompensateAsync(e => e is NotFoundError, GetSuccessResultAsync);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Success");
    }

    [Fact]
    public async Task Result_failure_and_compensate_with_predicate_async_func_does_execute_when_matches()
    {
        Result<string> input = Result.Failure<string>(Error.NotFound("Not found"));
        Result<string> output = await input.CompensateAsync(e => e is NotFoundError, GetSuccessResultAsync);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Compensated Success");
    }

    [Fact]
    public async Task Result_failure_and_compensate_with_predicate_async_func_does_not_execute_when_not_matches()
    {
        Result<string> input = Result.Failure<string>(Error.Unexpected("Error"));
        Result<string> output = await input.CompensateAsync(e => e is NotFoundError, GetSuccessResultAsync);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsFailure.Should().BeTrue();
        output.Error.Should().Be(Error.Unexpected("Error"));
    }

    #endregion

    #region Compensate_with_predicate_async_Result_async_func
    [Fact]
    public async Task Task_Result_success_and_compensate_with_predicate_async_func_does_not_execute()
    {
        Task<Result<string>> input = Task.FromResult(Result.Success("Success"));
        Result<string> output = await input.CompensateAsync(e => e is NotFoundError, GetSuccessResultAsync);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_with_predicate_async_func_does_execute_when_matches()
    {
        Task<Result<string>> input = Task.FromResult(Result.Failure<string>(Error.NotFound("Not found")));
        Result<string> output = await input.CompensateAsync(e => e is NotFoundError, GetSuccessResultAsync);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("Compensated Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_with_predicate_async_func_does_not_execute_when_not_matches()
    {
        Task<Result<string>> input = Task.FromResult(Result.Failure<string>(Error.Unexpected("Error")));
        Result<string> output = await input.CompensateAsync(e => e is NotFoundError, GetSuccessResultAsync);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsFailure.Should().BeTrue();
        output.Error.Should().Be(Error.Unexpected("Error"));
    }
    #endregion

    private Result<string> GetSuccessResult()
    {
        _compensatingFunctionCalled = true;
        return Result.Success("Compensated Success");
    }

    private Result<string> GetErrorResult()
    {
        _compensatingFunctionCalled = true;
        return Result.Failure<string>(Error.Unexpected("Compensated Error"));
    }

    private Task<Result<string>> GetSuccessResultAsync()
    {
        _compensatingFunctionCalled = true;
        return Task.FromResult(Result.Success("Compensated Success"));
    }

    private Task<Result<string>> GetErrorResultAsync()
    {
        _compensatingFunctionCalled = true;
        return Task.FromResult(Result.Failure<string>(Error.Unexpected("Compensated Error")));
    }
}
