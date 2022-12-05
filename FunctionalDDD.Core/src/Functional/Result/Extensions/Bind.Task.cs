namespace FunctionalDDD.Core;

public static partial class AsyncResultExtensionsBothOperands
{
    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<K>> BindAsync<T, K>(this Task<Result<T>> resultTask, Func<T, Task<Result<K>>> func)
    {
        Result<T> result = await resultTask.DefaultAwait();
        return await result.BindAsync(func).DefaultAwait();
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<UnitResult> BindAsync(this Task<UnitResult> resultTask, Func<Task<UnitResult>> func)
    {
        UnitResult result = await resultTask.DefaultAwait();
        return await result.BindAsync(func).DefaultAwait();
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<T>> BindAsync<T>(this Task<UnitResult> resultTask, Func<Task<Result<T>>> func)
    {
        UnitResult result = await resultTask.DefaultAwait();
        return await result.BindAsync(func).DefaultAwait();
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<UnitResult> BindAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<UnitResult>> func)
    {
        Result<T> result = await resultTask.DefaultAwait();
        return await result.BindAsync(func).DefaultAwait();
    }
}
