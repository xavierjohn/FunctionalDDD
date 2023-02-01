namespace FunctionalDDD;
public readonly struct Result<TOk, TErr>
{
    public TOk Ok => IsFailure ? throw new ResultFailureException<TErr>(Error) : _ok!;
    public TErr Error => _error ?? throw new ResultSuccessException();

    public bool IsFailure { get; }
    public bool IsSuccess => !IsFailure;

    public static implicit operator Result<TOk, TErr>(TOk value) => Result.Success<TOk, TErr>(value);

    public static implicit operator Result<TOk, TErr>(TErr errors) => Result.Failure<TOk, TErr>(errors);

    internal Result(bool isFailure, TOk? ok, TErr? error)
    {
        if (isFailure && error is null)
            throw new ArgumentException("If 'isFailure' is true, 'error' must not be null.");

        IsFailure = isFailure;
        _error = error;
        _ok = ok;
    }

    private readonly TOk? _ok;
    private readonly TErr? _error;
}
