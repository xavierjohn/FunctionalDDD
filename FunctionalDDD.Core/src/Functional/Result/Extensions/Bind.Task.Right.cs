namespace FunctionalDDD.Core;

public static partial class AsyncResultExtensionsRightOperand
{
    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Task<Result<K>> BindAsync<T, K>(this Result<T> result, Func<T, Task<Result<K>>> func)
    {
        if (result.IsFailure)
            return Result.Failure<K>(result.Error).AsCompletedTask();

        return func(result.Value);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Task<UnitResult> BindAsync(this UnitResult result, Func<Task<UnitResult>> func)
    {
        if (result.IsFailure)
            return UnitResult.Failure(result.Error).AsCompletedTask();

        return func();
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Task<Result<T>> BindAsync<T>(this UnitResult result, Func<Task<Result<T>>> func)
    {
        if (result.IsFailure)
            return Result.Failure<T>(result.Error).AsCompletedTask();

        return func();
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Task<UnitResult> BindAsync<T>(this Result<T> result, Func<T, Task<UnitResult>> func)
    {
        if (result.IsFailure)
            return UnitResult.Failure(result.Error).AsCompletedTask();

        return func(result.Value);
    }
}
