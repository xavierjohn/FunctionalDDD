﻿namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static Result<T> Ensure<T>(this Result<T> result, Func<bool> predicate, ErrorList errors)
    {
        if (result.IsFailure)
            return result;

        if (!predicate())
            return Result.Failure<T>(errors);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, ErrorList errors)
    {
        if (result.IsFailure)
            return result;

        if (!predicate(result.Value))
            return Result.Failure<T>(errors);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Func<T, ErrorList> errorPredicate)
    {
        if (result.IsFailure)
            return result;

        if (!predicate(result.Value))
            return Result.Failure<T>(errorPredicate(result.Value));

        return result;
    }


    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static Result<T> Ensure<T>(this Result<T> result, Func<Result<T>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = predicate();

        if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, Result<T>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = predicate(result.Value);

        if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);

        return result;
    }

    public static Result<string> EnsureNotNullOrWhiteSpace(this Maybe<string> maybe, Error error) =>
        maybe.ToResult(error)
                .Ensure(name => !string.IsNullOrWhiteSpace(name), error);
}
