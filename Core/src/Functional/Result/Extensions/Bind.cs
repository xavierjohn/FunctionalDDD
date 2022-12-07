namespace FunctionalDDD.Core;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<K> Bind<T, K>(this Result<T> result, Func<T, Result<K>> func)
    {
        if (result.IsFailure)
            return Result.Failure<K>(result.Error);

        return func(result.Value);
    }
}
