namespace FunctionalDDD.RailwayOrientedProgramming;
using System.Threading.Tasks;
using FunctionalDDD.RailwayOrientedProgramming.Errors;

/// <summary>
/// Static methods to create the <see cref="Result{TValue}"/> object.
/// </summary>
public static class Result
{
    /// <summary>
    ///     Creates a success result containing the given value.
    /// </summary>
    public static Result<TOk> Success<TOk>(TOk ok) =>
        new(false, ok, default);

    /// <summary>
    ///     Creates a success result containing the given value.
    /// </summary>
    public static Result<TOk> Success<TOk>(Func<TOk> fok)
    {
        TOk ok = fok();
        return new(false, ok, default);
    }

    /// <summary>
    ///     Creates a failure result with the given error.
    /// </summary>
    public static Result<TOk> Failure<TOk>(Error error) =>
        new(true, default, error);

    /// <summary>
    ///     Creates a failure result with the given error.
    /// </summary>
    public static Result<TOk> Failure<TOk>(Func<Error> error)
    {
        Error err = error();
        return new(true, default, err);
    }

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of FailureIf().
    /// </summary>
    public static Result<TOk> SuccessIf<TOk>(bool isSuccess, in TOk value, Error error)
    {
        return isSuccess
            ? Success(value)
            : Failure<TOk>(error);

    }

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of SuccessIf().
    /// </summary>
    public static Result<TOk> FailureIf<TOk>(bool isFailure, TOk value, Error error)
        => SuccessIf(!isFailure, value, error);

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static Result<TOk> FailureIf<TOk>(Func<bool> failurePredicate, in TOk value, Error error)
        => SuccessIf(!failurePredicate(), value, error);

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
    /// </summary>
    public static async Task<Result<TOk>> SuccessIfAsync<TOk>(Func<Task<bool>> predicate, TOk value, Error error)
    {
        bool isSuccess = await predicate().ConfigureAwait(false);
        return SuccessIf(isSuccess, value, error);
    }

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static async Task<Result<TOk>> FailureIfAsync<TOk>(Func<Task<bool>> failurePredicate, TOk value, Error error)
    {
        bool isFailure = await failurePredicate().ConfigureAwait(false);
        return SuccessIf(!isFailure, value, error);
    }

    internal static class Messages
    {
        public static readonly string ErrorIsInaccessibleForSuccess =
            "You attempted to access the Error property for a successful result. A successful result has no Error.";

        public static string ValueIsInaccessibleForFailure() =>
            "You attempted to access the Value for a failed result. A failed result has no Value.";

        public static readonly string ErrorObjectIsNotProvidedForFailure =
            "You attempted to create a failure result, which must have an error, but a null error object (or empty string) was passed to the constructor.";

        public static readonly string ErrorObjectIsProvidedForSuccess =
            "You attempted to create a success result, which cannot have an error, but a non-null error object was passed to the constructor.";

        public static readonly string ConvertFailureExceptionOnSuccess =
            "Convert failed because the Result is in a success state.";
    }
}
