namespace FunctionalDDD;


public static partial class AsyncResultExtensionsBothOperands
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, Errs errors)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<T>(errors);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, Func<T, Errs> errorPredicate)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<T>(errorPredicate(result.Ok));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, Func<T, Task<Errs>> errorPredicate)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<T>(await errorPredicate(result.Ok).ConfigureAwait(false));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<Task<Result<T>>> predicate)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        var predicateResult = await predicate();

        if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Err);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<Result<T>>> predicate)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        var predicateResult = await predicate(result.Ok);

        if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Err);

        return result;
    }
}

