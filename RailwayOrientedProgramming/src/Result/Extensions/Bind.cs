namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult, Err> Bind<TOk, TResult>(this Result<TOk, Err> result, Func<TOk, Result<TResult, Err>> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Errs);

        return func(result.Ok);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Task<Result<TResult, Err>> BindAsync<TOk, TResult>(this Result<TOk, Err> result, Func<TOk, Task<Result<TResult, Err>>> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Err).AsCompletedTask();

        return func(result.Ok);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<TResult, Err>> BindAsync<TOk, TResult>(this Task<Result<TOk, Err>> resultTask, Func<TOk, Task<Result<TResult, Err>>> func)
    {
        Result<TOk, Err> result = await resultTask.ConfigureAwait(false);
        return await result.BindAsync(func).ConfigureAwait(false);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<TResult, Err>> BindAsync<TOk, TResult>(this Task<Result<TOk, Err>> resultTask, Func<TOk, Result<TResult, Err>> func)
    {
        Result<TOk, Err> result = await resultTask.ConfigureAwait(false);
        return result.Bind(func);
    }

    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<TResult, Err>> BindAsync<TOk, TResult>(this ValueTask<Result<TOk, Err>> resultTask, Func<TOk, ValueTask<Result<TResult, Err>>> valueTask)
    {
        Result<TOk, Err> result = await resultTask;
        return await result.BindAsync(valueTask);
    }

    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<TResult, Err>> BindAsync<TOk, TResult>(this ValueTask<Result<TOk, Err>> resultTask, Func<TOk, Result<TResult, Err>> func)
    {
        Result<TOk, Err> result = await resultTask;
        return result.Bind(func);
    }

    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static ValueTask<Result<TResult, Err>> BindAsync<TOk, TResult>(this Result<TOk, Err> result, Func<TOk, ValueTask<Result<TResult, Err>>> valueTask)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Err).AsCompletedValueTask();

        return valueTask(result.Ok);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<TResult, Err>> BindAsync<T1, T2, TResult>(this Task<Result<(T1, T2), Err>> resultTask, Func<T1, T2, Result<TResult, Err>> func)
    {
        var result = await resultTask;
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Errs);

        var (args1, args2) = result.Ok;
        return func(args1, args2);
    }
}
