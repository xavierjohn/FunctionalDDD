namespace FunctionalDDD;


public static partial class AsyncResultExtensionsBothOperands
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, ErrorList errors)
    {
        Result<T> result = await resultTask.DefaultAwait();

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).DefaultAwait())
            return Result.Failure<T>(errors);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, Func<T, ErrorList> errorPredicate)
    {
        Result<T> result = await resultTask.DefaultAwait();

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).DefaultAwait())
            return Result.Failure<T>(errorPredicate(result.Value));

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, Func<T, Task<ErrorList>> errorPredicate)
    {
        Result<T> result = await resultTask.DefaultAwait();

        if (result.IsFailure)
            return result;

        if (!await predicate(result.Value).DefaultAwait())
            return Result.Failure<T>(await errorPredicate(result.Value).DefaultAwait());

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<UnitResult> EnsureAsync(this Task<UnitResult> resultTask, Func<Task<bool>> predicate, ErrorList errorMessage)
    {
        UnitResult result = await resultTask.DefaultAwait();

        if (result.IsFailure)
            return result;

        if (!await predicate().DefaultAwait())
            return UnitResult.Failure(errorMessage);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<Task<Result<T>>> predicate)
    {
        Result<T> result = await resultTask.DefaultAwait();

        if (result.IsFailure)
            return result;

        var predicateResult = await predicate();

        if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<UnitResult>> predicate)
    {
        Result<T> result = await resultTask.DefaultAwait();

        if (result.IsFailure)
            return result;

        var predicateResult = await predicate(result.Value);

        if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);

        return result;
    }
    
    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<Result<T>>> predicate)
    {
        Result<T> result = await resultTask.DefaultAwait();

        if (result.IsFailure)
            return result;

        var predicateResult = await predicate(result.Value);

        if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);

        return result;
    }
}

