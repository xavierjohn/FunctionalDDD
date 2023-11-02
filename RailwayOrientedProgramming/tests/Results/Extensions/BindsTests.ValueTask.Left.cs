namespace RailwayOrientedProgramming.Tests.Functional.Results.Extensions;

using FunctionalDdd;
using RailwayOrientedProgramming.Tests.Results.Extensions;

public class BindTests_ValueTask_Left : BindBase
{
    [Fact]
    public async Task Bind_ValueTask_Left_T_K_returns_failure_and_does_not_execute_func()
    {
        var output = await ValueTask_Failure_T().BindAsync(Success_T_Func_K);

        AssertFailure(output);
    }

    [Fact]
    public async Task Bind_ValueTask_Left_T_K_selects_new_result()
    {
        var output = await ValueTask_Success_T(T.Value).BindAsync(Success_T_Func_K);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }
}
