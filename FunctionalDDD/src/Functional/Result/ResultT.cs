namespace FunctionalDDD;
using System;
using FunctionalDDD.Internal;

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

    public static implicit operator Result<T>(T value)
    {
        if (value is Result<T> result)
        {
            ErrorList? resultError = result.IsFailure ? result.Errors : default;
            T? resultValue = result.IsSuccess ? result.Value : default;

            return new Result<T>(result.IsFailure, resultError, resultValue);
        }

        return Result.Success(value);
    }

    public static implicit operator Result<T>(ErrorList errors)
    {
        return Result.Failure<T>(errors);
    }

    public static implicit operator UnitResult(Result<T> result)
    {
        if (result.IsSuccess)
            return UnitResult.Success();
        else
            return UnitResult.Failure(result.Error);
    }
}
