﻿namespace FunctionalDdd;

using System;

/// <summary>
///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
/// </summary>
public static class EnsureExtensions
{
    public static Result<TValue> Ensure<TValue>(this Result<TValue> result, Func<bool> predicate, Error errors)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure)
            return result;

        if (!predicate())
            return Result.Failure<TValue>(errors);

        return result;
    }

    public static Result<TValue> Ensure<TValue>(this Result<TValue> result, Func<TValue, bool> predicate, Error error)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure)
            return result;

        if (!predicate(result.Value))
            return Result.Failure<TValue>(error);

        return result;
    }

    public static Result<TValue> Ensure<TValue>(this Result<TValue> result, Func<TValue, bool> predicate, Func<TValue, Error> errorPredicate)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure)
            return result;

        if (!predicate(result.Value))
            return Result.Failure<TValue>(errorPredicate(result.Value));

        return result;
    }

    public static Result<TValue> Ensure<TValue>(this Result<TValue> result, Func<Result<TValue>> predicate)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure)
            return result;

        var predicateResult = predicate();

        if (predicateResult.IsFailure)
            return Result.Failure<TValue>(predicateResult.Error);

        return result;
    }

    public static Result<TValue> Ensure<TValue>(this Result<TValue> result, Func<TValue, Result<TValue>> predicate)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure)
            return result;

        var predicateResult = predicate(result.Value);

        if (predicateResult.IsFailure)
            return Result.Failure<TValue>(predicateResult.Error);

        return result;
    }

    public static Result<string> EnsureNotNullOrWhiteSpace(this string? str, Error error)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        return string.IsNullOrWhiteSpace(str) ? Result.Failure<string>(error) : Result.Success(str);
    }

    public static Result<Unit> Ensure(bool flag, Error error) => flag ? Result.Success() : Result.Failure(error);
}
