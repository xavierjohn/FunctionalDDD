namespace FunctionalDDD;

public static partial class AsyncResultExtensionsRightOperand
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Error>> EnsureAsync<TOk>(this Result<TOk, Error> result, Func<Task<bool>> predicate, Error errorMessage)
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
    public static async Task<Result<TOk, Error>> EnsureAsync<TOk>(this Result<TOk, Error> result, Func<TOk, Task<bool>> predicate, Error errorMessage)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<TOk, Error>(errorMessage);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Error>> EnsureAsync<TOk>(this Result<TOk, Error> result, Func<TOk, Task<bool>> predicate, Func<TOk, Error> errorPredicate)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<TOk, Error>(errorPredicate(result.Ok));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Error>> EnsureAsync<TOk>(this Result<TOk, Error> result, Func<TOk, Task<bool>> predicate, Func<TOk, Task<Error>> errorPredicate)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<TOk, Error>(await errorPredicate(result.Ok).ConfigureAwait(false));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk, Error>> EnsureAsync<TOk>(this Result<TOk, Error> result, Func<Task<Result<TOk, Error>>> predicate)
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
    public static async Task<Result<TOk, Error>> EnsureAsync<TOk>(this Result<TOk, Error> result, Func<TOk, Task<Result<TOk, Error>>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = await predicate(result.Ok).ConfigureAwait(false);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk, Error>(predicateResult.Error);

        return result;
    }
}
