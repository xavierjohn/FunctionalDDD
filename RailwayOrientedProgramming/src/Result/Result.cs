namespace FunctionalDDD;
using System.Threading.Tasks;

public partial struct Result
{
    /// <summary>
    ///     Creates a success result containing the given value.
    /// </summary>
    public static Result<TOk, Error> Success<TOk>(TOk ok) =>
        new(false, ok, default);

    /// <summary>
    ///     Creates a failure result with the given error.
    /// </summary>
    public static Result<TOk, Error> Failure<TOk>(Error error) =>
        new(true, default, error);

    /// <summary>
    ///     Creates a success result containing the given value.
    /// </summary>
    public static Result<TOk, TErr> Success<TOk, TErr>(TOk value) =>
        new(false, value, default);

    /// <summary>
    ///     Creates a failure result with the given error.
    /// </summary>
    public static Result<TOk, TErr> Failure<TOk, TErr>(TErr error) =>
        new(true, default, error);

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of FailureIf().
    /// </summary>
    public static Result<TOk, Error> SuccessIf<TOk>(bool isSuccess, in TOk value, Error error)
    {
        return isSuccess
            ? Success<TOk, Error>(value)
            : Failure<TOk, Error>(error);

    }

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of SuccessIf().
    /// </summary>
    public static Result<TOk, Error> FailureIf<TOk>(bool isFailure, TOk value, Error error)
        => SuccessIf(!isFailure, value, error);

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static Result<TOk, Error> FailureIf<TOk>(Func<bool> failurePredicate, in TOk value, Error error)
        => SuccessIf(!failurePredicate(), value, error);

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of FailureIf().
    /// </summary>
    public static Result<TOk, TErr> SuccessIf<TOk, TErr>(bool isSuccess, in TOk value, TErr error)
    {
        return isSuccess
            ? Success<TOk, TErr>(value)
            : Failure<TOk, TErr>(error);

    }

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of SuccessIf().
    /// </summary>
    public static Result<TOk, TErr> FailureIf<TOk, TErr>(bool isFailure, TOk value, TErr error)
        => SuccessIf(!isFailure, value, error);

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static Result<TOk, TErr> FailureIf<TOk, TErr>(Func<bool> failurePredicate, in TOk value, TErr error)
        => SuccessIf(!failurePredicate(), value, error);

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
    /// </summary>
    public static async Task<Result<TOk, Error>> SuccessIfAsync<TOk>(Func<Task<bool>> predicate, TOk value, Error error)
    {
        bool isSuccess = await predicate().ConfigureAwait(false);
        return SuccessIf(isSuccess, value, error);
    }

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static async Task<Result<TOk, Error>> FailureIfAsync<TOk>(Func<Task<bool>> failurePredicate, TOk value, Error error)
    {
        bool isFailure = await failurePredicate().ConfigureAwait(false);
        return SuccessIf(!isFailure, value, error);
    }

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
    /// </summary>
    public static async Task<Result<TOk, TErr>> SuccessIfAsync<TOk, TErr>(Func<Task<bool>> predicate, TOk value, TErr error)
    {
        bool isSuccess = await predicate().ConfigureAwait(false);
        return SuccessIf(isSuccess, value, error);
    }

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static async Task<Result<TOk, TErr>> FailureIfAsync<TOk, TErr>(Func<Task<bool>> failurePredicate, TOk value, TErr error)
    {
        bool isFailure = await failurePredicate().ConfigureAwait(false);
        return SuccessIf(!isFailure, value, error);
    }
}
