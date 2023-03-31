namespace FunctionalDDD;
public readonly struct Result<TValue>
{
    public TValue Value => IsFailure ? throw new ResultFailureException(Error) : _value!;
    public Error Error => _error ?? throw new ResultSuccessException();

    public bool IsFailure { get; }
    public bool IsSuccess => !IsFailure;

    public static implicit operator Result<TValue>(TValue value) => Result.Success(value);

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
