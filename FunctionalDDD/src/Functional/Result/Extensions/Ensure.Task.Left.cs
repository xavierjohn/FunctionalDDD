namespace FunctionalDDD;

public static class AsyncResultExtensionsLeftOperand
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<bool> predicate, ErrorList errorMessage)
    {
        Result<T> result = await resultTask.DefaultAwait();
        return result.Ensure(predicate, errorMessage);
    }
    
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, ErrorList errorMessage)
    {
        Result<T> result = await resultTask.DefaultAwait();
        return result.Ensure(predicate, errorMessage);
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Func<T, ErrorList> errorPredicate)
    {
        Result<T> result = await resultTask.DefaultAwait();

        if (result.IsFailure)
            return result;

        return result.Ensure(predicate, errorPredicate);
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Func<T, Task<ErrorList>> errorPredicate)
    {
        Result<T> result = await resultTask.DefaultAwait();

        if (result.IsFailure)
            return result;

        if (predicate(result.Value))
            return result;

        return Result.Failure<T>(await errorPredicate(result.Value).DefaultAwait());
    }


    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<Result<T>> predicate)
    {
      Result<T> result = await resultTask.DefaultAwait();
      return result.Ensure(predicate);
    }
    
    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T,Result<T>> predicate)
    {
      Result<T> result = await resultTask.DefaultAwait();
      return result.Ensure(predicate);
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static async Task<UnitResult> EnsureAsync(this Task<UnitResult> resultTask, Func<bool> predicate, ErrorList errorMessage)
    {
        UnitResult result = await resultTask.DefaultAwait();
        return result.Ensure(predicate, errorMessage);
    }
}
