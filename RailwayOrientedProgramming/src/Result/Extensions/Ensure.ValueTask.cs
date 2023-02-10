namespace FunctionalDDD;


public static partial class AsyncResultExtensionsBothOperands
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk, Error>> EnsureAsync<TOk>(this ValueTask<Result<TOk, Error>> resultTask, Func<TOk, ValueTask<bool>> predicate, Error errors)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).ConfigureAwait(false))
            return Result.Failure<TOk, Error>(errors);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk, Error>> EnsureAsync<TOk>(this ValueTask<Result<TOk, Error>> resultTask, Func<TOk, ValueTask<bool>> predicate, Func<TOk, Error> errorPredicate)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).ConfigureAwait(false))
            return Result.Failure<TOk, Error>(errorPredicate(result.Value));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk, Error>> EnsureAsync<TOk>(this ValueTask<Result<TOk, Error>> resultTask, Func<TOk, ValueTask<bool>> predicate, Func<TOk, ValueTask<Error>> errorPredicate)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).ConfigureAwait(false))
            return Result.Failure<TOk, Error>(await errorPredicate(result.Value).ConfigureAwait(false));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk, Error>> EnsureAsync<TOk>(this ValueTask<Result<TOk, Error>> resultTask, Func<ValueTask<Result<TOk, Error>>> predicate)
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
    public static async ValueTask<Result<TOk, Error>> EnsureAsync<TOk>(this ValueTask<Result<TOk, Error>> resultTask, Func<TOk, ValueTask<Result<TOk, Error>>> predicate)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        var predicateResult = await predicate(result.Value);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk, Error>(predicateResult.Error);

        return result;
    }
}

