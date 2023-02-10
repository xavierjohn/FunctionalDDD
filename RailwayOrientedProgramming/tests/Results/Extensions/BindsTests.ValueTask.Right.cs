namespace RailwayOrientedProgramming.Tests.Functional.Results.Extensions
{
    using FunctionalDDD;
    using RailwayOrientedProgramming.Tests.Results.Extensions;

    public class BindTests_ValueTask_Right : BindBase
    {
        [Fact]
        public async ValueTask Bind_ValueTask_Right_T_K_returns_failure_and_does_not_execute_func()
        {
            Result<K, Error> output = await Failure_T().BindAsync(Func_T_ValueTask_Success_K);

            AssertFailure(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_Right_T_K_selects_new_result()
        {
            Result<K, Error> output = await Success_T(T.Value).BindAsync(Func_T_ValueTask_Success_K);

            FuncParam.Should().Be(T.Value);
            AssertSuccess(output);
        }
    }
}
