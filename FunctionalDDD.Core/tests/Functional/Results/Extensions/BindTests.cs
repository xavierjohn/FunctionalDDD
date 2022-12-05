
namespace FunctionalDDD.Core.Tests.Functional.Results.Extensions;

public class BindTests : BindTestsBase
{

    [Fact]
    public void Bind_T_K_returns_failure_and_does_not_execute_func()
    {
        Result<K> output = Failure_T().Bind(Success_T_Func_K);

        AssertFailure(output);
    }

    [Fact]
    public void Bind_T_K_selects_new_result()
    {
        Result<K> output = Success_T(T.Value).Bind(Success_T_Func_K);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }
    
    [Fact]
    public void Bind_T_selects_new_UnitResult()
    {
        UnitResult output = Success_T_E().Bind(UnitResult_E_T);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }

    [Fact]
    public void Bind_T_returns_failure_and_does_not_execute_func()
    {
        UnitResult output = Failure_T_E().Bind(UnitResult_E_T);

        AssertFailure(output);
    }

    [Fact]
    public void Bind_E_selects_new_result()
    {
        UnitResult output = UnitResult_Success_E().Bind(Success_T_E);

        AssertSuccess(output);
    }

    [Fact]
    public void Bind_E_returns_failure_and_does_not_execute_func()
    {
        UnitResult output = UnitResult_Failure_E().Bind(Success_T_E);

        AssertFailure(output);
    }

    [Fact]
    public void Bind_E_selects_new_UnitResult()
    {
        UnitResult output = UnitResult_Success_E().Bind(UnitResult_Success_E);

        AssertSuccess(output);
    }

    [Fact]
    public void Bind_E_returns_UnitResult_failure_and_does_not_execute_func()
    {
        UnitResult output = UnitResult_Failure_E().Bind(UnitResult_Success_E);

        AssertFailure(output);
    }

    [Fact]
    public async Task Bind_Task_E_selects_new_result()
    {
        UnitResult output = await Task_UnitResult_Success_E().BindAsync(Task_Success_T_E);

        AssertSuccess(output);
    }

    [Fact]
    public async Task Bind_Task_E_returns_failure_and_does_not_execute_func()
    {
        UnitResult output = await Task_UnitResult_Failure_E().BindAsync(Task_Success_T_E);

        AssertFailure(output);
    }
}
