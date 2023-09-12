namespace FunctionalDDD.RailwayOrientedProgramming;

using FunctionalDDD.RailwayOrientedProgramming.Errors;

public static partial class EnsureExtensions
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<TOk>> EnsureAsync<TOk>(this Task<Result<TOk>> resultTask, Func<TOk, Task<bool>> predicate, Error errors)
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
    public static async Task<Result<TOk>> EnsureAsync<TOk>(this Task<Result<TOk>> resultTask, Func<TOk, Task<bool>> predicate, Func<TOk, Error> errorPredicate)
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
    public static async Task<Result<TOk>> EnsureAsync<TOk>(this Task<Result<TOk>> resultTask, Func<TOk, Task<bool>> predicate, Func<TOk, Task<Error>> errorPredicate)
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
    public static async Task<Result<TOk>> EnsureAsync<TOk>(this Task<Result<TOk>> resultTask, Func<Task<Result<TOk>>> predicate)
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
    public static async Task<Result<TOk>> EnsureAsync<TOk>(this Task<Result<TOk>> resultTask, Func<TOk, Task<Result<TOk>>> predicate)
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

