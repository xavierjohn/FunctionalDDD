namespace FunctionalDDD;

public static partial class AsyncResultExtensionsLeftOperand
{
    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<K>> BindAsync<T, K>(this ValueTask<Result<T>> resultTask, Func<T, Result<K>> valueTask)
    {
        Result<T> result = await resultTask;
        return result.Bind(valueTask);
    }

     /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<T>> BindAsync<T>(this ValueTask<UnitResult> resultTask, Func<Result<T>> valueTask)
    {
        UnitResult result = await resultTask;
        return result.Bind(valueTask);
    }

    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<UnitResult> BindAsync<T>(this ValueTask<Result<T>> resultTask, Func<T, UnitResult> valueTask)
    {
        Result<T> result = await resultTask;
        return result.Bind(valueTask);
    }
    
    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<UnitResult> BindAsync(this ValueTask<UnitResult> resultTask, Func<UnitResult> valueTask)
    {
        UnitResult result = await resultTask;
        return result.Bind(valueTask);
    }
}
