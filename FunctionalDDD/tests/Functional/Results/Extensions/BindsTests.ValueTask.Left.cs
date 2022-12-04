namespace FunctionalDDD.Tests.ResultTests.Extensions;

public class BindTests_ValueTask_Left : BindTestsBase
{
    [Fact]
    public async ValueTask Bind_ValueTask_Left_T_K_returns_failure_and_does_not_execute_func()
    {
        Result<K> output = await ValueTask_Failure_T().BindAsync(Success_T_Func_K);

        AssertFailure(output);
    }

    [Fact]
    public async ValueTask Bind_ValueTask_Left_T_K_selects_new_result()
    {
        Result<K> output = await ValueTask_Success_T(T.Value).BindAsync(Success_T_Func_K);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }

    [Fact]
    public async ValueTask Bind_ValueTask_Left_E_selects_new_result()
    {
        UnitResult output = await ValueTask_UnitResult_Success_E().BindAsync(Success_T_E);

        AssertSuccess(output);
    }

    [Fact]
    public async ValueTask Bind_ValueTask_Left_E_returns_failure_and_does_not_execute_func()
    {
        UnitResult output = await ValueTask_UnitResult_Failure_E().BindAsync(Success_T_E);

        AssertFailure(output);
    }

    [Fact]
    public async ValueTask Bind_ValueTask_Left_E_selects_new_UnitResult()
    {
        UnitResult output = await ValueTask_UnitResult_Success_E().BindAsync(UnitResult_Success_E);

        AssertSuccess(output);
    }

    [Fact]
    public async ValueTask Bind_ValueTask_Left_E_returns_UnitResult_failure_and_does_not_execute_func()
    {
        UnitResult output = await ValueTask_UnitResult_Failure_E().BindAsync(UnitResult_Success_E);

        AssertFailure(output);
    }

    [Fact]
    public async ValueTask Bind_ValueTask_Left_T_E_selects_new_UnitResult()
    {
        UnitResult output = await Func_ValueTask_Success_T_E().BindAsync(UnitResult_E_T);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }

    [Fact]
    public async ValueTask Bind_ValueTask_Left_T_E_returns_failure_and_does_not_execute_func()
    {
        UnitResult output = await ValueTask_Failure_T_E().BindAsync(UnitResult_E_T);

        AssertFailure(output);
    }
}
