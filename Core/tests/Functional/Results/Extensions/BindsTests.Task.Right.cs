namespace FunctionalDDD.Core.Tests.ResultTests.Extensions;

public class BindTests_Task_Right : BindTestsBase
{
    [Fact]
    public async Task Bind_Task_Right_T_K_returns_failure_and_does_not_execute_func()
    {
        Result<K> output = await Failure_T().BindAsync(Func_T_Task_Success_K);

        AssertFailure(output);
    }

    [Fact]
    public async Task Bind_Task_Right_T_K_selects_new_result()
    {
        Result<K> output = await Success_T(T.Value).BindAsync(Func_T_Task_Success_K);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }
}
