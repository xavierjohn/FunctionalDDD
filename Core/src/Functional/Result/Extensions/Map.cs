using System;

namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<K> Map<T, K>(this Result<T> result, Func<T, K> func)
    {
        if (result.IsFailure)
            return Result.Failure<K>(result.Error);

        return Result.Success<K>(func(result.Value));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<K>> Map<T, K, E>(this Task<Result<T>> resultTask, Func<T, Task<K>> func)
    {
        Result<T> result = await resultTask.DefaultAwait();

        if (result.IsFailure)
            return Result.Failure<K>(result.Error);

        K value = await func(result.Value).DefaultAwait();

        return Result.Success<K>(value);
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<K>> Map<T, K>(this Task<Result<T>> resultTask, Func<T, Task<K>> func)
    {
        Result<T> result = await resultTask.DefaultAwait();

        if (result.IsFailure)
            return Result.Failure<K>(result.Error);

        K value = await func(result.Value).DefaultAwait();

        return Result.Success(value);
    }

    /// <summary>
    ///     Creates a new result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<K>> Map<T, K>(this ValueTask<Result<T>> resultTask, Func<T, ValueTask<K>> valueTask)
    {
        Result<T> result = await resultTask;

        if (result.IsFailure)
            return Result.Failure<K>(result.Error);

        K value = await valueTask(result.Value);

        return Result.Success(value);
    }

}
