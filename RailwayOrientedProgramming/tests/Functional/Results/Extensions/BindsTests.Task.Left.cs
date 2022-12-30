namespace RailwayOrientedProgramming.Tests.Functional.Results.Extensions;

using FunctionalDDD;

public class BindTests_Task_Left : BindTestsBase
{
    [Fact]
    public async Task Bind_Task_Left_T_K_returns_failure_and_does_not_execute_func()
    {
        var output = await Task_Failure_T().BindAsync(Success_T_Func_K);

        AssertFailure(output);
    }

    [Fact]
    public async Task Bind_Task_Left_T_K_selects_new_result()
    {
        var output = await Task_Success_T(T.Value).BindAsync(Success_T_Func_K);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }

    [Fact]
    public async Task Bind_Task_Left_T_K_E_returns_failure_and_does_not_execute_func()
    {
        var output = await Task_Failure_T_E().BindAsync(Success_T_E_Func_K);

        AssertFailure(output);
    }
}
