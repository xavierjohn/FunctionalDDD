namespace FunctionalDDD.RailwayOrientedProgramming;

using FunctionalDDD.RailwayOrientedProgramming.Errors;

public static class EnsureExtensions
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static Result<TOk> Ensure<TOk>(this Result<TOk> result, Func<bool> predicate, Error errors)
    {
        if (result.IsFailure)
            return result;

        if (!predicate())
            return Result.Failure<TOk>(errors);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static Result<TOk> Ensure<TOk>(this Result<TOk> result, Func<TOk, bool> predicate, Error error)
    {
        if (result.IsFailure)
            return result;

        if (!predicate(result.Value))
            return Result.Failure<TOk>(error);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static Result<TOk> Ensure<TOk>(this Result<TOk> result, Func<TOk, bool> predicate, Func<TOk, Error> errorPredicate)
    {
        if (result.IsFailure)
            return result;

        if (!predicate(result.Value))
            return Result.Failure<TOk>(errorPredicate(result.Value));

        return result;
    }


    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static Result<TOk> Ensure<TOk>(this Result<TOk> result, Func<Result<TOk>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = predicate();

        if (predicateResult.IsFailure)
            return Result.Failure<TOk>(predicateResult.Error);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static Result<TOk> Ensure<TOk>(this Result<TOk> result, Func<TOk, Result<TOk>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = predicate(result.Value);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk>(predicateResult.Error);

        return result;
    }

    public static Result<string> EnsureNotNullOrWhiteSpace(this string? str, Error error) =>
        str.ToResult(error)
                .Ensure(name => !string.IsNullOrWhiteSpace(name), error);
}
