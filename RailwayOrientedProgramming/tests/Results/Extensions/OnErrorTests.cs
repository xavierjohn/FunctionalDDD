namespace RailwayOrientedProgramming.Tests.Results.Extensions;

using FunctionalDDD;

public class OnErrorTests
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
        Result<string, Error> input = Result.Success("Success");
        Result<string, Error> output = input.OnError(GetErrorResult);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsOk.Should().BeTrue();
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
        Result<string, Error> input = Result.Failure<string>(Error.Unexpected("Error"));
        Result<string, Error> output = input.OnError(GetSuccessResult);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsOk.Should().BeTrue();
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
        Result<string, Error> input = Result.Failure<string>(Error.Unexpected("Error"));
        Result<string, Error> output = input.OnError(GetErrorResult);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsError.Should().BeTrue();
        output.Error.Should().Be(Error.Unexpected("Compensated Error"));
    }
    #endregion

    #region Compensate_async_Result_sync_func
    [Fact]
    public async Task Task_Result_success_and_compensate_func_does_not_execute()
    {
        Task<Result<string, Error>> input = Task.FromResult(Result.Success("Success"));
        Result<string, Error> output = await input.OnErrorAsync(GetErrorResult);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsOk.Should().BeTrue();
        output.Ok.Should().Be("Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_func_does_execute_and_succeed()
    {
        Task<Result<string, Error>> input = Task.FromResult(Result.Failure<string>(Error.Unexpected("Error")));
        Result<string, Error> output = await input.OnErrorAsync(GetSuccessResult);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsOk.Should().BeTrue();
        output.Ok.Should().Be("Compensated Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_func_does_execute_and_fail()
    {
        Task<Result<string, Error>> input = Task.FromResult(Result.Failure<string>(Error.Unexpected("Error")));
        Result<string, Error> output = await input.OnErrorAsync(GetErrorResult);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsError.Should().BeTrue();
        output.Error.Should().Be(Error.Unexpected("Compensated Error"));
    }
    #endregion

    #region Compensate_async_Result_async_func
    [Fact]
    public async Task Task_Result_success_and_compensate_async_func_does_not_execute()
    {
        Task<Result<string, Error>> input = Task.FromResult(Result.Success("Success"));
        Result<string, Error> output = await input.OnErrorAsync(GetErrorResultAsync);

        _compensatingFunctionCalled.Should().BeFalse();
        output.IsOk.Should().BeTrue();
        output.Ok.Should().Be("Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_async_func_does_execute_and_succeed()
    {
        Task<Result<string, Error>> input = Task.FromResult(Result.Failure<string>(Error.Unexpected("Error")));
        Result<string, Error> output = await input.OnErrorAsync(GetSuccessResultAsync);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsOk.Should().BeTrue();
        output.Ok.Should().Be("Compensated Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_async_func_does_execute_and_fail()
    {
        Task<Result<string, Error>> input = Task.FromResult(Result.Failure<string>(Error.Unexpected("Error")));
        Result<string, Error> output = await input.OnErrorAsync(GetErrorResultAsync);

        _compensatingFunctionCalled.Should().BeTrue();
        output.IsError.Should().BeTrue();
        output.Error.Should().Be(Error.Unexpected("Compensated Error"));
    }
    #endregion

    private Result<string, Error> GetSuccessResult()
    {
        _compensatingFunctionCalled = true;
        return Result.Success("Compensated Success");
    }

    private Result<string, Error> GetErrorResult()
    {
        _compensatingFunctionCalled = true;
        return Result.Failure<string>(Error.Unexpected("Compensated Error"));
    }

    private Task<Result<string, Error>> GetSuccessResultAsync()
    {
        _compensatingFunctionCalled = true;
        return Task.FromResult(Result.Success("Compensated Success"));
    }

    private Task<Result<string, Error>> GetErrorResultAsync()
    {
        _compensatingFunctionCalled = true;
        return Task.FromResult(Result.Failure<string>(Error.Unexpected("Compensated Error")));
    }
}
