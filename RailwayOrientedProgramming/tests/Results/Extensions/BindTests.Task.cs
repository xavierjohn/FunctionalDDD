namespace RailwayOrientedProgramming.Tests.Functional.Results.Extensions;

using FunctionalDDD;
using RailwayOrientedProgramming.Tests.Results.Extensions;

public class BindTests_Task : BindTestsBase
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
}
