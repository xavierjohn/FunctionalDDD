namespace FunctionalDdd;

using System;
using System.Diagnostics;

/// <summary>
/// Provides extension methods for executing side effects on successful Results without changing the Result.
/// </summary>
/// <remarks>
/// Tap is useful for performing side effects (like logging, auditing, or debugging) in a functional pipeline
/// without breaking the chain. The action is only executed if the Result is successful, and the original
/// Result is always returned unchanged. This allows you to "peek" at values flowing through the pipeline.
/// </remarks>
/// <example>
/// <code>
/// var result = GetUser(id)
///     .Tap(user => _logger.LogInformation("Found user: {Name}", user.Name))
///     .Bind(user => ValidateUser(user))
///     .Tap(() => _metrics.IncrementCounter("user.validated"));
/// </code>
/// </example>
public static partial class TapExtensions
{
    /// <summary>
    /// Executes the given action if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="action">The action to execute if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    public static Result<TValue> Tap<TValue>(this Result<TValue> result, Action action)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsSuccess)
            action();

        result.LogActivityStatus();
        return result;
    }

    /// <summary>
    /// Executes the given action with the value if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="action">The action to execute with the value if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    public static Result<TValue> Tap<TValue>(this Result<TValue> result, Action<TValue> action)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsSuccess)
            action(result.Value);

        result.LogActivityStatus();
        return result;
    }
}

/// <summary>
/// Provides asynchronous extension methods for executing side effects on successful Results without changing the Result.
/// </summary>
public static partial class TapExtensionsAsync
{
    // Task<Result<TValue>> overloads with Action

    /// <summary>
    /// Asynchronously executes the given action if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The task containing the result to tap.</param>
    /// <param name="action">The action to execute if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> resultTask, Action action)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.Tap(action);
    }

    /// <summary>
    /// Asynchronously executes the given action with the value if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The task containing the result to tap.</param>
    /// <param name="action">The action to execute with the value if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> resultTask, Action<TValue> action)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.Tap(action);
    }

    // Result<TValue> overloads with Func<Task>

    /// <summary>
    /// Asynchronously executes the given async action if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="func">The async action to execute if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<Task> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(TapExtensions.Tap));
        if (result.IsSuccess)
            await func().ConfigureAwait(false);

        result.LogActivityStatus();
        return result;
    }

    /// <summary>
    /// Asynchronously executes the given async action if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="func">The async action to execute if the result is successful.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result unchanged.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<CancellationToken, Task> func, CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(TapExtensions.Tap));
        if (result.IsSuccess)
            await func(cancellationToken).ConfigureAwait(false);

        result.LogActivityStatus();
        return result;
    }

    // Task<Result<TValue>> overloads with Func<Task>

    /// <summary>
    /// Asynchronously executes the given async action if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The task containing the result to tap.</param>
    /// <param name="func">The async action to execute if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> resultTask, Func<Task> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapAsync(func).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously executes the given async action if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The task containing the result to tap.</param>
    /// <param name="func">The async action to execute if the result is successful.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result unchanged.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> resultTask, Func<CancellationToken, Task> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapAsync(func, cancellationToken).ConfigureAwait(false);
    }

    // Task<Result<TValue>> overloads with Func<TValue, Task>

    /// <summary>
    /// Asynchronously executes the given async action with the value if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The task containing the result to tap.</param>
    /// <param name="func">The async action to execute with the value if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> resultTask, Func<TValue, Task> func)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapAsync(func).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously executes the given async action with the value if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The task containing the result to tap.</param>
    /// <param name="func">The async action to execute with the value if the result is successful.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result unchanged.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> resultTask, Func<TValue, CancellationToken, Task> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapAsync(func, cancellationToken).ConfigureAwait(false);
    }

    // Result<TValue> overloads with Func<TValue, Task>

    /// <summary>
    /// Asynchronously executes the given async action with the value if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="func">The async action to execute with the value if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<TValue, Task> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(TapExtensions.Tap));
        if (result.IsSuccess)
            await func(result.Value).ConfigureAwait(false);

        result.LogActivityStatus();
        return result;
    }

    /// <summary>
    /// Asynchronously executes the given async action with the value if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="func">The async action to execute with the value if the result is successful.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result unchanged.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<TValue, CancellationToken, Task> func, CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(TapExtensions.Tap));
        if (result.IsSuccess)
            await func(result.Value, cancellationToken).ConfigureAwait(false);

        result.LogActivityStatus();
        return result;
    }

    // Result<TValue> overloads with Func<ValueTask> - THESE WERE MISSING!

    /// <summary>
    /// Asynchronously executes the given async action if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="func">The async action to execute if the result is successful.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result unchanged.</returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<CancellationToken, ValueTask> func, CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(TapExtensions.Tap));
        if (result.IsSuccess)
            await func(cancellationToken).ConfigureAwait(false);

        result.LogActivityStatus();
        return result;
    }

    /// <summary>
    /// Asynchronously executes the given async action if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="func">The async action to execute if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<ValueTask> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(TapExtensions.Tap));
        if (result.IsSuccess)
            await func().ConfigureAwait(false);
        
        result.LogActivityStatus();
        return result;
    }

    /// <summary>
    /// Asynchronously executes the given async action with the value if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="func">The async action to execute with the value if the result is successful.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result unchanged.</returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<TValue, CancellationToken, ValueTask> func, CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(TapExtensions.Tap));
        if (result.IsSuccess)
            await func(result.Value, cancellationToken).ConfigureAwait(false);

        result.LogActivityStatus();
        return result;
    }

    /// <summary>
    /// Asynchronously executes the given async action with the value if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="func">The async action to execute with the value if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<TValue, ValueTask> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(TapExtensions.Tap));
        if (result.IsSuccess)
            await func(result.Value).ConfigureAwait(false);

        result.LogActivityStatus();
        return result;
    }

    // ValueTask<Result<TValue>> overloads with Func<ValueTask>

    /// <summary>
    /// Asynchronously executes the given async action if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The ValueTask containing the result to tap.</param>
    /// <param name="valueTask">The async action to execute if the result is successful.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result unchanged.</returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Func<CancellationToken, ValueTask> valueTask, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapAsync(valueTask, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously executes the given async action if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The ValueTask containing the result to tap.</param>
    /// <param name="valueTask">The async action to execute if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Func<ValueTask> valueTask)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapAsync(valueTask).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously executes the given async action with the value if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The ValueTask containing the result to tap.</param>
    /// <param name="valueTask">The async action to execute with the value if the result is successful.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result unchanged.</returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Func<TValue, CancellationToken, ValueTask> valueTask, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapAsync(valueTask, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously executes the given async action with the value if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The ValueTask containing the result to tap.</param>
    /// <param name="valueTask">The async action to execute with the value if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Func<TValue, ValueTask> valueTask)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return await result.TapAsync(valueTask).ConfigureAwait(false);
    }

    // ValueTask<Result<TValue>> overloads with Action

    /// <summary>
    /// Asynchronously executes the given action if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The ValueTask containing the result to tap.</param>
    /// <param name="action">The action to execute if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Action action)
    {
        Result<TValue> result = await resultTask;
        return result.Tap(action);
    }

    /// <summary>
    /// Asynchronously executes the given action with the value if the result is a success. Returns the original result unchanged.
    /// </summary>
    /// <typeparam name="TValue">Type of the result value.</typeparam>
    /// <param name="resultTask">The ValueTask containing the result to tap.</param>
    /// <param name="action">The action to execute with the value if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> resultTask, Action<TValue> action)
    {
        Result<TValue> result = await resultTask;
        return result.Tap(action);
    }
}
