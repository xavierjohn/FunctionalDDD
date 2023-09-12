namespace FunctionalDDD.RailwayOrientedProgramming;

using FunctionalDDD.RailwayOrientedProgramming.Errors;

public static partial class FinallyExtensions
{
    /// <summary>
    ///     Passes the result to the given function (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static TOut Finally<TIn, TOut>(this Result<TIn> result,
        Func<Result<TIn>, TOut> func)
        => func(result);

    /// <summary>
    ///     Passes the result to the given function (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static async Task<TOut> FinallyAsync<TIn, TOut>(this Task<Result<TIn>> resultTask,
        Func<Result<TIn>, Task<TOut>> func)
    {
        Result<TIn> result = await resultTask.ConfigureAwait(false);
        return await func(result).ConfigureAwait(false);
    }

    /// <summary>
    ///     Passes the result to the given function (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static async Task<TOut> FinallyAsync<TIn, TOut>(this Task<Result<TIn>> resultTask,
        Func<Result<TIn>, TOut> func)
    {
        Result<TIn> result = await resultTask.ConfigureAwait(false);
        return result.Finally(func);
    }

    /// <summary>
    ///     Passes the result to the given function (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static Task<TOut> FinallyAsync<TIn, TOut>(this Result<TIn> result,
        Func<Result<TIn>, Task<TOut>> func)
      => func(result);

    /// <summary>
    ///     Passes the result to the given valueTask action (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static async ValueTask<TOut> FinallyAsync<TIn, TOut>(this ValueTask<Result<TIn>> resultTask,
        Func<Result<TIn>, ValueTask<TOut>> valueTask)
    {
        Result<TIn> result = await resultTask;
        return await valueTask(result);
    }

    /// <summary>
    ///     Passes the result to the given valueTask action (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static async ValueTask<TOut> FinallyAsync<TIn, TOut>(this ValueTask<Result<TIn>> resultTask,
        Func<Result<TIn>, TOut> valueTask)
    {
        Result<TIn> result = await resultTask;
        return result.Finally(valueTask);
    }

    /// <summary>
    ///     Passes the result to the given valueTask action (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static async ValueTask<TOut> FinallyAsync<TIn, TOut>(this Result<TIn> result,
        Func<Result<TIn>, ValueTask<TOut>> valueTask)
    {
        return await valueTask(result);
    }

    public static TOut Finally<TIn, TOut>(this Result<TIn> result,
        Func<TIn, TOut> funcOk,
        Func<Error, TOut> funcError) =>
        result.IsSuccess ? funcOk(result.Value) : funcError(result.Error);

    public static async Task<TOut> FinallyAsync<TIn, TOut>(this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> funcOk,
        Func<Error, TOut> funcError)
    {
        Result<TIn> result = await resultTask.ConfigureAwait(false);
        return result.Finally(funcOk, funcError);
    }

    public static async ValueTask<TOut> FinallyAsync<TIn, TOut>(this ValueTask<Result<TIn>> resultTask,
    Func<TIn, TOut> funcOk,
    Func<Error, TOut> funcError)
    {
        Result<TIn> result = await resultTask.ConfigureAwait(false);
        return result.Finally(funcOk, funcError);
    }
}
