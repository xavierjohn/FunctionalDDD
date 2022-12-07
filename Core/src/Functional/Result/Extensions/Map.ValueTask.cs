namespace FunctionalDDD.Core;

public static partial class AsyncResultExtensionsBothOperands
{
    /// <summary>
    ///     Creates a new result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<K>> Map<T, K>(this ValueTask<Result<T>> resultTask, Func<T, ValueTask<K>> valueTask)
    {
        Result<T> result = await resultTask;

        if (result.IsFailure)
            return Result.Failure<K>(result.Error);

        K value = await valueTask(result.Value);

        return Result.Success(value);
    }
}
