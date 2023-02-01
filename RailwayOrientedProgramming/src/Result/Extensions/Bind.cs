namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Result<TResult, Error> Bind<TOk, TResult>(this Result<TOk, Error> result, Func<TOk, Result<TResult, Error>> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error);

        return func(result.Ok);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static Task<Result<TResult, Error>> BindAsync<TOk, TResult>(this Result<TOk, Error> result, Func<TOk, Task<Result<TResult, Error>>> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error).AsCompletedTask();

        return func(result.Ok);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<TResult, Error>> BindAsync<TOk, TResult>(this Task<Result<TOk, Error>> resultTask, Func<TOk, Task<Result<TResult, Error>>> func)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);
        return await result.BindAsync(func).ConfigureAwait(false);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<TResult, Error>> BindAsync<TOk, TResult>(this Task<Result<TOk, Error>> resultTask, Func<TOk, Result<TResult, Error>> func)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);
        return result.Bind(func);
    }

    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<TResult, Error>> BindAsync<TOk, TResult>(this ValueTask<Result<TOk, Error>> resultTask, Func<TOk, ValueTask<Result<TResult, Error>>> valueTask)
    {
        Result<TOk, Error> result = await resultTask;
        return await result.BindAsync(valueTask);
    }

    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async ValueTask<Result<TResult, Error>> BindAsync<TOk, TResult>(this ValueTask<Result<TOk, Error>> resultTask, Func<TOk, Result<TResult, Error>> func)
    {
        Result<TOk, Error> result = await resultTask;
        return result.Bind(func);
    }

    /// <summary>
    ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static ValueTask<Result<TResult, Error>> BindAsync<TOk, TResult>(this Result<TOk, Error> result, Func<TOk, ValueTask<Result<TResult, Error>>> valueTask)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error).AsCompletedValueTask();

        return valueTask(result.Ok);
    }

    /// <summary>
    ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
    /// </summary>
    public static async Task<Result<TResult, Error>> BindAsync<T1, T2, TResult>(this Task<Result<(T1, T2), Error>> resultTask, Func<T1, T2, Result<TResult, Error>> func)
    {
        var result = await resultTask;
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error);

        var (args1, args2) = result.Ok;
        return func(args1, args2);
    }
}
