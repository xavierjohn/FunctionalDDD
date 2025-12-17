namespace FunctionalDdd;

using System.Diagnostics;

/// <summary>
/// Executes the given action if the calling <see cref="Result{TValue}"/> is a failure. Returns the calling <see cref="Result{TValue}"/>.
/// </summary>
public static partial class TapErrorExtensions
{
    public static Result<TValue> TapError<TValue>(this Result<TValue> result, Action action)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure)
        {
            action();
            activity?.SetStatus(ActivityStatusCode.Error);
        }

        return result;
    }

    public static Result<TValue> TapError<TValue>(this Result<TValue> result, Action<Error> action)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure)
        {
            action(result.Error);
            activity?.SetStatus(ActivityStatusCode.Error);
        }

        return result;
    }
}

/// <summary>
/// Executes the given action if the calling <see cref="Result{TValue}"/> is a failure. Returns the calling <see cref="Result{TValue}"/>.
/// </summary>
public static partial class TapErrorExtensionsAsync
{
    public static async Task<Result<TValue>> TapErrorAsync<TValue>(this Task<Result<TValue>> resultTask, Action<Error> action)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.TapError(action);
    }

    public static async Task<Result<TValue>> TapErrorAsync<TValue>(this Task<Result<TValue>> resultTask, Action action)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.TapError(action);
    }

    public static async Task<Result<TValue>> TapErrorAsync<TValue>(this Result<TValue> result, Func<Task> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure)
        {
            await func().ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Error);
        }

        return result;
    }

    public static async Task<Result<TValue>> TapErrorAsync<TValue>(this Result<TValue> result, Func<Error, Task> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure)
        {
            await func(result.Error).ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Error);
        }

        return result;
    }

    public static async Task<Result<TValue>> TapErrorAsync<TValue>(
        this Result<TValue> result,
        Func<CancellationToken, Task> func,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure)
        {
            await func(cancellationToken).ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Error);
        }

        return result;
    }

    public static async Task<Result<TValue>> TapErrorAsync<TValue>(
        this Result<TValue> result,
        Func<Error, CancellationToken, Task> func,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure)
        {
            await func(result.Error, cancellationToken).ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Error);
        }

        return result;
    }

    public static async Task<Result<TValue>> TapErrorAsync<TValue>(this Task<Result<TValue>> resultTask, Func<Task> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapErrorAsync(func).ConfigureAwait(false);
    }

    public static async Task<Result<TValue>> TapErrorAsync<TValue>(this Task<Result<TValue>> resultTask, Func<Error, Task> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapErrorAsync(func).ConfigureAwait(false);
    }

    public static async Task<Result<TValue>> TapErrorAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        Func<CancellationToken, Task> func,
        CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapErrorAsync(func, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<Result<TValue>> TapErrorAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        Func<Error, CancellationToken, Task> func,
        CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapErrorAsync(func, cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<Result<TValue>> TapErrorAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Action action)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.TapError(action);
    }

    public static async ValueTask<Result<TValue>> TapErrorAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Action<Error> action)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.TapError(action);
    }

    public static async ValueTask<Result<TValue>> TapErrorAsync<TValue>(this Result<TValue> result, Func<ValueTask> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure)
        {
            await func().ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Error);
        }

        return result;
    }

    public static async ValueTask<Result<TValue>> TapErrorAsync<TValue>(this Result<TValue> result, Func<Error, ValueTask> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure)
        {
            await func(result.Error).ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Error);
        }

        return result;
    }

    public static async ValueTask<Result<TValue>> TapErrorAsync<TValue>(
        this Result<TValue> result,
        Func<CancellationToken, ValueTask> func,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure)
        {
            await func(cancellationToken).ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Error);
        }

        return result;
    }

    public static async ValueTask<Result<TValue>> TapErrorAsync<TValue>(
        this Result<TValue> result,
        Func<Error, CancellationToken, ValueTask> func,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure)
        {
            await func(result.Error, cancellationToken).ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Error);
        }

        return result;
    }

    public static async ValueTask<Result<TValue>> TapErrorAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Func<ValueTask> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapErrorAsync(func).ConfigureAwait(false);
    }

    public static async ValueTask<Result<TValue>> TapErrorAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Func<Error, ValueTask> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapErrorAsync(func).ConfigureAwait(false);
    }

    public static async ValueTask<Result<TValue>> TapErrorAsync<TValue>(
        this ValueTask<Result<TValue>> resultTask,
        Func<CancellationToken, ValueTask> func,
        CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapErrorAsync(func, cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<Result<TValue>> TapErrorAsync<TValue>(
        this ValueTask<Result<TValue>> resultTask,
        Func<Error, CancellationToken, ValueTask> func,
        CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapErrorAsync(func, cancellationToken).ConfigureAwait(false);
    }
}
