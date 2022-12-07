namespace FunctionalDDD.Core;

public static partial class AsyncResultExtensionsBothOperands
{
    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<K>> BindAsync<T, K>(this ValueTask<Result<T>> resultTask, Func<T, ValueTask<Result<K>>> valueTask)
    {
        Result<T> result = await resultTask;
        return await result.BindAsync(valueTask);
    }
}
