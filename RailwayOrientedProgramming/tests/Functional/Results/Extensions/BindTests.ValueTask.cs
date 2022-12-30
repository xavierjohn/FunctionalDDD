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
}
