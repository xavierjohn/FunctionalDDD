namespace FunctionalDDD.Tests.ResultTests.Extensions;

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

    [Fact]
    public async Task Bind_Task_T_E_returns_failure_and_does_not_execute_func()
    {
        UnitResult output = await Task_Failure_T_E().BindAsync(Func_T_Task_UnitResult_E);

        AssertFailure(output);
    }

    [Fact]
    public async Task Bind_Task_E_selects_new_UnitResult()
    {
        UnitResult output = await Task_UnitResult_Success_E().BindAsync(Task_UnitResult_Success_E);

        AssertSuccess(output);
    }

    [Fact]
    public async Task Bind_Task_E_returns_UnitResult_failure_and_does_not_execute_func()
    {
        UnitResult output = await Task_UnitResult_Failure_E().BindAsync(Task_UnitResult_Success_E);

        AssertFailure(output);
    }
}
