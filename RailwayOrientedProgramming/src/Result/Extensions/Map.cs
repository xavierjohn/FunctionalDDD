namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TOut, Err> Map<TIn, TOut>(this Result<TIn, Err> result, Func<TIn, TOut> func)
    {
        if (result.IsFailure)
            return Result.Failure<TOut, Err>(result.Err);

        return Result.Success<TOut, Err>(func(result.Ok));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<TOut, Err>> MapAsync<TIn, TOut>(this Task<Result<TIn, Err>> resultTask, Func<TIn, Task<TOut>> func)
    {
        Result<TIn, Err> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return Result.Failure<TOut, Err>(result.Err);

        TOut value = await func(result.Ok).ConfigureAwait(false);

        return Result.Success<TOut, Err>(value);
    }

    public static async Task<Result<TOut, Err>> MapAsync<TIn, TOut>(this Task<Result<TIn, Err>> resultTask, Func<TIn, TOut> func)
    {
        Result<TIn, Err> result = await resultTask;

        if (result.IsFailure)
            return Result.Failure<TOut, Err>(result.Err);

        TOut value = func(result.Ok);

        return Result.Success<TOut, Err>(value);
    }

    /// <summary>
    ///     Creates a new result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<TOut, Err>> MapAsync<TIn, TOut>(this ValueTask<Result<TIn, Err>> resultTask, Func<TIn, ValueTask<TOut>> valueTask)
    {
        Result<TIn, Err> result = await resultTask;

        if (result.IsFailure)
            return Result.Failure<TOut, Err>(result.Err);

        TOut value = await valueTask(result.Ok);

        return Result.Success<TOut, Err>(value);
    }

    public static async ValueTask<Result<TOut, Err>> MapAsync<TIn, TOut>(this ValueTask<Result<TIn, Err>> resultTask, Func<TIn, TOut> func)
    {
        Result<TIn, Err> result = await resultTask;

        if (result.IsFailure)
            return Result.Failure<TOut, Err>(result.Err);

        TOut value = func(result.Ok);

        return Result.Success<TOut, Err>(value);
    }
}
