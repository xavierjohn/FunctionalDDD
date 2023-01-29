namespace RailwayOrientedProgramming.Tests.Results.Extensions;

using FunctionalDDD;

public class OnFailureCompensateTests
{
    bool _compensatingFunctionCalled;

    #region Compensate_Sync
    /// <summary>
    /// Given a successful result
    /// When Compensate is called
    /// Then the compensating function is not called.
    /// </summary>
    [Fact]
    public void Result_success_then_compensate_func_does_not_execute()
    {
        Result<string, Err> input = Result.Success("Success");
        Result<string, Err> output = input.OnFailureCompensate(GetErrorResult);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsSuccess.Should().BeTrue();
        output.Ok.Should().Be("Success");
    }

    /// <summary>
    /// Given a failed result
    /// When Compensate is called with a compensating function that succeeds
    /// Then the compensating function is called and returns success result.
    /// </summary>
    [Fact]
    public void Result_failure_and_compensate_func_does_execute_and_succeed()
    {
        Result<string, Err> input = Result.Failure<string>(Err.Unexpected("Error"));
        Result<string, Err> output = input.OnFailureCompensate(GetSuccessResult);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Ok.Should().Be("Compensated Success");
    }

    /// <summary>
    /// Given a failed result
    /// When Compensate is called with a compensating function that fails
    /// Then the compensating function is called and returns failed result.
    /// </summary>
    [Fact]
    public void Result_failure_and_compensate_func_does_execute_and_fail()
    {
        Result<string, Err> input = Result.Failure<string>(Err.Unexpected("Error"));
        Result<string, Err> output = input.OnFailureCompensate(GetErrorResult);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsFailure.Should().BeTrue();
        output.Err.Should().Be(Err.Unexpected("Compensated Error"));
    }
    #endregion

    #region Compensate_async_Result_sync_func
    [Fact]
    public async Task Task_Result_success_and_compensate_func_does_not_execute()
    {
        Task<Result<string, Err>> input = Task.FromResult(Result.Success("Success"));
        Result<string, Err> output = await input.OnFailureCompensateAsync(GetErrorResult);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsSuccess.Should().BeTrue();
        output.Ok.Should().Be("Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_func_does_execute_and_succeed()
    {
        Task<Result<string, Err>> input = Task.FromResult(Result.Failure<string>(Err.Unexpected("Error")));
        Result<string, Err> output = await input.OnFailureCompensateAsync(GetSuccessResult);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Ok.Should().Be("Compensated Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_func_does_execute_and_fail()
    {
        Task<Result<string, Err>> input = Task.FromResult(Result.Failure<string>(Err.Unexpected("Error")));
        Result<string, Err> output = await input.OnFailureCompensateAsync(GetErrorResult);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsFailure.Should().BeTrue();
        output.Err.Should().Be(Err.Unexpected("Compensated Error"));
    }
    #endregion

    #region Compensate_async_Result_async_func
    [Fact]
    public async Task Task_Result_success_and_compensate_async_func_does_not_execute()
    {
        Task<Result<string, Err>> input = Task.FromResult(Result.Success("Success"));
        Result<string, Err> output = await input.OnFailureCompensateAsync(GetErrorResultAsync);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsSuccess.Should().BeTrue();
        output.Ok.Should().Be("Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_async_func_does_execute_and_succeed()
    {
        Task<Result<string, Err>> input = Task.FromResult(Result.Failure<string>(Err.Unexpected("Error")));
        Result<string, Err> output = await input.OnFailureCompensateAsync(GetSuccessResultAsync);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Ok.Should().Be("Compensated Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_async_func_does_execute_and_fail()
    {
        Task<Result<string, Err>> input = Task.FromResult(Result.Failure<string>(Err.Unexpected("Error")));
        Result<string, Err> output = await input.OnFailureCompensateAsync(GetErrorResultAsync);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsFailure.Should().BeTrue();
        output.Err.Should().Be(Err.Unexpected("Compensated Error"));
    }
    #endregion

    private Result<string, Err> GetSuccessResult()
    {
        _compensatingFunctionCalled = true;
        return Result.Success("Compensated Success");
    }

    private Result<string, Err> GetErrorResult()
    {
        _compensatingFunctionCalled = true;
        return Result.Failure<string>(Err.Unexpected("Compensated Error"));
    }

    private Task<Result<string, Err>> GetSuccessResultAsync()
    {
        _compensatingFunctionCalled = true;
        return Task.FromResult(Result.Success("Compensated Success"));
    }

    private Task<Result<string, Err>> GetErrorResultAsync()
    {
        _compensatingFunctionCalled = true;
        return Task.FromResult(Result.Failure<string>(Err.Unexpected("Compensated Error")));
    }
}
