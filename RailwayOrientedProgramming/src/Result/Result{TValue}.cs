namespace FunctionalDDD.Results;

using FunctionalDDD.Results.Errors;

/// <summary>
/// This class is used to create a Result object that contains a value or an error.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public readonly struct Result<TValue>
{
    /// <summary>
    /// Gets the underlying Value if Result is in success state.
    /// </summary>
    /// <exception cref="ResultFailureException">Attempted to access the Value for a failed result.</exception>
    public TValue Value => IsFailure ? throw new ResultFailureException(Error) : _value!;

    /// <summary>
    /// Gets the Error object if Result is in failed state.
    /// </summary>
    /// <exception cref="ResultSuccessException">Attempted to access the Error property for a successful result.</exception>
    public Error Error => _error ?? throw new ResultSuccessException();

    /// <summary>
    /// Check if result is in failure state.
    /// </summary>
    public bool IsFailure { get; }

    /// <summary>
    /// Check if result is in success state.
    /// </summary>
    public bool IsSuccess => !IsFailure;

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
