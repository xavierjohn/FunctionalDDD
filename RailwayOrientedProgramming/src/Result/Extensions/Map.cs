namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TOut, Error> Map<TIn, TOut>(this Result<TIn, Error> result, Func<TIn, TOut> func)
    {
        if (result.IsFailure)
            return Result.Failure<TOut, Error>(result.Error);

        return Result.Success<TOut, Error>(func(result.Value));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<TOut, Error>> MapAsync<TIn, TOut>(this Task<Result<TIn, Error>> resultTask, Func<TIn, Task<TOut>> func)
    {
        Result<TIn, Error> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return Result.Failure<TOut, Error>(result.Error);

        TOut value = await func(result.Value).ConfigureAwait(false);

        return Result.Success<TOut, Error>(value);
    }

    public static async Task<Result<TOut, Error>> MapAsync<TIn, TOut>(this Task<Result<TIn, Error>> resultTask, Func<TIn, TOut> func)
    {
        Result<TIn, Error> result = await resultTask;

        if (result.IsFailure)
            return Result.Failure<TOut, Error>(result.Error);

        TOut value = func(result.Value);

        return Result.Success<TOut, Error>(value);
    }

    /// <summary>
    ///     Creates a new result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<TOut, Error>> MapAsync<TIn, TOut>(this ValueTask<Result<TIn, Error>> resultTask, Func<TIn, ValueTask<TOut>> valueTask)
    {
        Result<TIn, Error> result = await resultTask;

        if (result.IsFailure)
            return Result.Failure<TOut, Error>(result.Error);

        TOut value = await valueTask(result.Value);

        return Result.Success<TOut, Error>(value);
    }

    public static async ValueTask<Result<TOut, Error>> MapAsync<TIn, TOut>(this ValueTask<Result<TIn, Error>> resultTask, Func<TIn, TOut> func)
    {
        Result<TIn, Error> result = await resultTask;

        if (result.IsFailure)
            return Result.Failure<TOut, Error>(result.Error);

        TOut value = func(result.Value);

        return Result.Success<TOut, Error>(value);
    }
}
