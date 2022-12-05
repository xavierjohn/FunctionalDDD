namespace FunctionalDDD.Core;
using System.Threading.Tasks;

public partial struct Result
{
    /// <summary>
    ///     Creates a success result containing the given value.
    /// </summary>
    public static Result<T> Success<T>(T value) =>
        new(false, default, value);
    
    /// <summary>
    ///     Creates a failure result with the given error.
    /// </summary>
    public static Result<T> Failure<T>(ErrorList error) =>
        new(true, error, default);

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of FailureIf().
    /// </summary>
    public static Result<T> SuccessIf<T>(bool isSuccess, in T value, ErrorList error)
    {
        return isSuccess
            ? Success(value)
            : Failure<T>(error);

    }

    /// <summary>
    ///     Creates a result whose success/failure reflects the supplied condition. Opposite of SuccessIf().
    /// </summary>
    public static Result<T> FailureIf<T>(bool isFailure, T value, ErrorList error)
        => SuccessIf(!isFailure, value, error);

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static Result<T> FailureIf<T>(Func<bool> failurePredicate, in T value, ErrorList error)
        => SuccessIf(!failurePredicate(), value, error);

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
    /// </summary>
    public static async Task<Result<T>> SuccessIfAsync<T>(Func<Task<bool>> predicate, T value, ErrorList error)
    {
        bool isSuccess = await predicate().DefaultAwait();
        return SuccessIf(isSuccess, value, error);
    }

    /// <summary>
    ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of SuccessIf().
    /// </summary>
    public static async Task<Result<T>> FailureIfAsync<T>(Func<Task<bool>> failurePredicate, T value, ErrorList error)
    {
        bool isFailure = await failurePredicate().DefaultAwait();
        return SuccessIf(!isFailure, value, error);
    }
}
