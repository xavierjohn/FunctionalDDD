namespace FunctionalDdd;

/// <summary>
/// Async Ensure extensions where only the RIGHT (predicates) are async (ValueTask), input is sync.
/// </summary>
public static partial class EnsureExtensionsAsync
{
    /// <summary>
    /// Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to validate.</param>
    /// <param name="predicate">The async predicate to test.</param>
    /// <param name="error">The error to return if the predicate is false.</param>
    /// <returns>The original result if success and predicate is true; otherwise a failure.</returns>
    public static async ValueTask<Result<TValue>> EnsureAsync<TValue>(this Result<TValue> result, Func<ValueTask<bool>> predicate, Error error)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(EnsureExtensions.Ensure));
        if (result.IsFailure)
        {
            result.LogActivityStatus();
            return result;
        }

        if (!await predicate().ConfigureAwait(false))
            return Result.Failure<TValue>(error);

        result.LogActivityStatus();
        return result;
    }

    /// <summary>
    /// Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to validate.</param>
    /// <param name="predicate">The async predicate to test the value.</param>
    /// <param name="error">The error to return if the predicate is false.</param>
    /// <returns>The original result if success and predicate is true; otherwise a failure.</returns>
    public static async ValueTask<Result<TValue>> EnsureAsync<TValue>(this Result<TValue> result, Func<TValue, ValueTask<bool>> predicate, Error error)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(EnsureExtensions.Ensure));
        if (result.IsFailure)
        {
            result.LogActivityStatus();
            return result;
        }

        if (!await predicate(result.Value).ConfigureAwait(false))
            return Result.Failure<TValue>(error);

        result.LogActivityStatus();
        return result;
    }

    /// <summary>
    /// Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to validate.</param>
    /// <param name="predicate">The async predicate to test the value.</param>
    /// <param name="errorPredicate">The function that generates an error from the value.</param>
    /// <returns>The original result if success and predicate is true; otherwise a failure.</returns>
    public static async ValueTask<Result<TValue>> EnsureAsync<TValue>(this Result<TValue> result, Func<TValue, ValueTask<bool>> predicate, Func<TValue, Error> errorPredicate)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(EnsureExtensions.Ensure));
        if (result.IsFailure)
        {
            result.LogActivityStatus();
            return result;
        }

        if (!await predicate(result.Value).ConfigureAwait(false))
            return Result.Failure<TValue>(errorPredicate(result.Value));

        result.LogActivityStatus();
        return result;
    }

    /// <summary>
    /// Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to validate.</param>
    /// <param name="predicate">The async predicate to test the value.</param>
    /// <param name="errorPredicate">The async function that generates an error from the value.</param>
    /// <returns>The original result if success and predicate is true; otherwise a failure.</returns>
    public static async ValueTask<Result<TValue>> EnsureAsync<TValue>(this Result<TValue> result, Func<TValue, ValueTask<bool>> predicate, Func<TValue, ValueTask<Error>> errorPredicate)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(EnsureExtensions.Ensure));
        if (result.IsFailure)
        {
            result.LogActivityStatus();
            return result;
        }

        if (!await predicate(result.Value).ConfigureAwait(false))
            return Result.Failure<TValue>(await errorPredicate(result.Value).ConfigureAwait(false));

        result.LogActivityStatus();
        return result;
    }

    /// <summary>
    /// Returns a new failure result if the predicate result is a failure. Otherwise returns the starting result.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to validate.</param>
    /// <param name="predicate">The async predicate function that returns a Result.</param>
    /// <returns>The original result if both succeed; otherwise a failure.</returns>
    public static async ValueTask<Result<TValue>> EnsureAsync<TValue>(this Result<TValue> result, Func<ValueTask<Result<TValue>>> predicate)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(EnsureExtensions.Ensure));
        if (result.IsFailure)
        {
            result.LogActivityStatus();
            return result;
        }

        var predicateResult = await predicate().ConfigureAwait(false);

        if (predicateResult.IsFailure)
            return Result.Failure<TValue>(predicateResult.Error);

        result.LogActivityStatus();
        return result;
    }

    /// <summary>
    /// Returns a new failure result if the predicate result is a failure. Otherwise returns the starting result.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to validate.</param>
    /// <param name="predicate">The async predicate function that receives the value and returns a Result.</param>
    /// <returns>The original result if both succeed; otherwise a failure.</returns>
    public static async ValueTask<Result<TValue>> EnsureAsync<TValue>(this Result<TValue> result, Func<TValue, ValueTask<Result<TValue>>> predicate)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(EnsureExtensions.Ensure));
        if (result.IsFailure)
        {
            result.LogActivityStatus();
            return result;
        }

        var predicateResult = await predicate(result.Value).ConfigureAwait(false);

        if (predicateResult.IsFailure)
            return Result.Failure<TValue>(predicateResult.Error);

        result.LogActivityStatus();
        return result;
    }
}