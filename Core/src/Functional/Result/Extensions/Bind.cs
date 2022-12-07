namespace FunctionalDDD.Core;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult> Bind<T, TResult>(this Result<T> result, Func<T, Result<TResult>> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Errors);

        return func(result.Value);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult> Bind<T1, T2, T3, T4, TResult>(
        this Result<(T1, T2, T3, T4)> result
        , Func<T1, T2, T3, T4, Result<TResult>> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Errors);

        var (arg1, arg2, arg3, arg4) = result.Value;
        return func(arg1, arg2, arg3, arg4);
    }
}
