namespace FunctionalDdd;

/// <summary>
/// If the starting Result is a success, the Bind function will return a new Result from the given function.
/// Otherwise, the Bind function will return the starting failed Result.
/// </summary>
public static partial class BindExtensions
{
    public static Result<TResult> Bind<TValue, TResult>(this Result<TValue> result, Func<TValue, Result<TResult>> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
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
    public static async Task<Result<TResult>> BindAsync<TValue, TResult>(this Result<TValue> result, Func<TValue, CancellationToken, Task<Result<TResult>>> func, CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(BindExtensions.Bind));
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error);

        return await func(result.Value, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<Result<TResult>> BindAsync<TValue, TResult>(this Result<TValue> result, Func<TValue, Task<Result<TResult>>> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(BindExtensions.Bind));
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error);

        return await func(result.Value).ConfigureAwait(false);
    }

    public static async Task<Result<TResult>> BindAsync<TValue, TResult>(this Task<Result<TValue>> resultTask, Func<TValue, CancellationToken, Task<Result<TResult>>> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.BindAsync(func, cancellationToken).ConfigureAwait(false);
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

    public static async ValueTask<Result<TResult>> BindAsync<TValue, TResult>(this ValueTask<Result<TValue>> resultTask, Func<TValue, CancellationToken, ValueTask<Result<TResult>>> valueTask, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.BindAsync(valueTask, cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<Result<TResult>> BindAsync<TValue, TResult>(this ValueTask<Result<TValue>> resultTask, Func<TValue, ValueTask<Result<TResult>>> valueTask)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.BindAsync(valueTask).ConfigureAwait(false);
    }

    public static async ValueTask<Result<TResult>> BindAsync<TValue, TResult>(this ValueTask<Result<TValue>> resultTask, Func<TValue, Result<TResult>> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.Bind(func);
    }

    public static ValueTask<Result<TResult>> BindAsync<TValue, TResult>(this Result<TValue> result, Func<TValue, CancellationToken, ValueTask<Result<TResult>>> valueTask, CancellationToken cancellationToken = default)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error).AsCompletedValueTask();

        return valueTask(result.Value, cancellationToken);
    }

    public static ValueTask<Result<TResult>> BindAsync<TValue, TResult>(this Result<TValue> result, Func<TValue, ValueTask<Result<TResult>>> valueTask)
    {
        if (result.IsFailure)
            return Result.Failure<TResult>(result.Error).AsCompletedValueTask();

        return valueTask(result.Value);
    }
}
