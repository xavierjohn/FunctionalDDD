namespace RailwayOrientedProgramming.Tests.Results.Extensions;

public class TapTests : TestBase
{
    protected bool ActionExecuted { get; set; }

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
    public void Tap_T_executes_action_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = result.Tap(Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Tap_T_executes_action_T_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = result.Tap(Action_T);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Tap_T1_T2_executes_action_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<(T, K)> result = Result.SuccessIf(isSuccess, T.Value, K.Value, Error1);

        var returned = result.Tap(Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    #region Task
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_Task_T_E_executes_action_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        Result<T> returned = await result.AsTask().TapAsync(Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_Task_T_E_executes_action_T_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.AsTask().TapAsync(Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_Task_Left_T_E_executes_action_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.AsTask().TapAsync(Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_Task_Left_T_E_executes_action_T_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.AsTask().TapAsync(Action_T);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_Task_Left_T_E_executes_task_action_T_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.AsTask().TapAsync(Task_Action_T);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_Task_Right_T_E_executes_action_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.TapAsync(Task_Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_Task_Right_T_E_executes_action_T_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.TapAsync(Task_Action_T);

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
        Result<T> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.AsValueTask().TapAsync(ValueTask_Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_ValueTask_T_E_executes_action_T_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.AsValueTask().TapAsync(ValueTask_Action_T);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_ValueTask_Left_T_E_executes_action_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.AsValueTask().TapAsync(Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_ValueTask_Left_T_E_executes_action_T_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.AsValueTask().TapAsync(Action_T);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_ValueTask_Right_T_E_executes_action_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.TapAsync(ValueTask_Action);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tap_ValueTask_Right_T_E_executes_action_T_on_result_success_and_returns_self(bool isSuccess)
    {
        Result<T> result = Result.SuccessIf(isSuccess, T.Value, Error1);

        var returned = await result.TapAsync(ValueTask_Action_T);

        ActionExecuted.Should().Be(isSuccess);
        result.Should().Be(returned);
    }
    #endregion
}
