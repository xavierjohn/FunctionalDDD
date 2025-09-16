namespace FunctionalDdd;
using System;
using System.Threading.Tasks;

/// <summary>
/// Non-generic Result utility host.
/// NOTE: This struct is not intended to be instantiated.
/// </summary>
public readonly struct Result
{
    public static Result<TValue> Success<TValue>(TValue value) =>
        new(false, value, default);

    public static Result<TValue> Success<TValue>(Func<TValue> funcOk)
    {
        TValue value = funcOk();
        return new(false, value, default);
    }

    public static Result<TValue> Failure<TValue>(Error error) =>
        new(true, default, error);

    public static Result<TValue> Failure<TValue>(Func<Error> error)
    {
        Error err = error();
        return new(true, default, err);
    }

    public static Result<TValue> SuccessIf<TValue>(bool isSuccess, in TValue value, Error error)
        => isSuccess ? Success(value) : Failure<TValue>(error);

    public static Result<(T1, T2)> SuccessIf<T1, T2>(bool isSuccess, in T1 t1, in T2 t2, Error error)
        => isSuccess ? Success((t1, t2)) : Failure<(T1, T2)>(error);

    public static Result<TValue> FailureIf<TValue>(bool isFailure, TValue value, Error error)
        => SuccessIf(!isFailure, value, error);

    public static Result<TValue> FailureIf<TValue>(Func<bool> failurePredicate, in TValue value, Error error)
        => SuccessIf(!failurePredicate(), value, error);

    public static async Task<Result<TValue>> SuccessIfAsync<TValue>(Func<Task<bool>> predicate, TValue value, Error error)
    {
        bool isSuccess = await predicate().ConfigureAwait(false);
        return SuccessIf(isSuccess, value, error);
    }

    public static async Task<Result<TValue>> FailureIfAsync<TValue>(Func<Task<bool>> failurePredicate, TValue value, Error error)
    {
        bool isFailure = await failurePredicate().ConfigureAwait(false);
        return SuccessIf(!isFailure, value, error);
    }

    public static Result<Unit> Success() =>
        new(false, default, default);

    public static Result<Unit> Failure(Error error) =>
        new(true, default, error);

    // --- New: exception capture helpers --------------------------------------------------

    /// <summary>
    /// Executes the function and converts exceptions to a failed Result using the optional mapper (default Unexpected).
    /// </summary>
    public static Result<T> Try<T>(Func<T> func, Func<Exception, Error>? map = null)
    {
        try
        {
            return Success(func());
        }
        catch (Exception ex)
        {
            return Failure<T>((map ?? DefaultExceptionMapper)(ex));
        }
    }

    /// <summary>
    /// Executes the async function and converts exceptions to a failed Result using the optional mapper (default Unexpected).
    /// </summary>
    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func, Func<Exception, Error>? map = null)
    {
        try
        {
            return Success(await func().ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            return Failure<T>((map ?? DefaultExceptionMapper)(ex));
        }
    }

    /// <summary>
    /// Converts an exception to a failed unit Result using the optional mapper (default Unexpected).
    /// </summary>
    public static Result<Unit> FromException(Exception ex, Func<Exception, Error>? map = null) =>
        Failure((map ?? DefaultExceptionMapper)(ex));

    /// <summary>
    /// Converts an exception to a failed generic Result using the optional mapper (default Unexpected).
    /// </summary>
    public static Result<T> FromException<T>(Exception ex, Func<Exception, Error>? map = null) =>
        Failure<T>((map ?? DefaultExceptionMapper)(ex));

    private static UnexpectedError DefaultExceptionMapper(Exception ex) =>
        Error.Unexpected(ex.Message);
}
