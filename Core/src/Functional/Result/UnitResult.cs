namespace FunctionalDDD.Core;

using FunctionalDDD.Core.Internal;

/// <summary>
///     Represents the result of an operation that has no return value on success, or an error on failure.
/// </summary>
/// <typeparam name="E">
///     The error type returned by a failed operation.
/// </typeparam>
public readonly partial struct UnitResult
{
    public bool IsFailure { get; }
    public bool IsSuccess => !IsFailure;

    private readonly ErrorList? _error;
    public ErrorList Errors => ResultCommonLogic.GetErrorWithSuccessGuard(IsFailure, _error);

    public Error Error => ResultCommonLogic.GetErrorWithSuccessGuard(IsFailure, _error)[0];

    internal UnitResult(bool isFailure, in ErrorList? error)
    {
        IsFailure = ResultCommonLogic.ErrorStateGuard(isFailure, error);
        _error = error;
    }

    public static implicit operator UnitResult(ErrorList error)
    {
        return Failure(error);
    }

    public static UnitResult Failure(in ErrorList error) => new UnitResult(true, error);

    /// <summary>
    ///     Creates a success result containing the given error.
    /// </summary>
    public static UnitResult Success() => new UnitResult(false, default);

}
