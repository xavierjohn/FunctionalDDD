namespace FunctionalDDD.RailwayOrientedProgramming;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Passes the result to the given function (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static K Finally<T, K>(this Result<T> result, Func<Result<T>, K> func)
        => func(result);

    /// <summary>
    ///     Passes the result to the given function (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static async Task<K> FinallyAsync<T, K>(this Task<Result<T>> resultTask, Func<Result<T>, Task<K>> func)
    {
        Result<T> result = await resultTask.DefaultAwait();
        return await func(result).DefaultAwait();
    }

    /// <summary>
    ///     Passes the result to the given function (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static async Task<K> FinallyAsync<T, K>(this Task<Result<T>> resultTask, Func<Result<T>, K> func)
    {
        Result<T> result = await resultTask.DefaultAwait();
        return result.Finally(func);
    }

    /// <summary>
    ///     Passes the result to the given function (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static Task<K> FinallyAsync<T, K>(this Result<T> result, Func<Result<T>, Task<K>> func)
      => func(result);

    /// <summary>
    ///     Passes the result to the given valueTask action (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static async ValueTask<K> FinallyAsync<T, K>(this ValueTask<Result<T>> resultTask, Func<Result<T>, ValueTask<K>> valueTask)
    {
        Result<T> result = await resultTask;
        return await valueTask(result);
    }

    /// <summary>
    ///     Passes the result to the given valueTask action (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static async ValueTask<K> FinallyAsync<T, K>(this ValueTask<Result<T>> resultTask, Func<Result<T>, K> valueTask)
    {
        Result<T> result = await resultTask;
        return result.Finally(valueTask);
    }

    /// <summary>
    ///     Passes the result to the given valueTask action (regardless of success/failure state) to yield a final output value.
    /// </summary>
    public static async ValueTask<K> FinallyAsync<T, K>(this Result<T> result, Func<Result<T>, ValueTask<K>> valueTask)
    {
        return await valueTask(result);
    }
}
