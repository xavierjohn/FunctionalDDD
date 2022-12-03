namespace FunctionalDDD;

public static class AsyncResultExtensionsRightOperand
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<Task<bool>> predicate, ErrorList errorMessage)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate().DefaultAwait())
            return Result.Failure<T>(errorMessage);

        return result;
    }
    
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<bool>> predicate, ErrorList errorMessage)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).DefaultAwait())
            return Result.Failure<T>(errorMessage);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<bool>> predicate, Func<T, ErrorList> errorPredicate)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).DefaultAwait())
            return Result.Failure<T>(errorPredicate(result.Value));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<bool>> predicate, Func<T, Task<ErrorList>> errorPredicate)
    {
        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).DefaultAwait())
            return Result.Failure<T>(await errorPredicate(result.Value).DefaultAwait());

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<Task<Result<T>>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = await predicate().DefaultAwait();

        if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<Result<T>>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = await predicate(result.Value).DefaultAwait();

        if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);

        return result;
    }
}
