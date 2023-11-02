namespace RailwayOrientedProgramming.Tests.Results.Extensions;

using FunctionalDdd;

public class BindTests : BindBase
{

    [Fact]
    public void Bind_T_K_returns_failure_and_does_not_execute_func()
    {
        var output = Failure_T().Bind(Success_T_Func_K);

        AssertFailure(output);
    }

    [Fact]
    public void Bind_T_K_selects_new_result()
    {
        var output = Success_T(T.Value).Bind(Success_T_Func_K);

        FuncParam.Should().Be(T.Value);
        AssertSuccess(output);
    }
}
