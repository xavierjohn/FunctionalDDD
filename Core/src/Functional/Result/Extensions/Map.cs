using System;

namespace FunctionalDDD.Core;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<K> Map<T, K>(this Result<T> result, Func<T, K> func)
    {
        if (result.IsFailure)
            return Result.Failure<K>(result.Error);

        return Result.Success<K>(func(result.Value));
    }

}
