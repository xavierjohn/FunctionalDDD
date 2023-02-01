namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Passes the result to the given function (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static TOut Finally<TIn, TOut>(this Result<TIn, Error> result, Func<Result<TIn, Error>, TOut> func)
        => func(result);

    /// <summary>
    ///     Passes the result to the given function (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static async Task<TOut> FinallyAsync<TIn, TOut>(this Task<Result<TIn, Error>> resultTask, Func<Result<TIn, Error>, Task<TOut>> func)
    {
        Result<TIn, Error> result = await resultTask.ConfigureAwait(false);
        return await func(result).ConfigureAwait(false);
    }

    /// <summary>
    ///     Passes the result to the given function (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static async Task<TOut> FinallyAsync<TIn, TOut>(this Task<Result<TIn, Error>> resultTask, Func<Result<TIn, Error>, TOut> func)
    {
        Result<TIn, Error> result = await resultTask.ConfigureAwait(false);
        return result.Finally(func);
    }

    /// <summary>
    ///     Passes the result to the given function (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static Task<TOut> FinallyAsync<TIn, TOut>(this Result<TIn, Error> result, Func<Result<TIn, Error>, Task<TOut>> func)
      => func(result);

    /// <summary>
    ///     Passes the result to the given valueTask action (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static async ValueTask<TOut> FinallyAsync<TIn, TOut>(this ValueTask<Result<TIn, Error>> resultTask, Func<Result<TIn, Error>, ValueTask<TOut>> valueTask)
    {
        Result<TIn, Error> result = await resultTask;
        return await valueTask(result);
    }

    /// <summary>
    ///     Passes the result to the given valueTask action (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static async ValueTask<TOut> FinallyAsync<TIn, TOut>(this ValueTask<Result<TIn, Error>> resultTask, Func<Result<TIn, Error>, TOut> valueTask)
    {
        Result<TIn, Error> result = await resultTask;
        return result.Finally(valueTask);
    }

    /// <summary>
    ///     Passes the result to the given valueTask action (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static async ValueTask<TOut> FinallyAsync<TIn, TOut>(this Result<TIn, Error> result, Func<Result<TIn, Error>, ValueTask<TOut>> valueTask)
    {
        return await valueTask(result);
    }
}
