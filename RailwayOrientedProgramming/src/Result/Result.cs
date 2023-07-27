namespace FunctionalDDD;
using System.Threading.Tasks;

public partial struct Result
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
}
