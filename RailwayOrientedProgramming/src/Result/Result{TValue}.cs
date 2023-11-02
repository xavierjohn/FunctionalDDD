namespace FunctionalDdd;

using FunctionalDdd;

/// <summary>
/// The Result type used in functional programming languages to represent a success value or an error.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public readonly struct Result<TValue>
{
    /// <summary>
    /// Gets the underlying Value if Result is in success state.
    /// </summary>
    /// <exception cref="InvalidOperationException">Attempted to access the Value for a failed result.</exception>
    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("Attempted to access the Value for a failed result. A failed result has no Value.");

    /// <summary>
    /// Gets the Error object if Result is in failed state.
    /// </summary>
    /// <exception cref="InvalidOperationException">Attempted to access the Error property for a successful result.</exception>
    public Error Error => IsFailure ? _error! : throw new InvalidOperationException("Attempted to access the Error property for a successful result.A successful result has no Error.");

    /// <summary>
    /// Check if result is in success state.
    /// </summary>
    public bool IsSuccess => !IsFailure;

    /// <summary>
    /// Check if result is in failure state.
    /// </summary>
    public bool IsFailure { get; }

    /// <summary>
    /// Implicit operator to convert a value to a success <see cref="Result{TValue}"/>
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Result<TValue>(TValue value) => Result.Success(value);

    /// <summary>
    /// Implicit operator to convert an error to a failed <see cref="Result{TValue}"/>
    /// </summary>
    /// <param name="error"></param>
    public static implicit operator Result<TValue>(Error error) => Result.Failure<TValue>(error);

    internal Result(bool isFailure, TValue? ok, Error? error)
    {
        if (isFailure && error is null)
            throw new ArgumentException("If 'isFailure' is true, 'error' must not be null.");

        IsFailure = isFailure;
        _error = error;
        _value = ok;
    }

    private readonly TValue? _value;
    private readonly Error? _error;
}
