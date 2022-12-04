namespace FunctionalDDD;

public static partial class AsyncResultExtensionsRightOperand
{
    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static ValueTask<Result<K>> BindAsync<T, K>(this Result<T> result, Func<T, ValueTask<Result<K>>> valueTask)
    {
        if (result.IsFailure)
            return Result.Failure<K>(result.Error).AsCompletedValueTask();

        return valueTask(result.Value);
    }

    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static ValueTask<UnitResult> BindAsync(this UnitResult result, Func<ValueTask<UnitResult>> valueTask)
    {
        if (result.IsFailure)
            return UnitResult.Failure(result.Error).AsCompletedValueTask();

        return valueTask();
    }
    
    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static ValueTask<Result<T>> BindAsync<T>(this UnitResult result, Func<ValueTask<Result<T>>> valueTask)
    {
        if (result.IsFailure)
            return Result.Failure<T>(result.Error).AsCompletedValueTask();

        return valueTask();
    }

    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static ValueTask<UnitResult> BindAsync<T>(this Result<T> result, Func<T, ValueTask<UnitResult>> valueTask)
    {
        if (result.IsFailure)
            return UnitResult.Failure(result.Error).AsCompletedValueTask();

        return valueTask(result.Value);
    }
}
