namespace FunctionalDDD.Core;

public static partial class AsyncResultExtensionsBothOperands
{
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
}
