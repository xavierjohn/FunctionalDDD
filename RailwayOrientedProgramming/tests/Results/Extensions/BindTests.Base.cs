namespace RailwayOrientedProgramming.Tests.Results.Extensions;

using FunctionalDDD;
using RailwayOrientedProgramming.Tests.Results;

public abstract class BindTestsBase : TestBase
{
    private bool _funcExecuted;
    protected T? FuncParam { get; set; }

    protected BindTestsBase()
    {
        _funcExecuted = false;
        FuncParam = null;
    }

    protected bool FuncExecuted => _funcExecuted;

    protected Result<T, Err> Success_T(T value)
    {
        _funcExecuted = true;
        FuncParam = value;
        return Result.Success(value);
    }

    protected Result<T, Err> Failure_T()
    {
        _funcExecuted = false;
        return Result.Failure<T>(Error1);
    }
    protected Result<T, Err> Failure_T_E()
    {
        _funcExecuted = false;
        return Result.Failure<T>(Error1);
    }

    protected Result<K, Err> Success_K()
    {
        _funcExecuted = true;
        return Result.Success(K.Value);
    }
    protected Result<K, Err> Failure_K()
    {
        _funcExecuted = false;
        return Result.Failure<K>(Error1);
    }

    protected Result<K, Err> Success_T_Func_K(T value)
    {
        _funcExecuted = true;
        FuncParam = value;
        return Result.Success(K.Value);
    }

    protected Result<K, Err> Success_T_E_Func_K(T value)
    {
        _funcExecuted = true;
        FuncParam = value;
        return Result.Success(K.Value);
    }

    protected Result<K, Err> Failure_T_E_Func_K(T value)
    {
        _funcExecuted = false;
        FuncParam = value;
        return Result.Failure<K>(Error1);
    }

    protected Result<T, Err> Success_T_E()
    {
        _funcExecuted = true;
        return Result.Success(T.Value);
    }

    protected Task<Result<T, Err>> Task_Success_T(T value)
    {
        return Success_T(value).AsTask();
    }

    protected Task<Result<T, Err>> Task_Failure_T()
    {
        return Failure_T().AsTask();
    }
    protected Task<Result<T, Err>> Task_Failure_T_E()
    {
        return Failure_T_E().AsTask();
    }

    protected Task<Result<K, Err>> Task_Success_K()
    {
        return Success_K().AsTask();
    }

    protected Task<Result<K, Err>> Task_Failure_K()
    {
        return Failure_K().AsTask();
    }

    protected Task<Result<K, Err>> Func_T_Task_Success_K(T value)
    {
        return Success_T_Func_K(value).AsTask();
    }

    protected ValueTask<Result<T, Err>> ValueTask_Success_T(T value)
    {
        return Success_T(value).AsValueTask();
    }

    protected ValueTask<Result<T, Err>> ValueTask_Failure_T()
    {
        return Failure_T().AsValueTask();
    }

    protected ValueTask<Result<K, Err>> ValueTask_Success_K()
    {
        return Success_K().AsValueTask();
    }
    protected ValueTask<Result<K, Err>> ValueTask_Failure_K()
    {
        return Failure_K().AsValueTask();
    }

    protected ValueTask<Result<K, Err>> Func_T_ValueTask_Success_K(T value)
    {
        return Success_T_Func_K(value).AsValueTask();
    }

    protected void AssertFailure(Result<K, Err> output)
    {
        _funcExecuted.Should().BeFalse();
        output.IsFailure.Should().BeTrue();
        output.Err.Should().Be(Error1);
    }

    protected void AssertSuccess(Result<K, Err> output)
    {
        _funcExecuted.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
        output.Ok.Should().Be(K.Value);
    }

    protected Task<Result<T, Err>> Task_Success_T_E()
    {
        return Success_T_E().AsTask();
    }

    protected ValueTask<Result<T, Err>> Func_ValueTask_Success_T_E()
    {
        return Success_T_E().AsValueTask();
    }

    protected ValueTask<Result<T, Err>> ValueTask_Failure_T_E()
    {
        return Failure_T_E().AsValueTask();
    }
}
