namespace FunctionalDDD;

public static partial class AsyncResultExtensionsRightOperand
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<Task<bool>> predicate, Errs errorMessage)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate().ConfigureAwait(false))
            return Result.Failure<T>(errorMessage);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<bool>> predicate, Errs errorMessage)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<T>(errorMessage);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<bool>> predicate, Func<T, Errs> errorPredicate)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<T>(errorPredicate(result.Ok));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<bool>> predicate, Func<T, Task<Errs>> errorPredicate)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Ok).ConfigureAwait(false))
            return Result.Failure<T>(await errorPredicate(result.Ok).ConfigureAwait(false));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<Task<Result<T>>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = await predicate().ConfigureAwait(false);

        if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Err);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<Result<T>>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = await predicate(result.Ok).ConfigureAwait(false);

        if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Err);

        return result;
    }
}
