namespace FunctionalDDD;

public static partial class AsyncResultExtensionsRightOperand
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Result<TOk, Err> result, Func<Task<bool>> predicate, Err errorMessage)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate().ConfigureAwait(false))
            return Result.Failure<TOk, Err>(errorMessage);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Result<TOk, Err> result, Func<TOk, Task<bool>> predicate, Err errorMessage)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<TOk, Err>(errorMessage);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Result<TOk, Err> result, Func<TOk, Task<bool>> predicate, Func<TOk, Err> errorPredicate)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<TOk, Err>(errorPredicate(result.Ok));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Result<TOk, Err> result, Func<TOk, Task<bool>> predicate, Func<TOk, Task<Err>> errorPredicate)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<TOk, Err>(await errorPredicate(result.Ok).ConfigureAwait(false));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Result<TOk, Err> result, Func<Task<Result<TOk, Err>>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = await predicate().ConfigureAwait(false);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk, Err>(predicateResult.Error);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Err>> EnsureAsync<TOk>(this Result<TOk, Err> result, Func<TOk, Task<Result<TOk, Err>>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = await predicate(result.Ok).ConfigureAwait(false);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk, Err>(predicateResult.Error);

        return result;
    }
}
