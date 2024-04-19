namespace RailwayOrientedProgramming.Tests.Results.Extensions;

public class BindTests : BindBase
{

    [Fact]
    public void Bind_success_result_executes_functions_return_K()
    {
        var output = Success_T(T.Value1).Bind(Success_T_Func_K);

        FuncParam.Should().Be(T.Value1);
        AssertSuccess(output);
    }

    [Fact]
    public void Bind_failed_result_does_not_execute_func()
    {
        var output = Failure_T().Bind(Success_T_Func_K);

        AssertFailure(output);
    }

    [Fact]
    public void Bind_tuple_success_result_executes_function()
    {
        var output = Result.Success((T.Value1, K.Value1)).Bind(Success_T_K_Func_K);

        FuncParam.Should().Be(T.Value1);
        AssertSuccess(output);
    }
}
