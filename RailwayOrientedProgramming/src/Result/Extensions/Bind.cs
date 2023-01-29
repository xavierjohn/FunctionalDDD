namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult> Bind<T, TResult>(this Result<T> result, Func<T, Result<TResult>> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Errs);

        return func(result.Ok);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Task<Result<TResult>> BindAsync<T, TResult>(this Result<T> result, Func<T, Task<Result<TResult>>> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Err).AsCompletedTask();

        return func(result.Ok);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<TResult>> BindAsync<T, TResult>(this Task<Result<T>> resultTask, Func<T, Task<Result<TResult>>> func)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.BindAsync(func).ConfigureAwait(false);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<TResult>> BindAsync<T, TResult>(this Task<Result<T>> resultTask, Func<T, Result<TResult>> func)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return result.Bind(func);
    }

    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<TResult>> BindAsync<T, TResult>(this ValueTask<Result<T>> resultTask, Func<T, ValueTask<Result<TResult>>> valueTask)
    {
        Result<T> result = await resultTask;
        return await result.BindAsync(valueTask);
    }

    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<TResult>> BindAsync<T, TResult>(this ValueTask<Result<T>> resultTask, Func<T, Result<TResult>> func)
    {
        Result<T> result = await resultTask;
        return result.Bind(func);
    }

    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static ValueTask<Result<TResult>> BindAsync<T, TResult>(this Result<T> result, Func<T, ValueTask<Result<TResult>>> valueTask)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Err).AsCompletedValueTask();

        return valueTask(result.Ok);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<TResult>> BindAsync<T1, T2, TResult>(this Task<Result<(T1, T2)>> resultTask, Func<T1, T2, Result<TResult>> func)
    {
        var result = await resultTask;
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Errs);

        var (args1, args2) = result.Ok;
        return func(args1, args2);
    }
}
