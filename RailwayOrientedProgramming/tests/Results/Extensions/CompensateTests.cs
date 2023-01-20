namespace RailwayOrientedProgramming.Tests.Results.Extensions;

using FunctionalDDD;

public class CompensateTests
{
    bool _compensated;

    [Fact]
    public void Result_success_and_compensate_func_does_not_execute()
    {
        Result<string> input = Result.Success("Success");
        Result<string> output = input.Compensate(GetErrorResult);

        _compensated.Should().BeFalse();
        output.Value.Should().Be("Success");
    }

    [Fact]
    public void Result_failure_and_compensate_func_does_execute_and_succeed()
    {
        Result<string> input = Result.Failure<string>(Error.Unexpected("Error"));
        Result<string> output = input.Compensate(GetSuccessResult);

        _compensated.Should().BeTrue();
        output.Value.Should().Be("Compensate Success");
    }

    [Fact]
    public void Result_failure_and_compensate_func_does_execute_and_fail()
    {
        Result<string> input = Result.Failure<string>(Error.Unexpected("Error"));
        Result<string> output = input.Compensate(GetErrorResult);

        _compensated.Should().BeTrue();
        output.Error.Should().Be(Error.Unexpected("Compensate Error"));
    }

    [Fact]
    public async Task Task_Result_success_and_compensate_func_does_not_execute()
    {
        Task<Result<string>> input = Task.FromResult(Result.Success("Success"));
        Result<string> output = await input.CompensateAsync(GetErrorResult);

        _compensated.Should().BeFalse();
        output.Value.Should().Be("Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_func_does_execute_and_succeed()
    {
        Task<Result<string>> input = Task.FromResult(Result.Failure<string>(Error.Unexpected("Error")));
        Result<string> output = await input.CompensateAsync(GetSuccessResult);

        _compensated.Should().BeTrue();
        output.Value.Should().Be("Compensate Success");
    }

    [Fact]
    public async Task Task_Result_failure_and_compensate_func_does_execute_and_fail()
    {
        Task<Result<string>> input = Task.FromResult(Result.Failure<string>(Error.Unexpected("Error")));
        Result<string> output = await input.CompensateAsync(GetErrorResult);

        _compensated.Should().BeTrue();
        output.Error.Should().Be(Error.Unexpected("Compensate Error"));
    }

    private Result<string> GetSuccessResult(ErrorList errors)
    {
        _compensated = true;
        return Result.Success("Compensate Success");
    }

    private Result<string> GetErrorResult(ErrorList errors)
    {
        _compensated = true;
        return Result.Failure<string>(Error.Unexpected("Compensate Error"));
    }
}
