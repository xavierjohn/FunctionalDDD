namespace FunctionalDDD.RailwayOrientedProgramming;

public static partial class ResultExtensions
{
    /// <summary>
    ///     The Bind function returns a new Result from a specified function if the current Result is in a successful state.
    ///     If the current Result is in a failed state, the Bind function returns the current failed Result.
    /// </summary>
    public static Result<TResult> Bind<TOk, TResult>(this Result<TOk> result, Func<TOk, Result<TResult>> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error);

        return func(result.Value);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Task<Result<TResult>> BindAsync<TOk, TResult>(this Result<TOk> result, Func<TOk, Task<Result<TResult>>> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error).AsCompletedTask();

        return func(result.Value);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<TResult>> BindAsync<TOk, TResult>(this Task<Result<TOk>> resultTask, Func<TOk, Task<Result<TResult>>> func)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);
        return await result.BindAsync(func).ConfigureAwait(false);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<TResult>> BindAsync<TOk, TResult>(this Task<Result<TOk>> resultTask, Func<TOk, Result<TResult>> func)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);
        return result.Bind(func);
    }

    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<TResult>> BindAsync<TOk, TResult>(this ValueTask<Result<TOk>> resultTask, Func<TOk, ValueTask<Result<TResult>>> valueTask)
    {
        Result<TOk> result = await resultTask;
        return await result.BindAsync(valueTask);
    }

    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<TResult>> BindAsync<TOk, TResult>(this ValueTask<Result<TOk>> resultTask, Func<TOk, Result<TResult>> func)
    {
        Result<TOk> result = await resultTask;
        return result.Bind(func);
    }

    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static ValueTask<Result<TResult>> BindAsync<TOk, TResult>(this Result<TOk> result, Func<TOk, ValueTask<Result<TResult>>> valueTask)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error).AsCompletedValueTask();

        return valueTask(result.Value);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<TResult>> BindAsync<T1, T2, TResult>(this Task<Result<(T1, T2)>> resultTask, Func<T1, T2, Result<TResult>> func)
    {
        var result = await resultTask;
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error);

        var (args1, args2) = result.Value;
        return func(args1, args2);
    }
}
