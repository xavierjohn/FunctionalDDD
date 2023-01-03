﻿
// Generated code
namespace FunctionalDDD;

public static partial class ResultExtensions
{


    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult> Map<T1, T2, TResult>(
        this Result<(T1, T2)> result
        , Func<T1, T2, TResult> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Errors);

        var (args1, args2) = result.Value;
        return Result.Success(func(args1, args2));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult> Map<T1, T2, T3, TResult>(
        this Result<(T1, T2, T3)> result
        , Func<T1, T2, T3, TResult> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Errors);

        var (args1, args2, args3) = result.Value;
        return Result.Success(func(args1, args2, args3));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult> Map<T1, T2, T3, T4, TResult>(
        this Result<(T1, T2, T3, T4)> result
        , Func<T1, T2, T3, T4, TResult> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Errors);

        var (args1, args2, args3, args4) = result.Value;
        return Result.Success(func(args1, args2, args3, args4));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult> Map<T1, T2, T3, T4, T5, TResult>(
        this Result<(T1, T2, T3, T4, T5)> result
        , Func<T1, T2, T3, T4, T5, TResult> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Errors);

        var (args1, args2, args3, args4, args5) = result.Value;
        return Result.Success(func(args1, args2, args3, args4, args5));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult> Map<T1, T2, T3, T4, T5, T6, TResult>(
        this Result<(T1, T2, T3, T4, T5, T6)> result
        , Func<T1, T2, T3, T4, T5, T6, TResult> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Errors);

        var (args1, args2, args3, args4, args5, args6) = result.Value;
        return Result.Success(func(args1, args2, args3, args4, args5, args6));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult> Map<T1, T2, T3, T4, T5, T6, T7, TResult>(
        this Result<(T1, T2, T3, T4, T5, T6, T7)> result
        , Func<T1, T2, T3, T4, T5, T6, T7, TResult> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Errors);

        var (args1, args2, args3, args4, args5, args6, args7) = result.Value;
        return Result.Success(func(args1, args2, args3, args4, args5, args6, args7));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult> Map<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(
        this Result<(T1, T2, T3, T4, T5, T6, T7, T8)> result
        , Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Errors);

        var (args1, args2, args3, args4, args5, args6, args7, args8) = result.Value;
        return Result.Success(func(args1, args2, args3, args4, args5, args6, args7, args8));
    }

    /// <summary>
    ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult> Map<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(
        this Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> result
        , Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Errors);

        var (args1, args2, args3, args4, args5, args6, args7, args8, args9) = result.Value;
        return Result.Success(func(args1, args2, args3, args4, args5, args6, args7, args8, args9));
    }

}
