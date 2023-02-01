namespace FunctionalDDD;

public static partial class AsyncResultExtensionsLeftOperand
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Task<Result<TOk, Err>> resultTask, Func<bool> predicate, Err errorMessage)
    {
        Result<TOk, Err> result = await resultTask.ConfigureAwait(false);
        return result.Ensure(predicate, errorMessage);
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Task<Result<TOk, Err>> resultTask, Func<TOk, bool> predicate, Err errorMessage)
    {
        Result<TOk, Err> result = await resultTask.ConfigureAwait(false);
        return result.Ensure(predicate, errorMessage);
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Task<Result<TOk, Err>> resultTask, Func<TOk, bool> predicate, Func<TOk, Err> errorPredicate)
    {
        Result<TOk, Err> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        return result.Ensure(predicate, errorPredicate);
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Task<Result<TOk, Err>> resultTask, Func<TOk, bool> predicate, Func<TOk, Task<Err>> errorPredicate)
    {
        Result<TOk, Err> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (predicate(result.Ok))
            return result;

        return Result.Failure<TOk, Err>(await errorPredicate(result.Ok).ConfigureAwait(false));
    }


    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Task<Result<TOk, Err>> resultTask, Func<Result<TOk, Err>> predicate)
    {
        Result<TOk, Err> result = await resultTask.ConfigureAwait(false);
        return result.Ensure(predicate);
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Task<Result<TOk, Err>> resultTask, Func<TOk, Result<TOk, Err>> predicate)
    {
        Result<TOk, Err> result = await resultTask.ConfigureAwait(false);
        return result.Ensure(predicate);
    }

}
