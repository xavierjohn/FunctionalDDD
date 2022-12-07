using System;
using System.Threading.Tasks;

namespace FunctionalDDD.Core;

public static partial class AsyncResultExtensionsRightOperand
{
    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<K>> Map<T, K>(this Result<T> result, Func<T, Task<K>> func)
    {
        if (result.IsFailure)
            return Result.Failure<K>(result.Error);

        K value = await func(result.Value).DefaultAwait();

        return Result.Success<K>(value);
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<K>> Map<T, K>(this Result<T> result, Func<T, Task<Result<K>>> func)
    {
        if (result.IsFailure)
            return Result.Failure<K>(result.Error);

        Result<K> value = await func(result.Value).DefaultAwait();
        return value;
    }
}
