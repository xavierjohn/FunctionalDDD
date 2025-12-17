namespace FunctionalDdd;

using System.Diagnostics;

/// <summary>
/// Provides retry logic with exponential backoff for handling transient failures.
/// Useful for operations that may fail temporarily due to network issues, rate limiting, or resource unavailability.
/// </summary>
/// <example>
/// <code>
/// // Retry with default settings (3 attempts, 100ms initial delay, 2x backoff)
/// var result = await RetryExtensions.RetryAsync(
///     async () => await FetchDataFromApiAsync()
/// );
/// 
/// // Retry with custom settings and cancellation
/// var result = await RetryExtensions.RetryAsync(
///     async ct => await FetchDataAsync(ct),
///     maxRetries: 5,
///     initialDelay: TimeSpan.FromMilliseconds(500),
///     backoffMultiplier: 1.5,
///     shouldRetry: error => error is not ValidationError, // Don't retry validation errors
///     cancellationToken: cancellationToken
/// );
/// 
/// // Retry only specific error types
/// var result = await RetryExtensions.RetryAsync(
///     async () => await ProcessOrderAsync(order),
///     shouldRetry: error => error.Code.Contains("timeout") || error.Code.Contains("503")
/// );
/// </code>
/// </example>
public static class RetryExtensions
{
    /// <summary>
    /// Retries an operation with exponential backoff on failure.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="operation">Operation to retry.</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
    /// <param name="initialDelay">Initial delay between retries (default: 100ms).</param>
    /// <param name="backoffMultiplier">Multiplier for exponential backoff (default: 2.0).</param>
    /// <param name="shouldRetry">Optional predicate to determine if an error should trigger a retry.</param>
    /// <returns>Task producing the result of the operation, or the final failure after all retries.</returns>
    public static async Task<Result<T>> RetryAsync<T>(
        this Func<Task<Result<T>>> operation,
        int maxRetries = 3,
        TimeSpan? initialDelay = null,
        double backoffMultiplier = 2.0,
        Func<Error, bool>? shouldRetry = null)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        var delay = initialDelay ?? TimeSpan.FromMilliseconds(100);
        
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            var result = await operation().ConfigureAwait(false);
            
            if (result.IsSuccess)
            {
                return result;
            }
            
            if (attempt == maxRetries)
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                return result;
            }
            
            if (shouldRetry != null && !shouldRetry(result.Error))
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                return result;
            }
            
            await Task.Delay(delay).ConfigureAwait(false);
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * backoffMultiplier);
        }
        
        activity?.SetStatus(ActivityStatusCode.Error);
        return Result.Failure<T>(Error.Unexpected("Maximum retry attempts exceeded", "RetryExhausted"));
    }

    /// <summary>
    /// Retries an operation with exponential backoff on failure. Supports cancellation.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="operation">Operation to retry with cancellation support.</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
    /// <param name="initialDelay">Initial delay between retries (default: 100ms).</param>
    /// <param name="backoffMultiplier">Multiplier for exponential backoff (default: 2.0).</param>
    /// <param name="shouldRetry">Optional predicate to determine if an error should trigger a retry.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>Task producing the result of the operation, or the final failure after all retries.</returns>
    public static async Task<Result<T>> RetryAsync<T>(
        this Func<CancellationToken, Task<Result<T>>> operation,
        int maxRetries = 3,
        TimeSpan? initialDelay = null,
        double backoffMultiplier = 2.0,
        Func<Error, bool>? shouldRetry = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        var delay = initialDelay ?? TimeSpan.FromMilliseconds(100);
        
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var result = await operation(cancellationToken).ConfigureAwait(false);
            
            if (result.IsSuccess)
            {
                return result;
            }
            
            if (attempt == maxRetries)
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                return result;
            }
            
            if (shouldRetry != null && !shouldRetry(result.Error))
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                return result;
            }
            
            try
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                return result;
            }
            
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * backoffMultiplier);
        }
        
        activity?.SetStatus(ActivityStatusCode.Error);
        return Result.Failure<T>(Error.Unexpected("Maximum retry attempts exceeded", "RetryExhausted"));
    }

    /// <summary>
    /// Retries an operation with exponential backoff on failure using ValueTask for zero-allocation scenarios.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="operation">Operation to retry.</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
    /// <param name="initialDelay">Initial delay between retries (default: 100ms).</param>
    /// <param name="backoffMultiplier">Multiplier for exponential backoff (default: 2.0).</param>
    /// <param name="shouldRetry">Optional predicate to determine if an error should trigger a retry.</param>
    /// <returns>ValueTask producing the result of the operation, or the final failure after all retries.</returns>
    public static async ValueTask<Result<T>> RetryAsync<T>(
        this Func<ValueTask<Result<T>>> operation,
        int maxRetries = 3,
        TimeSpan? initialDelay = null,
        double backoffMultiplier = 2.0,
        Func<Error, bool>? shouldRetry = null)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        var delay = initialDelay ?? TimeSpan.FromMilliseconds(100);
        
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            var result = await operation().ConfigureAwait(false);
            
            if (result.IsSuccess)
            {
                return result;
            }
            
            if (attempt == maxRetries)
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                return result;
            }
            
            if (shouldRetry != null && !shouldRetry(result.Error))
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                return result;
            }
            
            await Task.Delay(delay).ConfigureAwait(false);
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * backoffMultiplier);
        }
        
        activity?.SetStatus(ActivityStatusCode.Error);
        return Result.Failure<T>(Error.Unexpected("Maximum retry attempts exceeded", "RetryExhausted"));
    }

    /// <summary>
    /// Retries an operation with exponential backoff on failure using ValueTask with cancellation support.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="operation">Operation to retry with cancellation support.</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
    /// <param name="initialDelay">Initial delay between retries (default: 100ms).</param>
    /// <param name="backoffMultiplier">Multiplier for exponential backoff (default: 2.0).</param>
    /// <param name="shouldRetry">Optional predicate to determine if an error should trigger a retry.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>ValueTask producing the result of the operation, or the final failure after all retries.</returns>
    public static async ValueTask<Result<T>> RetryAsync<T>(
        this Func<CancellationToken, ValueTask<Result<T>>> operation,
        int maxRetries = 3,
        TimeSpan? initialDelay = null,
        double backoffMultiplier = 2.0,
        Func<Error, bool>? shouldRetry = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        var delay = initialDelay ?? TimeSpan.FromMilliseconds(100);
        
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var result = await operation(cancellationToken).ConfigureAwait(false);
            
            if (result.IsSuccess)
            {
                return result;
            }
            
            if (attempt == maxRetries)
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                return result;
            }
            
            if (shouldRetry != null && !shouldRetry(result.Error))
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                return result;
            }
            
            try
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                return result;
            }
            
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * backoffMultiplier);
        }
        
        activity?.SetStatus(ActivityStatusCode.Error);
        return Result.Failure<T>(Error.Unexpected("Maximum retry attempts exceeded", "RetryExhausted"));
    }
}
