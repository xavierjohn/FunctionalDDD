namespace FunctionalDDD.Core;

public static partial class AsyncResultExtensionsRightOperand
{

    /// <summary>
    ///     Creates a new result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<K>> Map<T, K>(this Result<T> result, Func<T, ValueTask<K>> valueTask)
    {
        if (result.IsFailure)
            return Result.Failure<K>(result.Error);

        K value = await valueTask(result.Value);

        return Result.Success(value);
    }

    /// <summary>
    ///     Creates a new result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<K>> Map<T, K>(this Result<T> result, Func<T, ValueTask<Result<K>>> valueTask)
    {
        if (result.IsFailure)
            return Result.Failure<K>(result.Error);

        Result<K> value = await valueTask(result.Value);
        return value;
    }
}
