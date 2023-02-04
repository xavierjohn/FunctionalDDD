namespace FunctionalDDD;
public readonly struct Result<TOk, TErr>
{
    public TOk Ok => IsError ? throw new ResultFailureException<TErr>(Error) : _ok!;
    public TErr Error => _error ?? throw new ResultSuccessException();

    public bool IsError { get; }
    public bool IsOk => !IsError;

    public static implicit operator Result<TOk, TErr>(TOk value) => Result.Success<TOk, TErr>(value);

    public static implicit operator Result<TOk, TErr>(TErr errors) => Result.Failure<TOk, TErr>(errors);

    internal Result(bool isFailure, TOk? ok, TErr? error)
    {
        if (isFailure && error is null)
            throw new ArgumentException("If 'isFailure' is true, 'error' must not be null.");

        IsError = isFailure;
        _error = error;
        _ok = ok;
    }

    private readonly TOk? _ok;
    private readonly TErr? _error;
}
