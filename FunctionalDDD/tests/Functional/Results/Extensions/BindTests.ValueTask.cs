namespace FunctionalDDD.Tests.ResultTests.Extensions;

public class BindTests_ValueTask : BindTestsBase
{
    [Fact]
    public async ValueTask Bind_ValueTask_T_K_returns_failure_and_does_not_execute_func()
    {
        Result<K> output = await ValueTask_Failure_T().BindAsync(Func_T_ValueTask_Success_K);

        AssertFailure(output);
    }

    [Fact]
    public async ValueTask Bind_ValueTask_T_K_selects_new_result()
    {
        Result<K> output = await ValueTask_Success_T(T.Value).BindAsync(Func_T_ValueTask_Success_K);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }

    [Fact]
    public async ValueTask Bind_ValueTask_E_selects_new_UnitResult()
    {
        UnitResult output = await ValueTask_UnitResult_Success_E().BindAsync(ValueTask_UnitResult_Success_E);

        AssertSuccess(output);
    }

    [Fact]
    public async ValueTask Bind_ValueTask_E_returns_UnitResult_failure_and_does_not_execute_func()
    {
        UnitResult output = await ValueTask_UnitResult_Failure_E().BindAsync(ValueTask_UnitResult_Success_E);

        AssertFailure(output);
    }

    [Fact]
    public async ValueTask Bind_ValueTask_E_selects_new_result()
    {
        UnitResult output = await ValueTask_UnitResult_Success_E().BindAsync(Func_ValueTask_Success_T_E);

        AssertSuccess(output);
    }

    [Fact]
    public async ValueTask Bind_ValueTask_E_returns_failure_and_does_not_execute_func()
    {
        UnitResult output = await ValueTask_UnitResult_Failure_E().BindAsync(Func_ValueTask_Success_T_E);

        AssertFailure(output);
    }

    [Fact]
    public async ValueTask Bind_ValueTask_T_E_selects_new_UnitResult()
    {
        UnitResult output = await Func_ValueTask_Success_T_E().BindAsync(Func_T_ValueTask_UnitResult_E);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }

    [Fact]
    public async ValueTask Bind_ValueTask_T_E_returns_failure_and_does_not_execute_func()
    {
        UnitResult output = await ValueTask_Failure_T_E().BindAsync(Func_T_ValueTask_UnitResult_E);

        AssertFailure(output);
    }
}
