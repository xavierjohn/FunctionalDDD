namespace RailwayOrientedProgramming.Tests.Results.Extensions;

public class TeeTests : TestBase
{
    protected bool ActionExecuted { get; set; }

    protected TeeTests()
    {
        ActionExecuted = false;
    }

    protected void Action()
    {
        ActionExecuted = true;
    }

    protected void Action_T(T _)
    {
        ActionExecuted = true;
    }

    protected Task Task_Action()
    {
        ActionExecuted = true;
        return Task.CompletedTask;
    }

    protected Task Task_Action_T(T _)
    {
        ActionExecuted = true;
        return Task.CompletedTask;
    }

    protected ValueTask ValueTask_Action()
    {
        ActionExecuted = true;
        return ValueTask.CompletedTask;
    }

    protected ValueTask ValueTask_Action_T(T _)
    {
        ActionExecuted = true;
        return ValueTask.CompletedTask;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Tee_T_executes_action_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T, Error> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = result.Tee(Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Tee_T_executes_action_T_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T, Error> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = result.Tee(Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    #region Task
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tee_Task_T_E_executes_action_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T, Error> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        Result<T, Error> returned = await result.AsTask().TeeAsync(Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tee_Task_T_E_executes_action_T_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T, Error> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.AsTask().TeeAsync(Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tee_Task_Left_T_E_executes_action_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T, Error> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.AsTask().TeeAsync(Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_Task_Left_T_E_executes_action_T_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T, Error> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.AsTask().TeeAsync(Action_T);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_Task_Left_T_E_executes_task_action_T_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T, Error> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.AsTask().TeeAsync(Task_Action_T);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_Task_Right_T_E_executes_action_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T, Error> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.TeeAsync(Task_Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_Task_Right_T_E_executes_action_T_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T, Error> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.TeeAsync(Task_Action_T);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }
    #endregion

    #region ValueTask
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_ValueTask_T_E_executes_action_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T, Error> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.AsValueTask().TeeAsync(ValueTask_Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_ValueTask_T_E_executes_action_T_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T, Error> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.AsValueTask().TeeAsync(ValueTask_Action_T);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_ValueTask_Left_T_E_executes_action_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T, Error> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.AsValueTask().TeeAsync(Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_ValueTask_Left_T_E_executes_action_T_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T, Error> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.AsValueTask().TeeAsync(Action_T);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_ValueTask_Right_T_E_executes_action_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T, Error> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.TeeAsync(ValueTask_Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_ValueTask_Right_T_E_executes_action_T_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T, Error> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.TeeAsync(ValueTask_Action_T);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }
    #endregion
}
