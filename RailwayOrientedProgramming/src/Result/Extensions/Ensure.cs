namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static Result<TOk, Error> Ensure<TOk>(this Result<TOk, Error> result, Func<bool> predicate, Error errors)
    {
        if (result.IsFailure)
            return result;

        if (!predicate())
            return Result.Failure<TOk, Error>(errors);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static Result<TOk, Error> Ensure<TOk>(this Result<TOk, Error> result, Func<TOk, bool> predicate, Error errors)
    {
        if (result.IsFailure)
            return result;

        if (!predicate(result.Value))
            return Result.Failure<TOk, Error>(errors);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
    /// </summary>
    public static Result<TOk, Error> Ensure<TOk>(this Result<TOk, Error> result, Func<TOk, bool> predicate, Func<TOk, Error> errorPredicate)
    {
        if (result.IsFailure)
            return result;

        if (!predicate(result.Value))
            return Result.Failure<TOk, Error>(errorPredicate(result.Value));

        return result;
    }


    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static Result<TOk, Error> Ensure<TOk>(this Result<TOk, Error> result, Func<Result<TOk, Error>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = predicate();

        if (predicateResult.IsFailure)
            return Result.Failure<TOk, Error>(predicateResult.Error);

        return result;
    }

    /// <summary>
    ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
    /// </summary>
    public static Result<TOk, Error> Ensure<TOk>(this Result<TOk, Error> result, Func<TOk, Result<TOk, Error>> predicate)
    {
        if (result.IsFailure)
            return result;

        var predicateResult = predicate(result.Value);

        if (predicateResult.IsFailure)
            return Result.Failure<TOk, Error>(predicateResult.Error);

        return result;
    }

    public static Result<string, Error> EnsureNotNullOrWhiteSpace(this Maybe<string> maybe, Error error) =>
        maybe.ToResult(error)
                .Ensure(name => !string.IsNullOrWhiteSpace(name), error);
}
