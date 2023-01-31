namespace FunctionalDDD;
public readonly struct Result<TOk, TErr>
{
    public TOk Ok => _ok ?? throw new ResultFailureException<TErr>(Error);
    public TErr Error => _error ?? throw new ResultSuccessException();

    public bool IsFailure => _ok is null;
    public bool IsSuccess => !IsFailure;

    internal Result(TOk? ok, TErr? error)
    {
        if (ok is null && error is null)
            throw new ArgumentException("Either 'ok' or 'error' must be non-null.");
        _error = error;
        _ok = ok;
    }

    private readonly TOk? _ok;
    private readonly TErr? _error;
}
