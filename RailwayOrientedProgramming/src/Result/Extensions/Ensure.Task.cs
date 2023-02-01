namespace FunctionalDDD;


public static partial class AsyncResultExtensionsBothOperands
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Error>> EnsureAsync<TOk>(this Task<Result<TOk, Error>> resultTask, Func<TOk, Task<bool>> predicate, Error errors)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<TOk, Error>(errors);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Error>> EnsureAsync<TOk>(this Task<Result<TOk, Error>> resultTask, Func<TOk, Task<bool>> predicate, Func<TOk, Error> errorPredicate)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<TOk, Error>(errorPredicate(result.Ok));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Error>> EnsureAsync<TOk>(this Task<Result<TOk, Error>> resultTask, Func<TOk, Task<bool>> predicate, Func<TOk, Task<Error>> errorPredicate)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<TOk, Error>(await errorPredicate(result.Ok).ConfigureAwait(false));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Error>> EnsureAsync<TOk>(this Task<Result<TOk, Error>> resultTask, Func<Task<Result<TOk, Error>>> predicate)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        var predicateResult = await predicate();

        if (predicateResult.IsFailure)
            return Result.Failure<TOk, Error>(predicateResult.Error);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Error>> EnsureAsync<TOk>(this Task<Result<TOk, Error>> resultTask, Func<TOk, Task<Result<TOk, Error>>> predicate)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        var predicateResult = await predicate(result.Ok);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk, Error>(predicateResult.Error);

        return result;
    }
}

