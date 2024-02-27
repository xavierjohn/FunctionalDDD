namespace RailwayOrientedProgramming.Tests.Functional.Results.Extensions;

using FunctionalDdd;
using RailwayOrientedProgramming.Tests.Results.Extensions;

public class BindTests_Task : BindBase
{
    [Fact]
    public async Task Bind_Task_T_K_returns_failure_and_does_not_execute_func()
    {
        Result<K> output = await Task_Failure_T().BindAsync(Func_T_Task_Success_K);

        AssertFailure(output);
    }

    [Fact]
    public async Task Bind_Task_T_K_selects_new_result()
    {
        Result<K> output = await Task_Success_T(T.Value).BindAsync(Func_T_Task_Success_K);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }

    [Fact]
    public async Task OnSuccess_Task_Left_T_K_returns_failure_and_does_not_execute_func()
    {
        var output = await Task_Failure_T().BindAsync(Success_T_Func_K);

        AssertFailure(output);
    }

    [Fact]
    public async Task OnSuccess_Task_Left_T_K_selects_new_result()
    {
        var output = await Task_Success_T(T.Value).BindAsync(Success_T_Func_K);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }

    [Fact]
    public async Task OnSuccess_Task_Left_T_K_E_returns_failure_and_does_not_execute_func()
    {
        var output = await Task_Failure_T_E().BindAsync(Success_T_E_Func_K);

        AssertFailure(output);
    }

    [Fact]
    public async Task OnSuccess_Task_Right_T_K_returns_failure_and_does_not_execute_func()
    {
        var output = await Failure_T().BindAsync(Func_T_Task_Success_K);

        AssertFailure(output);
    }

    [Fact]
    public async Task OnSuccess_Task_Right_T_K_selects_new_result()
    {
        var output = await Success_T(T.Value).BindAsync(Func_T_Task_Success_K);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }

    [Fact]
    public async Task OnSuccess_Tuple_execute_async_result()
    {
        var result = Result.Success((5, "Hello"));
        var output = await result.BindAsync((val, str) => ReturnFive(val, str));

        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be(5);

        Task<Result<int>> ReturnFive(int val, string str)
        {
            val.Should().Be(5);
            str.Should().Be("Hello");
            return Task.FromResult(Result.Success(5));
        }
    }
}
