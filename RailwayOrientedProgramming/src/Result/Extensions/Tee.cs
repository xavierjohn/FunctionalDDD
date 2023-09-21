﻿namespace FunctionalDDD.Results;

/// <summary>
/// Executes the given action if the starting result is a success. Returns the starting result.
/// It is useful to execute functions that don't have a return type or return type can be ignored.
/// </summary>
public static class TeeExtensions
{
    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static Result<TValue> Tee<TValue>(this Result<TValue> result, Action action)
    {
        if (result.IsSuccess)
            action();

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static Result<TValue> Tee<TValue>(this Result<TValue> result, Action<TValue> action)
    {
        if (result.IsSuccess)
            action(result.Value);

        return result;
    }
}

/// <summary>
/// Executes the given action if the starting result is a success. Returns the starting result.
/// It is useful to execute functions that don't have a return type or return type can be ignored.
/// </summary>
public static class TeeExtensionsAsync
{
    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TValue>> TeeAsync<TValue>(this Task<Result<TValue>> resultTask, Action action)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.Tee(action);
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TValue>> TeeAsync<TValue>(this Task<Result<TValue>> resultTask, Action<TValue> action)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.Tee(action);
    }

    /// <summary>
    /// Executes the given action if the calling result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TValue>> TeeAsync<TValue>(this Result<TValue> result, Func<Task> func)
    {
        if (result.IsSuccess)
            await func().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TValue>> TeeAsync<TValue>(this Task<Result<TValue>> resultTask, Func<Task> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);

        if (result.IsSuccess)
            await func().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TValue>> TeeAsync<TValue>(this Task<Result<TValue>> resultTask, Func<TValue, Task> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess)
            await func(result.Value).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TValue>> TeeAsync<TValue>(this Result<TValue> result, Func<TValue, Task> func)
    {
        if (result.IsSuccess)
            await func(result.Value).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TValue>> TeeAsync<TValue>(this Result<TValue> result, Func<ValueTask> func)
    {
        if (result.IsSuccess)
            await func().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TValue>> TeeAsync<TValue>(this Result<TValue> result, Func<TValue, ValueTask> func)
    {
        if (result.IsSuccess)
            await func(result.Value).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TValue>> TeeAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Func<ValueTask> valueTask)
    {
        Result<TValue> result = await resultTask;

        if (result.IsSuccess)
            await valueTask();

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TValue>> TeeAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Func<TValue, ValueTask> valueTask)
    {
        Result<TValue> result = await resultTask;

        if (result.IsSuccess)
            await valueTask(result.Value);

        return result;
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TValue>> TeeAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Action action)
    {
        Result<TValue> result = await resultTask;
        return result.Tee(action);
    }

    /// <summary>
    /// Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TValue>> TeeAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Action<TValue> action)
    {
        Result<TValue> result = await resultTask;
        return result.Tee(action);
    }
}
