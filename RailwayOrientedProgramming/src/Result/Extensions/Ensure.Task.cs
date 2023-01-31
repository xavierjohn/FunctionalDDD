namespace FunctionalDDD;


public static partial class AsyncResultExtensionsBothOperands
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Task<Result<TOk, Err>> resultTask, Func<TOk, Task<bool>> predicate, Errs<Err> errors)
    {
        Result<TOk, Err> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<TOk, Err>(errors);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Task<Result<TOk, Err>> resultTask, Func<TOk, Task<bool>> predicate, Func<TOk, Errs<Err>> errorPredicate)
    {
        Result<TOk, Err> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<TOk, Err>(errorPredicate(result.Ok));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Task<Result<TOk, Err>> resultTask, Func<TOk, Task<bool>> predicate, Func<TOk, Task<Errs<Err>>> errorPredicate)
    {
        Result<TOk, Err> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<TOk, Err>(await errorPredicate(result.Ok).ConfigureAwait(false));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Task<Result<TOk, Err>> resultTask, Func<Task<Result<TOk, Err>>> predicate)
    {
        Result<TOk, Err> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        var predicateResult = await predicate();

        if (predicateResult.IsFailure)
            return Result.Failure<TOk, Err>(predicateResult.Error);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Task<Result<TOk, Err>> resultTask, Func<TOk, Task<Result<TOk, Err>>> predicate)
    {
        Result<TOk, Err> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        var predicateResult = await predicate(result.Ok);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk, Err>(predicateResult.Error);

        return result;
    }
}

