namespace FunctionalDDD.RailwayOrientedProgramming;

using FunctionalDDD.RailwayOrientedProgramming.Errors;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk>> EnsureAsync<TOk>(this Result<TOk> result, Func<Task<bool>> predicate, Error errorMessage)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate().ConfigureAwait(false))
            return Result.Failure<TOk>(errorMessage);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk>> EnsureAsync<TOk>(this Result<TOk> result, Func<TOk, Task<bool>> predicate, Error errorMessage)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).ConfigureAwait(false))
            return Result.Failure<TOk>(errorMessage);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk>> EnsureAsync<TOk>(this Result<TOk> result, Func<TOk, Task<bool>> predicate, Func<TOk, Error> errorPredicate)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).ConfigureAwait(false))
            return Result.Failure<TOk>(errorPredicate(result.Value));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk>> EnsureAsync<TOk>(this Result<TOk> result, Func<TOk, Task<bool>> predicate, Func<TOk, Task<Error>> errorPredicate)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).ConfigureAwait(false))
            return Result.Failure<TOk>(await errorPredicate(result.Value).ConfigureAwait(false));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk>> EnsureAsync<TOk>(this Result<TOk> result, Func<Task<Result<TOk>>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = await predicate().ConfigureAwait(false);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk>(predicateResult.Error);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk>> EnsureAsync<TOk>(this Result<TOk> result, Func<TOk, Task<Result<TOk>>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = await predicate(result.Value).ConfigureAwait(false);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk>(predicateResult.Error);

        return result;
    }
}
