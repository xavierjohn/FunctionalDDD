namespace FunctionalDdd;

using System.Diagnostics;

/// <summary>
///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
/// </summary>
public static class EnsureExtensions
{
    public static Result<TValue> Ensure<TValue>(this Result<TValue> result, Func<bool> predicate, Error errors)
    {
        if (result.IsFailure)
            return result;

        if (!predicate())
            return Result.Failure<TValue>(errors);

        return result;
    }

    public static Result<TValue> Ensure<TValue>(this Result<TValue> result, Func<TValue, bool> predicate, Error error, string name = nameof(Ensure))
    {
        using var activity = Trace.ActivitySource.StartActivity(name);
        if (result.IsFailure)
            return result;

        if (!predicate(result.Value))
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            return Result.Failure<TValue>(error);
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
        return result;
    }

    public static Result<TValue> Ensure<TValue>(this Result<TValue> result, Func<TValue, bool> predicate, Func<TValue, Error> errorPredicate, string name = nameof(Ensure))
    {
        using var activity = Trace.ActivitySource.StartActivity(name);
        if (result.IsFailure)
            return result;

        if (!predicate(result.Value))
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            return Result.Failure<TValue>(errorPredicate(result.Value));
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
        return result;
    }

    public static Result<TValue> Ensure<TValue>(this Result<TValue> result, Func<Result<TValue>> predicate)
    {
        using var activity = Trace.ActivitySource.StartActivity();
        if (result.IsFailure)
            return result;

        var predicateResult = predicate();

        if (predicateResult.IsFailure)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            return Result.Failure<TValue>(predicateResult.Error);
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
        return result;
    }

    public static Result<TValue> Ensure<TValue>(this Result<TValue> result, Func<TValue, Result<TValue>> predicate)
    {
        using var activity = Trace.ActivitySource.StartActivity();
        if (result.IsFailure)
            return result;

        var predicateResult = predicate(result.Value);

        if (predicateResult.IsFailure)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            return Result.Failure<TValue>(predicateResult.Error);
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
        return result;
    }

    public static Result<string> EnsureNotNullOrWhiteSpace(this string? str, Error error)
        => string.IsNullOrWhiteSpace(str) ? Result.Failure<string>(error) : Result.Success(str);
}
