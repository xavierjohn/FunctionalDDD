namespace FunctionalDdd;

using System;
using System.Diagnostics;

/// <summary>
/// Executes the given action if the starting result is a success. Returns the starting result.
/// It is useful to execute functions that don't have a return type or return type can be ignored.
/// </summary>
public static partial class TapExtensions
{
    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static Result<TValue> Tap<TValue>(this Result<TValue> result, Action action)
    {
        if (result.IsSuccess)
            action();

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static Result<TValue> Tap<TValue>(this Result<TValue> result, Action<TValue> action)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsSuccess)
        {
            action(result.Value);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }

        return result;
    }
}

/// <summary>
/// Executes the given action if the starting result is a success. Returns the starting result.
/// It is useful to execute functions that don't have a return type or return type can be ignored.
/// </summary>
public static partial class TapExtensionsAsync
{
    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> resultTask, Action action)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.Tap(action);
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> resultTask, Action<TValue> action)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.Tap(action);
    }

    /// <summary>
    /// Executes the given action if the calling result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<Task> func)
    {
        if (result.IsSuccess)
            await func().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the calling result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<CancellationToken, Task> func, CancellationToken cancellationToken = default)
    {
        if (result.IsSuccess)
            await func(cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> resultTask, Func<Task> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);

        if (result.IsSuccess)
            await func().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> resultTask, Func<CancellationToken, Task> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);

        if (result.IsSuccess)
            await func(cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> resultTask, Func<TValue, Task> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(TapExtensions.Tap));
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await func(result.Value).ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> resultTask, Func<TValue, CancellationToken, Task> func, CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(TapExtensions.Tap));
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await func(result.Value, cancellationToken).ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<TValue, Task> func)
    {
        if (result.IsSuccess)
            await func(result.Value).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<TValue, CancellationToken, Task> func, CancellationToken cancellationToken = default)
    {
        if (result.IsSuccess)
            await func(result.Value, cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<CancellationToken, ValueTask> func, CancellationToken cancellationToken = default)
    {
        if (result.IsSuccess)
            await func(cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<ValueTask> func)
    {
        if (result.IsSuccess)
            await func().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<TValue, CancellationToken, ValueTask> func, CancellationToken cancellationToken = default)
    {
        if (result.IsSuccess)
            await func(result.Value, cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<TValue, ValueTask> func)
    {
        if (result.IsSuccess)
            await func(result.Value).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Func<CancellationToken, ValueTask> valueTask, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);

        if (result.IsSuccess)
            await valueTask(cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Func<ValueTask> valueTask)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);

        if (result.IsSuccess)
            await valueTask().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Func<TValue, CancellationToken, ValueTask> valueTask, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);

        if (result.IsSuccess)
            await valueTask(result.Value, cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Func<TValue, ValueTask> valueTask)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);

        if (result.IsSuccess)
            await valueTask(result.Value).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Action action)
    {
        Result<TValue> result = await resultTask;
        return result.Tap(action);
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Action<TValue> action)
    {
        Result<TValue> result = await resultTask;
        return result.Tap(action);
    }
}
