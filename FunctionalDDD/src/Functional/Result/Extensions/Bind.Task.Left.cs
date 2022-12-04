namespace FunctionalDDD;

public static partial class AsyncResultExtensionsLeftOperand
{
    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<K>> BindAsync<T, K>(this Task<Result<T>> resultTask, Func<T, Result<K>> func)
    {
        Result<T> result = await resultTask.DefaultAwait();
        return result.Bind(func);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<UnitResult> BindAsync(this Task<UnitResult> resultTask, Func<UnitResult> func)
    {
        UnitResult result = await resultTask.DefaultAwait();
        return result.Bind(func);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<T>> BindAsync<T>(this Task<UnitResult> resultTask, Func<Result<T>> func)
    {
        UnitResult result = await resultTask.DefaultAwait();
        return result.Bind(func);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<UnitResult> BindAsync<T>(this Task<Result<T>> resultTask, Func<T, UnitResult> func)
    {
        Result<T> result = await resultTask.DefaultAwait();
        return result.Bind(func);
    }
}
