namespace FunctionalDdd;
using System.Threading.Tasks;

/// <summary>
/// Non-generic Result utility host.
/// NOTE: This struct is not intended to be instantiated.
/// </summary>
public readonly struct Result
{
    /// <summary>
    ///     Creates a success result containing the given value.
    /// </summary>
    public static Result<TValue> Success<TValue>(TValue value) =>
        new(false, value, default);

    /// <summary>
    ///     Creates a success result containing the given value via deferred factory.
    /// </summary>
    public static Result<TValue> Success<TValue>(Func<TValue> funcOk)
    {
        TValue value = funcOk();
        return new(false, value, default);
    }

    /// <summary>
    ///     Creates a failure result with the given error.
    /// </summary>
    public static Result<TValue> Failure<TValue>(Error error) =>
        new(true, default, error);

    /// <summary>
    ///     Creates a failure result with the given error via deferred factory.
    /// </summary>
    public static Result<TValue> Failure<TValue>(Func<Error> error)
    {
        Error err = error();
        return new(true, default, err);
    }

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of FailureIf().
    /// </summary>
    public static Result<TValue> SuccessIf<TValue>(bool isSuccess, in TValue value, Error error)
        => isSuccess ? Success(value) : Failure<TValue>(error);

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of FailureIf().
    /// </summary>
    public static Result<(T1, T2)> SuccessIf<T1, T2>(bool isSuccess, in T1 t1, in T2 t2, Error error)
        => isSuccess ? Success((t1, t2)) : Failure<(T1, T2)>(error);

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of SuccessIf().
    /// </summary>
    public static Result<TValue> FailureIf<TValue>(bool isFailure, TValue value, Error error)
        => SuccessIf(!isFailure, value, error);

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static Result<TValue> FailureIf<TValue>(Func<bool> failurePredicate, in TValue value, Error error)
        => SuccessIf(!failurePredicate(), value, error);

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied async predicate. Opposite of FailureIf().
    /// </summary>
    public static async Task<Result<TValue>> SuccessIfAsync<TValue>(Func<Task<bool>> predicate, TValue value, Error error)
    {
        bool isSuccess = await predicate().ConfigureAwait(false);
        return SuccessIf(isSuccess, value, error);
    }

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied async predicate. Opposite of SuccessIf().
    /// </summary>
    public static async Task<Result<TValue>> FailureIfAsync<TValue>(Func<Task<bool>> failurePredicate, TValue value, Error error)
    {
        bool isFailure = await failurePredicate().ConfigureAwait(false);
        return SuccessIf(!isFailure, value, error);
    }

    /// <summary>
    ///     Creates a success unit result.
    /// </summary>
    public static Result<Unit> Success() =>
        new(false, default, default);

    /// <summary>
    ///     Creates a failed unit result with the given error.
    /// </summary>
    public static Result<Unit> Failure(Error error) =>
        new(true, default, error);
}
