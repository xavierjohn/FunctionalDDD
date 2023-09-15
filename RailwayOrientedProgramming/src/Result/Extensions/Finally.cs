namespace FunctionalDDD.RailwayOrientedProgramming;

using FunctionalDDD.RailwayOrientedProgramming.Errors;

/// <summary>
/// Finally extensions are used to terminate the chain and return the final value.
/// </summary>
public static class FinallyExtensions
{
    /// <summary>
    /// Passes the result to the given function (regardless of success/failure state) to yield a final output value.
    /// </summary>
    /// <typeparam name="TIn">Type of the data contained within Result object.</typeparam>
    /// <typeparam name="TOut">Return type</typeparam>
    /// <param name="result">The <see cref="Result{TValue}"/>.</param>
    /// <param name="func">A delegate that processes the <see cref="Result{TValue}"/> and provides the final value.</param>
    /// <returns></returns>

    public static TOut Finally<TIn, TOut>(this Result<TIn> result, Func<Result<TIn>, TOut> func)
        => func(result);

    /// <summary>
    /// Passes the result to the given function (regardless of success/failure state) to yield a final output value.
    /// </summary>
    /// <typeparam name="TIn">Type of the data contained within Result object.</typeparam>
    /// <typeparam name="TOut">Return type</typeparam>
    /// <param name="result">The <see cref="Result{TValue}"/>.</param>
    /// <param name="funcOk">A delegate the success value.</param>
    /// <param name="funcError">A delegate that returns the error.</param>
    /// <returns>The final result of type TOut</returns>
    public static TOut Finally<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> funcOk, Func<Error, TOut> funcError)
        => result.IsSuccess ? funcOk(result.Value) : funcError(result.Error);
}

/// <summary>
/// Finally extensions are used to terminate the chain and return the final value.
/// </summary>
public static class FinallyExtensionsAsync
{
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
