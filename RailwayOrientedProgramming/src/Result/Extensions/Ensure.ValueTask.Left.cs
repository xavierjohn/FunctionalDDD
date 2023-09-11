namespace FunctionalDDD.RailwayOrientedProgramming;

public static partial class AsyncResultExtensionsLeftOperand
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<bool> predicate, Error errorMessage)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);
        return result.Ensure(predicate, errorMessage);
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<TOk, bool> predicate, Error errorMessage)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);
        return result.Ensure(predicate, errorMessage);
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<TOk, bool> predicate, Func<TOk, Error> errorPredicate)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        return result.Ensure(predicate, errorPredicate);
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<TOk, bool> predicate, Func<TOk, ValueTask<Error>> errorPredicate)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (predicate(result.Value))
            return result;

        return Result.Failure<TOk>(await errorPredicate(result.Value).ConfigureAwait(false));
    }


    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<Result<TOk>> predicate)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);
        return result.Ensure(predicate);
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<TOk, Result<TOk>> predicate)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);
        return result.Ensure(predicate);
    }

}
