namespace FunctionalDdd;

/// <summary>
///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
/// </summary>
public static partial class EnsureExtensionsAsync
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<TOk, CancellationToken, ValueTask<bool>> predicate, Error errors, CancellationToken cancellationToken = default)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value, cancellationToken).ConfigureAwait(false))
            return Result.Failure<TOk>(errors);

        return result;
    }

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
    public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<TOk, CancellationToken, ValueTask<bool>> predicate, Func<TOk, Error> errorPredicate, CancellationToken cancellationToken = default)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value, cancellationToken).ConfigureAwait(false))
            return Result.Failure<TOk>(errorPredicate(result.Value));

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
    public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<TOk, CancellationToken, ValueTask<bool>> predicate, Func<TOk, CancellationToken, ValueTask<Error>> errorPredicate, CancellationToken cancellationToken = default)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value, cancellationToken).ConfigureAwait(false))
            return Result.Failure<TOk>(await errorPredicate(result.Value, cancellationToken).ConfigureAwait(false));

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
    public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<CancellationToken, ValueTask<Result<TOk>>> predicate, CancellationToken cancellationToken = default)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        var predicateResult = await predicate(cancellationToken).ConfigureAwait(false);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk>(predicateResult.Error);

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

        var predicateResult = await predicate().ConfigureAwait(false);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk>(predicateResult.Error);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> EnsureAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<TOk, CancellationToken, ValueTask<Result<TOk>>> predicate, CancellationToken cancellationToken = default)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        var predicateResult = await predicate(result.Value, cancellationToken).ConfigureAwait(false);

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

        var predicateResult = await predicate(result.Value).ConfigureAwait(false);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk>(predicateResult.Error);

        return result;
    }
}

