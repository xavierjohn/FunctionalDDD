namespace FunctionalDDD;
using System.Threading.Tasks;

public partial struct Result
{
    /// <summary>
    ///     Creates a success result containing the given value.
    /// </summary>
    public static Result<TOk, Err> Success<TOk>(TOk value) =>
        new(false, default, value);

    /// <summary>
    ///     Creates a failure result with the given error.
    /// </summary>
    public static Result<TOk, Err> Failure<TOk>(Errs<Err> error) =>
        new(true, error, default);

    /// <summary>
    ///     Creates a success result containing the given value.
    /// </summary>
    public static Result<TOk, TErr> Success<TOk, TErr>(TOk value) =>
        new(false, default, value);

    /// <summary>
    ///     Creates a failure result with the given error.
    /// </summary>
    public static Result<TOk, TErr> Failure<TOk, TErr>(Errs<TErr> error) =>
        new(true, error, default);

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of FailureIf().
    /// </summary>
    public static Result<TOk, Err> SuccessIf<TOk>(bool isSuccess, in TOk value, Errs<Err> errors)
    {
        return isSuccess
            ? Success<TOk, Err>(value)
            : Failure<TOk, Err>(errors);

    }

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of SuccessIf().
    /// </summary>
    public static Result<TOk, Err> FailureIf<TOk>(bool isFailure, TOk value, Errs<Err> error)
        => SuccessIf(!isFailure, value, error);

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static Result<TOk, Err> FailureIf<TOk>(Func<bool> failurePredicate, in TOk value, Errs<Err> error)
        => SuccessIf(!failurePredicate(), value, error);

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of FailureIf().
    /// </summary>
    public static Result<TOk, TErr> SuccessIf<TOk, TErr>(bool isSuccess, in TOk value, Errs<TErr> errors)
    {
        return isSuccess
            ? Success<TOk, TErr>(value)
            : Failure<TOk, TErr>(errors);

    }

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of SuccessIf().
    /// </summary>
    public static Result<TOk, TErr> FailureIf<TOk, TErr>(bool isFailure, TOk value, Errs<TErr> error)
        => SuccessIf(!isFailure, value, error);

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static Result<TOk, TErr> FailureIf<TOk, TErr>(Func<bool> failurePredicate, in TOk value, Errs<TErr> error)
        => SuccessIf(!failurePredicate(), value, error);

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
    /// </summary>
    public static async Task<Result<TOk, Err>> SuccessIfAsync<TOk>(Func<Task<bool>> predicate, TOk value, Errs<Err> error)
    {
        bool isSuccess = await predicate().ConfigureAwait(false);
        return SuccessIf(isSuccess, value, error);
    }

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static async Task<Result<TOk, Err>> FailureIfAsync<TOk>(Func<Task<bool>> failurePredicate, TOk value, Errs<Err> error)
    {
        bool isFailure = await failurePredicate().ConfigureAwait(false);
        return SuccessIf(!isFailure, value, error);
    }

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
    /// </summary>
    public static async Task<Result<TOk, TErr>> SuccessIfAsync<TOk, TErr>(Func<Task<bool>> predicate, TOk value, Errs<TErr> error)
    {
        bool isSuccess = await predicate().ConfigureAwait(false);
        return SuccessIf(isSuccess, value, error);
    }

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static async Task<Result<TOk, TErr>> FailureIfAsync<TOk, TErr>(Func<Task<bool>> failurePredicate, TOk value, Errs<TErr> error)
    {
        bool isFailure = await failurePredicate().ConfigureAwait(false);
        return SuccessIf(!isFailure, value, error);
    }
}
