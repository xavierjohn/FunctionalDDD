namespace FunctionalDDD;
using System;

public readonly struct Result<TOk, TErr>
{
    public TOk Ok
    {
        get
        {
            if (IsFailure)
                throw new ResultFailureException<TErr>(Errs);

            if (Nullable.GetUnderlyingType(typeof(TOk)) == null && _value is null)
                throw new InvalidOperationException("Result is in success state, but value is null");

            return _value;
        }
    }

    public Errs<TErr> Errs => ResultCommonLogic.GetErrorWithSuccessGuard(IsFailure, _error);
    public bool IsFailure { get; }
    public bool IsSuccess => !IsFailure;

    public TErr Error => ResultCommonLogic.GetErrorWithSuccessGuard(IsFailure, _error)[0];

    private readonly TOk _value;
    private readonly Errs<TErr>? _error;

    internal Result(bool isFailure, Errs<TErr>? error, TOk? value)
    {
        IsFailure = ResultCommonLogic.ErrorStateGuard(isFailure, error);
        _error = error;
        _value = value ?? default!;
    }

    public static implicit operator Result<TOk, TErr>(TOk value) => Result.Success<TOk, TErr>(value);

    public static implicit operator Result<TOk, TErr>(Errs<TErr> errors) => Result.Failure<TOk, TErr>(errors);
}
