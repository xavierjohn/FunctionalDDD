namespace FunctionalDDD.RailwayOrientedProgramming;
using System;

public readonly partial struct Result<T>
{
    public bool IsFailure { get; }
    public bool IsSuccess => !IsFailure;

    private readonly ErrorList? _error;
    public ErrorList Errors => ResultCommonLogic.GetErrorWithSuccessGuard(IsFailure, _error);

    public Error Error => ResultCommonLogic.GetErrorWithSuccessGuard(IsFailure, _error)[0];

    private readonly T _value;

    public T Value
    {
        get
        {
            if (IsFailure)
                throw new ResultFailureException(Errors);

            if (Nullable.GetUnderlyingType(typeof(T)) == null && _value is null)
                throw new InvalidOperationException("Result is in success state, but value is null");

            return _value;
        }
    }

    internal Result(bool isFailure, ErrorList? error, T? value)
    {
        IsFailure = ResultCommonLogic.ErrorStateGuard(isFailure, error);
        _error = error;
        _value = value ?? default!;
    }

    public static implicit operator Result<T>(T value) => Result.Success(value);

    public static implicit operator Result<T>(ErrorList errors) => Result.Failure<T>(errors);
}
