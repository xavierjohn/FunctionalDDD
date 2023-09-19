namespace FunctionalDDD.Results;

using FunctionalDDD.Results.Errors;

public static partial class EnsureExtensionsAsync
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<TOk, ValueTask<bool>> predicate, Error errors)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).ConfigureAwait(false))
            return Result.Failure<TOk>(errors);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<TOk, ValueTask<bool>> predicate, Func<TOk, Error> errorPredicate)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).ConfigureAwait(false))
            return Result.Failure<TOk>(errorPredicate(result.Value));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<TOk, ValueTask<bool>> predicate, Func<TOk, ValueTask<Error>> errorPredicate)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).ConfigureAwait(false))
            return Result.Failure<TOk>(await errorPredicate(result.Value).ConfigureAwait(false));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<ValueTask<Result<TOk>>> predicate)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        var predicateResult = await predicate();

        if (predicateResult.IsFailure)
            return Result.Failure<TOk>(predicateResult.Error);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<TOk, ValueTask<Result<TOk>>> predicate)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        var predicateResult = await predicate(result.Value);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk>(predicateResult.Error);

        return result;
    }
}

