namespace FunctionalDdd;

using System.Diagnostics;

/// <summary>
/// Provides extension methods for executing side effects on failed Results without changing the Result.
/// </summary>
/// <remarks>
/// TapError is the counterpart to <see cref="TapExtensions"/>. It allows you to perform side effects
/// (like logging, metrics, or debugging) when a Result is in a failed state, without altering the Result itself.
/// The action is only executed if the Result is a failure, and the original Result is always returned unchanged.
/// </remarks>
/// <example>
/// <code>
/// var result = GetUser(id)
///     .TapError(error => _logger.LogError("Failed to get user: {Error}", error.Detail))
///     .TapError(() => _metrics.IncrementCounter("user.get.failed"));
/// </code>
/// </example>
public static partial class TapErrorExtensions
{
    /// <summary>
    /// Executes the given action if the result is a failure. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="action">The action to execute if the result is a failure.</param>
    /// <returns>The original result unchanged.</returns>
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

    /// <summary>
    /// Executes the given action with the error if the result is a failure. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="action">The action to execute with the error if the result is a failure.</param>
    /// <returns>The original result unchanged.</returns>
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
/// Provides asynchronous extension methods for executing side effects on failed Results without changing the Result.
/// </summary>
/// <remarks>
/// These methods enable async error handling patterns such as logging to external services,
/// sending notifications, or recording metrics when operations fail.
/// </remarks>
public static partial class TapErrorExtensionsAsync
{
    /// <summary>
    /// Asynchronously executes the given action with the error if the result is a failure. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The task containing the result to tap.</param>
    /// <param name="action">The action to execute with the error if the result is a failure.</param>
    /// <returns>The original result unchanged.</returns>
    public static async Task<Result<TValue>> TapErrorAsync<TValue>(this Task<Result<TValue>> resultTask, Action<Error> action)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.TapError(action);
    }

    /// <summary>
    /// Asynchronously executes the given action if the result is a failure. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The task containing the result to tap.</param>
    /// <param name="action">The action to execute if the result is a failure.</param>
    /// <returns>The original result unchanged.</returns>
    public static async Task<Result<TValue>> TapErrorAsync<TValue>(this Task<Result<TValue>> resultTask, Action action)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.TapError(action);
    }

    /// <summary>
    /// Asynchronously executes the given async action if the result is a failure. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="func">The async action to execute if the result is a failure.</param>
    /// <returns>The original result unchanged.</returns>
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

    /// <summary>
    /// Asynchronously executes the given async action with the error if the result is a failure. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="func">The async action to execute with the error if the result is a failure.</param>
    /// <returns>The original result unchanged.</returns>
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

    /// <summary>
    /// Asynchronously executes the given async action if the result is a failure. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The task containing the result to tap.</param>
    /// <param name="func">The async action to execute if the result is a failure.</param>
    /// <returns>The original result unchanged.</returns>
    public static async Task<Result<TValue>> TapErrorAsync<TValue>(this Task<Result<TValue>> resultTask, Func<Task> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapErrorAsync(func).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously executes the given async action with the error if the result is a failure. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The task containing the result to tap.</param>
    /// <param name="func">The async action to execute with the error if the result is a failure.</param>
    /// <returns>The original result unchanged.</returns>
    public static async Task<Result<TValue>> TapErrorAsync<TValue>(this Task<Result<TValue>> resultTask, Func<Error, Task> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapErrorAsync(func).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously executes the given action if the result is a failure. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The ValueTask containing the result to tap.</param>
    /// <param name="action">The action to execute if the result is a failure.</param>
    /// <returns>The original result unchanged.</returns>
    public static async ValueTask<Result<TValue>> TapErrorAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Action action)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.TapError(action);
    }

    /// <summary>
    /// Asynchronously executes the given action with the error if the result is a failure. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The ValueTask containing the result to tap.</param>
    /// <param name="action">The action to execute with the error if the result is a failure.</param>
    /// <returns>The original result unchanged.</returns>
    public static async ValueTask<Result<TValue>> TapErrorAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Action<Error> action)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.TapError(action);
    }

    /// <summary>
    /// Asynchronously executes the given async action if the result is a failure. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="func">The async action to execute if the result is a failure.</param>
    /// <returns>The original result unchanged.</returns>
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

    /// <summary>
    /// Asynchronously executes the given async action with the error if the result is a failure. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="func">The async action to execute with the error if the result is a failure.</param>
    /// <returns>The original result unchanged.</returns>
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

    /// <summary>
    /// Asynchronously executes the given async action if the result is a failure. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The ValueTask containing the result to tap.</param>
    /// <param name="func">The async action to execute if the result is a failure.</param>
    /// <returns>The original result unchanged.</returns>
    public static async ValueTask<Result<TValue>> TapErrorAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Func<ValueTask> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapErrorAsync(func).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously executes the given async action with the error if the result is a failure. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The ValueTask containing the result to tap.</param>
    /// <param name="func">The async action to execute with the error if the result is a failure.</param>
    /// <returns>The original result unchanged.</returns>
    public static async ValueTask<Result<TValue>> TapErrorAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Func<Error, ValueTask> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapErrorAsync(func).ConfigureAwait(false);
    }
}
