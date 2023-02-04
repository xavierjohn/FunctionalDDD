
// Generated code
namespace FunctionalDDD;

public static partial class ResultExtensions
{


    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult, Error> Map<T1, T2, TResult>(
        this Result<(T1, T2), Error> result
        , Func<T1, T2, TResult> func)
    {
        if (result.IsError)
            return Result.Failure<TResult, Error>(result.Error);

        var (args1, args2) = result.Ok;
        return Result.Success<TResult, Error>(func(args1, args2));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult, Error> Map<T1, T2, T3, TResult>(
        this Result<(T1, T2, T3), Error> result
        , Func<T1, T2, T3, TResult> func)
    {
        if (result.IsError)
            return Result.Failure<TResult, Error>(result.Error);

        var (args1, args2, args3) = result.Ok;
        return Result.Success<TResult, Error>(func(args1, args2, args3));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult, Error> Map<T1, T2, T3, T4, TResult>(
        this Result<(T1, T2, T3, T4), Error> result
        , Func<T1, T2, T3, T4, TResult> func)
    {
        if (result.IsError)
            return Result.Failure<TResult, Error>(result.Error);

        var (args1, args2, args3, args4) = result.Ok;
        return Result.Success<TResult, Error>(func(args1, args2, args3, args4));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult, Error> Map<T1, T2, T3, T4, T5, TResult>(
        this Result<(T1, T2, T3, T4, T5), Error> result
        , Func<T1, T2, T3, T4, T5, TResult> func)
    {
        if (result.IsError)
            return Result.Failure<TResult, Error>(result.Error);

        var (args1, args2, args3, args4, args5) = result.Ok;
        return Result.Success<TResult, Error>(func(args1, args2, args3, args4, args5));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult, Error> Map<T1, T2, T3, T4, T5, T6, TResult>(
        this Result<(T1, T2, T3, T4, T5, T6), Error> result
        , Func<T1, T2, T3, T4, T5, T6, TResult> func)
    {
        if (result.IsError)
            return Result.Failure<TResult, Error>(result.Error);

        var (args1, args2, args3, args4, args5, args6) = result.Ok;
        return Result.Success<TResult, Error>(func(args1, args2, args3, args4, args5, args6));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult, Error> Map<T1, T2, T3, T4, T5, T6, T7, TResult>(
        this Result<(T1, T2, T3, T4, T5, T6, T7), Error> result
        , Func<T1, T2, T3, T4, T5, T6, T7, TResult> func)
    {
        if (result.IsError)
            return Result.Failure<TResult, Error>(result.Error);

        var (args1, args2, args3, args4, args5, args6, args7) = result.Ok;
        return Result.Success<TResult, Error>(func(args1, args2, args3, args4, args5, args6, args7));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult, Error> Map<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(
        this Result<(T1, T2, T3, T4, T5, T6, T7, T8), Error> result
        , Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func)
    {
        if (result.IsError)
            return Result.Failure<TResult, Error>(result.Error);

        var (args1, args2, args3, args4, args5, args6, args7, args8) = result.Ok;
        return Result.Success<TResult, Error>(func(args1, args2, args3, args4, args5, args6, args7, args8));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult, Error> Map<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(
        this Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Error> result
        , Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> func)
    {
        if (result.IsError)
            return Result.Failure<TResult, Error>(result.Error);

        var (args1, args2, args3, args4, args5, args6, args7, args8, args9) = result.Ok;
        return Result.Success<TResult, Error>(func(args1, args2, args3, args4, args5, args6, args7, args8, args9));
    }

}
