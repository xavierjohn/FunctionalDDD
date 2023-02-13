namespace FunctionalDDD;
public readonly struct Result<TValue, TError>
{
    public TValue Value => IsFailure ? throw new ResultFailureException<TError>(Error) : _value!;
    public TError Error => _error ?? throw new ResultSuccessException();

    public bool IsFailure { get; }
    public bool IsSuccess => !IsFailure;

    public static implicit operator Result<TValue, TError>(TValue value) => Result.Success<TValue, TError>(value);

    public static implicit operator Result<TValue, TError>(TError errors) => Result.Failure<TValue, TError>(errors);

    internal Result(bool isFailure, TValue? ok, TError? error)
    {
        if (isFailure && error is null)
            throw new ArgumentException("If 'isFailure' is true, 'error' must not be null.");

        IsFailure = isFailure;
        _error = error;
        _value = ok;
    }

    private readonly TValue? _value;
    private readonly TError? _error;
}
