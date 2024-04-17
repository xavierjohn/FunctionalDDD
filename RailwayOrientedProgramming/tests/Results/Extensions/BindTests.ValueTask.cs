namespace RailwayOrientedProgramming.Tests.Functional.Results.Extensions;

using FunctionalDdd;
using RailwayOrientedProgramming.Tests.Results.Extensions;

public class BindTests_ValueTask : BindBase
{
    [Fact]
    public async Task Bind_ValueTask_T_K_returns_failure_and_does_not_execute_func()
    {
        Result<K> output = await ValueTask_Failure_T().BindAsync(Func_T_ValueTask_Success_K);

        AssertFailure(output);
    }

    [Fact]
    public async Task Bind_ValueTask_T_K_selects_new_result()
    {
        Result<K> output = await ValueTask_Success_T(T.Value).BindAsync(Func_T_ValueTask_Success_K);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }

    #region Left
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
    #endregion

    #region Right
    [Fact]
    public async Task Bind_ValueTask_Right_T_K_returns_failure_and_does_not_execute_func()
    {
        Result<K> output = await Failure_T().BindAsync(Func_T_ValueTask_Success_K);

        AssertFailure(output);
    }

    [Fact]
    public async Task Bind_ValueTask_Right_T_K_selects_new_result()
    {
        Result<K> output = await Success_T(T.Value).BindAsync(Func_T_ValueTask_Success_K);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }
    #endregion
}
