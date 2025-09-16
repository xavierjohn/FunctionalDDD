namespace FunctionalDdd;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

[DebuggerDisplay("{IsSuccess ? \"Success\" : \"Failure\"}, Value = {(_value is null ? \"<null>\" : _value)}, Error = {(_error is null ? \"<none>\" : _error.Code)}")]
/// <summary>
/// Represents either a successful computation (with a value) or a failure (with an <see cref="Error"/>).
/// </summary>
/// <typeparam name="TValue">Success value type.</typeparam>
public readonly struct Result<TValue> : IResult<TValue>, IEquatable<Result<TValue>>
{
    /// <summary>
    /// Gets the underlying value if the result is successful; otherwise throws.
    /// </summary>
    public TValue Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("Attempted to access the Value for a failed result. A failed result has no Value.");

    /// <summary>
    /// Gets the error if the result is a failure; otherwise throws.
    /// </summary>
    public Error Error =>
        IsFailure
            ? _error!
            : throw new InvalidOperationException("Attempted to access the Error property for a successful result. A successful result has no Error.");

    /// <summary>
    /// True when the result represents success.
    /// </summary>
    public bool IsSuccess => !IsFailure;

    /// <summary>
    /// True when the result represents failure.
    /// </summary>
    public bool IsFailure { get; }

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) => Result.Success(value);

    /// <summary>
    /// Implicitly converts an error to a failed result.
    /// </summary>
    public static implicit operator Result<TValue>(Error error) => Result.Failure<TValue>(error);

    internal Result(bool isFailure, TValue? ok, Error? error)
    {
        if (isFailure)
        {
            if (error is null)
                throw new ArgumentException("If 'isFailure' is true, 'error' must not be null.", nameof(error));
        }
        else
        {
            if (error is not null)
                throw new ArgumentException("If 'isFailure' is false, 'error' must be null.", nameof(error));
        }

        IsFailure = isFailure;
        _error = error;
        _value = isFailure ? default : ok;

        Activity.Current?.SetStatus(IsFailure ? ActivityStatusCode.Error : ActivityStatusCode.Ok);

        // Optional enrichment (safe no-op if no activity)
        if (IsFailure && Activity.Current is { } act && error is not null)
        {
            act.SetTag("result.error.code", error.Code);
        }
    }

    private readonly TValue? _value;
    private readonly Error? _error;

    // ------------- Convenience / ergonomic APIs -------------

    /// <summary>
    /// Attempts to get the success value without throwing.
    /// </summary>
    public bool TryGetValue(out TValue value)
    {
        if (IsSuccess)
        {
            value = _value!;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Attempts to get the error without throwing.
    /// </summary>
    public bool TryGetError(out Error error)
    {
        if (IsFailure)
        {
            error = _error!;
            return true;
        }

        error = default!;
        return false;
    }

    /// <summary>
    /// Deconstructs into success flag, value (may be default if failure) and error (may be null if success).
    /// </summary>
    public void Deconstruct(out bool isSuccess, out TValue? value, out Error? error)
    {
        isSuccess = IsSuccess;
        value = _value;
        error = _error;
    }

    // ------------- Equality & hashing -------------

    public bool Equals(Result<TValue> other)
    {
        if (IsFailure != other.IsFailure) return false;
        if (IsFailure) return _error!.Equals(other._error);
        return EqualityComparer<TValue>.Default.Equals(_value!, other._value);
    }

    public override bool Equals(object? obj) => obj is Result<TValue> other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            return IsFailure
                ? HashCode.Combine(true, _error)
                : HashCode.Combine(false, _value);
        }
    }

    public static bool operator ==(Result<TValue> left, Result<TValue> right) => left.Equals(right);
    public static bool operator !=(Result<TValue> left, Result<TValue> right) => !left.Equals(right);

    public override string ToString() =>
        IsFailure
            ? $"Failure({Error.Code}: {Error.Detail})"
            : $"Success({(_value is null ? "<null>" : _value)})";
}
