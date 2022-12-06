namespace FunctionalDDD.Core;

public static partial class AsyncResultExtensionsLeftOperand
{
    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<K>> Map<T, K>(this Task<Result<T>> resultTask, Func<T, K> func)
    {
        Result<T> result = await resultTask.DefaultAwait();
        return result.Map(func);
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<K>> Map<K, E>(this Task<UnitResult> resultTask, Func<K> func) 
    {
        UnitResult result = await resultTask.DefaultAwait();
        return result.Map(func);
    }
}
