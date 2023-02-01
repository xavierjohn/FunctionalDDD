namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static Result<TOk, Err> Ensure<TOk>(this Result<TOk, Err> result, Func<bool> predicate, Err errors)
    {
        if (result.IsFailure)
            return result;

        if (!predicate())
            return Result.Failure<TOk, Err>(errors);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static Result<TOk, Err> Ensure<TOk>(this Result<TOk, Err> result, Func<TOk, bool> predicate, Err errors)
    {
        if (result.IsFailure)
            return result;

        if (!predicate(result.Ok))
            return Result.Failure<TOk, Err>(errors);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static Result<TOk, Err> Ensure<TOk>(this Result<TOk, Err> result, Func<TOk, bool> predicate, Func<TOk, Err> errorPredicate)
    {
        if (result.IsFailure)
            return result;

        if (!predicate(result.Ok))
            return Result.Failure<TOk, Err>(errorPredicate(result.Ok));

        return result;
    }


    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static Result<TOk, Err> Ensure<TOk>(this Result<TOk, Err> result, Func<Result<TOk, Err>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = predicate();

        if (predicateResult.IsFailure)
            return Result.Failure<TOk, Err>(predicateResult.Error);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static Result<TOk, Err> Ensure<TOk>(this Result<TOk, Err> result, Func<TOk, Result<TOk, Err>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = predicate(result.Ok);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk, Err>(predicateResult.Error);

        return result;
    }

    public static Result<string, Err> EnsureNotNullOrWhiteSpace(this Maybe<string> maybe, Err error) =>
        maybe.ToResult(error)
                .Ensure(name => !string.IsNullOrWhiteSpace(name), error);
}
