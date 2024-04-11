namespace FunctionalDdd;

using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

/// <summary>
/// If the starting Result is a success, the Bind function will return a new Result from the given function.
/// Otherwise, the Bind function will return the starting failed Result.
/// </summary>
public static partial class BindExtensions
{
    public static Result<TResult> Bind<TValue, TResult>(this Result<TValue> result, Func<TValue, Result<TResult>> function)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error);

        using var activity  = Trace.ActivitySource.StartActivity();
        activity?.AddTag("Function", function.Method.Name);
        var retResult = function(result.Value);

        activity?.SetStatus(retResult.IsSuccess ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
        return retResult;
    }
}

/// <summary>
/// If the starting Result is a success, the Bind function will return a new Result from the given function.
/// Otherwise, the Bind function will return the starting failed Result.
/// </summary>
public static partial class BindExtensionsAsync
{
    public static async Task<Result<TResult>> BindAsync<TValue, TResult>(this Result<TValue> result, Func<TValue, Task<Result<TResult>>> function)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error);

        using var activity = Trace.ActivitySource.StartActivity();
        activity?.AddTag("Function", function.Method.Name);
        var retResult = await function(result.Value);

        activity?.SetStatus(retResult.IsSuccess ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
        return retResult;
    }

    public static async Task<Result<TResult>> BindAsync<TValue, TResult>(this Task<Result<TValue>> resultTask, Func<TValue, Task<Result<TResult>>> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.BindAsync(func).ConfigureAwait(false);
    }

    public static async Task<Result<TResult>> BindAsync<TValue, TResult>(this Task<Result<TValue>> resultTask, Func<TValue, Result<TResult>> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.Bind(func);
    }

    public static async ValueTask<Result<TResult>> BindAsync<TValue, TResult>(this ValueTask<Result<TValue>> resultTask, Func<TValue, ValueTask<Result<TResult>>> valueTask)
    {
        Result<TValue> result = await resultTask;
        return await result.BindAsync(valueTask);
    }

    public static async ValueTask<Result<TResult>> BindAsync<TValue, TResult>(this ValueTask<Result<TValue>> resultTask, Func<TValue, Result<TResult>> func)
    {
        Result<TValue> result = await resultTask;
        return result.Bind(func);
    }

    public static ValueTask<Result<TResult>> BindAsync<TValue, TResult>(this Result<TValue> result, Func<TValue, ValueTask<Result<TResult>>> valueTask)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error).AsCompletedValueTask();

        return valueTask(result.Value);
    }
}
