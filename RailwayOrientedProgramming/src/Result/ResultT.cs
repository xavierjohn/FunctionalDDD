namespace FunctionalDDD;
using System;

public readonly struct Result<T>
{
    public bool IsFailure { get; }
    public bool IsSuccess => !IsFailure;

    private readonly Errs? _error;
    public Errs Errs => ResultCommonLogic.GetErrorWithSuccessGuard(IsFailure, _error);

    public Err Err => ResultCommonLogic.GetErrorWithSuccessGuard(IsFailure, _error)[0];

    private readonly T _value;

    public T Ok
    {
        get
        {
            if (IsFailure)
                throw new ResultFailureException(Errs);

            if (Nullable.GetUnderlyingType(typeof(T)) == null && _value is null)
                throw new InvalidOperationException("Result is in success state, but value is null");

            return _value;
        }
    }

    internal Result(bool isFailure, Errs? error, T? value)
    {
        IsFailure = ResultCommonLogic.ErrorStateGuard(isFailure, error);
        _error = error;
        _value = value ?? default!;
    }

    public static implicit operator Result<T>(T value) => Result.Success(value);

    public static implicit operator Result<T>(Errs errors) => Result.Failure<T>(errors);
}
