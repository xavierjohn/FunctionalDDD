namespace FunctionalDDD;

public static partial class AsyncResultExtensionsRightOperand
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk, Error>> EnsureAsync<TOk>(this Result<TOk, Error> result, Func<ValueTask<bool>> predicate, Error errorMessage)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate().ConfigureAwait(false))
            return Result.Failure<TOk, Error>(errorMessage);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk, Error>> EnsureAsync<TOk>(this Result<TOk, Error> result, Func<TOk, ValueTask<bool>> predicate, Error errorMessage)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).ConfigureAwait(false))
            return Result.Failure<TOk, Error>(errorMessage);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk, Error>> EnsureAsync<TOk>(this Result<TOk, Error> result, Func<TOk, ValueTask<bool>> predicate, Func<TOk, Error> errorPredicate)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).ConfigureAwait(false))
            return Result.Failure<TOk, Error>(errorPredicate(result.Value));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk, Error>> EnsureAsync<TOk>(this Result<TOk, Error> result, Func<TOk, ValueTask<bool>> predicate, Func<TOk, ValueTask<Error>> errorPredicate)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).ConfigureAwait(false))
            return Result.Failure<TOk, Error>(await errorPredicate(result.Value).ConfigureAwait(false));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk, Error>> EnsureAsync<TOk>(this Result<TOk, Error> result, Func<ValueTask<Result<TOk, Error>>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = await predicate().ConfigureAwait(false);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk, Error>(predicateResult.Error);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk, Error>> EnsureAsync<TOk>(this Result<TOk, Error> result, Func<TOk, ValueTask<Result<TOk, Error>>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = await predicate(result.Value).ConfigureAwait(false);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk, Error>(predicateResult.Error);

        return result;
    }
}
