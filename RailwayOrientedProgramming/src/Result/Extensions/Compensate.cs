namespace FunctionalDdd;

/// <summary>
/// Compensate for failed result by calling the given function.
/// </summary>
public static class CompensateExtensions
{
    /// <summary>
    /// Compensate for failed result by calling the given function.
    /// </summary>
    public static Result<T> Compensate<T>(this Result<T> result, Func<Result<T>> func)
    {
        using var activity = Trace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        return func();
    }

    /// <summary>
    /// Compensate for failed result by calling the given function with the failed error.
    /// </summary>
    public static Result<T> Compensate<T>(this Result<T> result, Func<Error, Result<T>> func)
    {
        using var activity = Trace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        return func(result.Error);
    }
}

/// <summary>
/// Compensate for failed result by calling the given function.
/// </summary>
public static class CompensateExtensionsAsync
{
    /// <summary>
    /// Compensate for failed result by calling the given function.
    /// </summary>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Result<T>> func)
    {
        Result<T> result = await resultTask;
        return result.Compensate(func);
    }

    /// <summary>
    /// Compensate for failed result by calling the given function.
    /// </summary>
    public static async Task<Result<T>> CompensateAsync<T>(this Result<T> result, Func<Task<Result<T>>> funcAsync)
    {
        using var activity = Trace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        return await funcAsync();
    }

    /// <summary>
    /// Compensate for failed result by calling the given function.
    /// </summary>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Task<Result<T>>> funcAsync)
    {
        Result<T> result = await resultTask;
        return await result.CompensateAsync(funcAsync);
    }

    /// <summary>
    /// Compensate for failed result by calling the given function.
    /// </summary>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Error, Result<T>> func)
    {
        Result<T> result = await resultTask;
        return result.Compensate(func);
    }

    /// <summary>
    /// Compensate for failed result by calling the given function.
    /// </summary>
    public static async Task<Result<T>> CompensateAsync<T>(this Result<T> result, Func<Error, Task<Result<T>>> funcAsync)
    {
        using var activity = Trace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        return await funcAsync(result.Error);
    }

    /// <summary>
    /// Compensate for failed result by calling the given function.
    /// </summary>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Error, Task<Result<T>>> funcAsync)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        
        return await result.CompensateAsync(funcAsync);
    }
}
