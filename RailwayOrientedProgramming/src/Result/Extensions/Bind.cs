namespace FunctionalDdd;

/// <summary>
/// If the starting Result is a success, the Bind function will return a new Result from the given function.
/// Otherwise, the Bind function will return the starting failed Result.
/// </summary>
public static partial class BindExtensions
{
    public static Result<TResult> Bind<TValue, TResult>(this Result<TValue> result, Func<TValue, Result<TResult>> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error);

        return func(result.Value);
    }
}

/// <summary>
/// If the starting Result is a success, the Bind function will return a new Result from the given function.
/// Otherwise, the Bind function will return the starting failed Result.
/// </summary>
public static partial class BindExtensionsAsync
{
    public static Task<Result<TResult>> BindAsync<TValue, TResult>(this Result<TValue> result, Func<TValue, Task<Result<TResult>>> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error).AsCompletedTask();

        return func(result.Value);
    }

    public static async Task<Result<TResult>> BindAsync<TValue, TResult>(this Task<Result<TValue>> resultTask, Func<TValue, Task<Result<TResult>>> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.BindAsync(func).ConfigureAwait(false);
    }

    public static async Task<Result<TResult>> BindAsync<TValue, TResult>(this Task<Result<TValue>> resultTask, Func<TValue, Result<TResult>> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.Bind(func);
    }

    public static async ValueTask<Result<TResult>> BindAsync<TValue, TResult>(this ValueTask<Result<TValue>> resultTask, Func<TValue, ValueTask<Result<TResult>>> valueTask)
    {
        Result<TValue> result = await resultTask;
        return await result.BindAsync(valueTask);
    }

    public static async ValueTask<Result<TResult>> BindAsync<TValue, TResult>(this ValueTask<Result<TValue>> resultTask, Func<TValue, Result<TResult>> func)
    {
        Result<TValue> result = await resultTask;
        return result.Bind(func);
    }

    public static ValueTask<Result<TResult>> BindAsync<TValue, TResult>(this Result<TValue> result, Func<TValue, ValueTask<Result<TResult>>> valueTask)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error).AsCompletedValueTask();

        return valueTask(result.Value);
    }

    public static async Task<Result<TResult>> BindAsync<T1, T2, TResult>(this Task<Result<(T1, T2)>> resultTask, Func<T1, T2, Result<TResult>> func)
    {
        var result = await resultTask;
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error);

        var (args1, args2) = result.Value;
        return func(args1, args2);
    }

    public static async Task<Result<TResult>> BindAsync<T1, T2, TResult>(this Result<(T1, T2)> result, Func<T1, T2, Task<Result<TResult>>> func)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error);

        var (args1, args2) = result.Value;
        return await func(args1, args2);
    }

    public static async Task<Result<TResult>> BindAsync<T1, T2, TResult>(this Task<Result<(T1, T2)>> resultTask, Func<T1, T2, Task<Result<TResult>>> func)
    {
        var result = await resultTask;
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error);

        var (args1, args2) = result.Value;
        return await func(args1, args2);
    }
}
