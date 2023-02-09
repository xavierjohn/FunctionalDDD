namespace RailwayOrientedProgramming.Tests.Functional.Results.Extensions;

using FunctionalDDD;
using RailwayOrientedProgramming.Tests.Results.Extensions;

public class BindTests_ValueTask_Left : OkTestsBase
{
    [Fact]
    public async ValueTask Bind_ValueTask_Left_T_K_returns_failure_and_does_not_execute_func()
    {
        var output = await ValueTask_Failure_T().OnOkAsync(Success_T_Func_K);

        AssertFailure(output);
    }

    [Fact]
    public async ValueTask Bind_ValueTask_Left_T_K_selects_new_result()
    {
        var output = await ValueTask_Success_T(T.Value).OnOkAsync(Success_T_Func_K);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }
}
