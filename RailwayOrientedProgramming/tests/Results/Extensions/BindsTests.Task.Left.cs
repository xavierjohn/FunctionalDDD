namespace RailwayOrientedProgramming.Tests.Functional.Results.Extensions;

using FunctionalDDD;
using RailwayOrientedProgramming.Tests.Results.Extensions;

public class BindTests_Task_Left : OkTestsBase
{
    [Fact]
    public async Task Bind_Task_Left_T_K_returns_failure_and_does_not_execute_func()
    {
        var output = await Task_Failure_T().IfOkAsync(Success_T_Func_K);

        AssertFailure(output);
    }

    [Fact]
    public async Task Bind_Task_Left_T_K_selects_new_result()
    {
        var output = await Task_Success_T(T.Value).IfOkAsync(Success_T_Func_K);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }

    [Fact]
    public async Task Bind_Task_Left_T_K_E_returns_failure_and_does_not_execute_func()
    {
        var output = await Task_Failure_T_E().IfOkAsync(Success_T_E_Func_K);

        AssertFailure(output);
    }
}
