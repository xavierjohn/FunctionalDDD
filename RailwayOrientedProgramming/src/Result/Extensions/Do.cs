namespace FunctionalDdd;

using System.Diagnostics;

/// <summary>
/// Executes side effects on values without wrapping in Result, useful for pass-through scenarios.
/// Unlike Railway Oriented Programming operations (Tap, Bind, etc.) which work on Result types,
/// Do operates on plain values for general-purpose side effects and value pass-through.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Important:</strong> Do is NOT part of the Railway Oriented Programming pattern.
/// It operates on plain values (not Result&lt;T&gt;) and always executes regardless of any error state.
/// </para>
/// <para>
/// Use <see cref="TapExtensions.Tap{TValue}(Result{TValue}, Action{TValue})"/> when you need 
/// railway-aware side effects that only execute on success.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // General-purpose value pass-through with side effects
/// var processedValue = await GetRawDataAsync()
///     .DoAsync(async data => await LogDataReceivedAsync(data))
///     .DoAsync(data => ValidateDataStructure(data));
/// 
/// // Logging without Result wrapper
/// var userId = "user123"
///     .Do(id => _logger.LogInformation($"Processing user {id}"))
///     .Do(id => _metrics.IncrementCounter("user_requests"));
/// 
/// // Compare with Railway Oriented Programming:
/// // Use Tap when working with Result&lt;T&gt;
/// var result = await GetUserAsync(userId)
///     .TapAsync(async user => await LogUserAccessAsync(user)); // Only logs on success
/// 
/// // Use Do when working with plain values
/// var processedId = userId
///     .DoAsync(async id => await LogAccessAttemptAsync(id)); // Always logs
/// </code>
/// </example>
public static class DoExtensions
{
    /// <summary>
    /// Executes a side effect action on the value and returns the value unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="value">The value to process.</param>
    /// <param name="action">Side effect action to execute.</param>
    /// <returns>The original value unchanged.</returns>
    public static T Do<T>(this T value, Action<T> action)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        action(value);
        return value;
    }

    /// <summary>
    /// Executes an asynchronous side effect action on the value and returns the value unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="value">The value to process.</param>
    /// <param name="action">Async side effect action to execute.</param>
    /// <returns>Task producing the original value unchanged.</returns>
    public static async Task<T> DoAsync<T>(this T value, Func<T, Task> action)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        await action(value).ConfigureAwait(false);
        return value;
    }

    /// <summary>
    /// Executes an asynchronous side effect action on the value and returns the value unchanged.
    /// Supports cancellation.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="value">The value to process.</param>
    /// <param name="action">Async side effect action with cancellation support.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>Task producing the original value unchanged.</returns>
    public static async Task<T> DoAsync<T>(
        this T value,
        Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        await action(value, cancellationToken).ConfigureAwait(false);
        return value;
    }

    /// <summary>
    /// Awaits the task value, executes a synchronous side effect, and returns the value unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="valueTask">Task producing the value.</param>
    /// <param name="action">Side effect action to execute.</param>
    /// <returns>Task producing the original value unchanged.</returns>
    public static async Task<T> DoAsync<T>(this Task<T> valueTask, Action<T> action)
    {
        T value = await valueTask.ConfigureAwait(false);
        return value.Do(action);
    }

    /// <summary>
    /// Awaits the task value, executes an asynchronous side effect, and returns the value unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="valueTask">Task producing the value.</param>
    /// <param name="action">Async side effect action to execute.</param>
    /// <returns>Task producing the original value unchanged.</returns>
    public static async Task<T> DoAsync<T>(this Task<T> valueTask, Func<T, Task> action)
    {
        T value = await valueTask.ConfigureAwait(false);
        return await value.DoAsync(action).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the task value, executes an asynchronous side effect with cancellation, and returns the value unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="valueTask">Task producing the value.</param>
    /// <param name="action">Async side effect action with cancellation support.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>Task producing the original value unchanged.</returns>
    public static async Task<T> DoAsync<T>(
        this Task<T> valueTask,
        Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        T value = await valueTask.ConfigureAwait(false);
        return await value.DoAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the ValueTask, executes a synchronous side effect, and returns the value unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="valueTask">ValueTask producing the value.</param>
    /// <param name="action">Side effect action to execute.</param>
    /// <returns>ValueTask producing the original value unchanged.</returns>
    public static async ValueTask<T> DoAsync<T>(this ValueTask<T> valueTask, Action<T> action)
    {
        T value = await valueTask.ConfigureAwait(false);
        return value.Do(action);
    }

    /// <summary>
    /// Awaits the ValueTask, executes an asynchronous side effect, and returns the value unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="valueTask">ValueTask producing the value.</param>
    /// <param name="action">Async side effect action to execute.</param>
    /// <returns>ValueTask producing the original value unchanged.</returns>
    public static async ValueTask<T> DoAsync<T>(this ValueTask<T> valueTask, Func<T, ValueTask> action)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        T value = await valueTask.ConfigureAwait(false);
        await action(value).ConfigureAwait(false);
        return value;
    }

    /// <summary>
    /// Awaits the ValueTask, executes an asynchronous side effect with cancellation, and returns the value unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="valueTask">ValueTask producing the value.</param>
    /// <param name="action">Async side effect action with cancellation support.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>ValueTask producing the original value unchanged.</returns>
    public static async ValueTask<T> DoAsync<T>(
        this ValueTask<T> valueTask,
        Func<T, CancellationToken, ValueTask> action,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        T value = await valueTask.ConfigureAwait(false);
        await action(value, cancellationToken).ConfigureAwait(false);
        return value;
    }

    /// <summary>
    /// Executes an asynchronous side effect on the value using ValueTask and returns the value unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="value">The value to process.</param>
    /// <param name="action">Async side effect action to execute.</param>
    /// <returns>ValueTask producing the original value unchanged.</returns>
    public static async ValueTask<T> DoAsync<T>(this T value, Func<T, ValueTask> action)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        await action(value).ConfigureAwait(false);
        return value;
    }

    /// <summary>
    /// Executes an asynchronous side effect on the value using ValueTask with cancellation and returns the value unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="value">The value to process.</param>
    /// <param name="action">Async side effect action with cancellation support.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>ValueTask producing the original value unchanged.</returns>
    public static async ValueTask<T> DoAsync<T>(
        this T value,
        Func<T, CancellationToken, ValueTask> action,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        await action(value, cancellationToken).ConfigureAwait(false);
        return value;
    }
}
