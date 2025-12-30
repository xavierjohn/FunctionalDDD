namespace FunctionalDdd;

/// <summary>
/// Provides extension methods for compensating (recovering from) failed results by executing fallback operations.
/// </summary>
/// <remarks>
/// Compensation allows you to provide alternative paths when a Result fails, similar to try-catch recovery logic
/// but in a functional style. This is useful for implementing fallback strategies, default values, or error recovery.
/// </remarks>
public static class CompensateExtensions
{
    /// <summary>
    /// Compensates for a failed result by calling the given function.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to compensate if it's a failure.</param>
    /// <param name="func">The function to call for compensation.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static Result<T> Compensate<T>(this Result<T> result, Func<Result<T>> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        return func();
    }

    /// <summary>
    /// Compensates for a failed result by calling the given function with the error.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to compensate if it's a failure.</param>
    /// <param name="func">The function that receives the error and returns a compensation result.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static Result<T> Compensate<T>(this Result<T> result, Func<Error, Result<T>> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        return func(result.Error);
    }

    /// <summary>
    /// Compensates for a failed result by calling the given function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="func">The function to call for compensation if the predicate is true.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the compensation function.</returns>
    public static Result<T> Compensate<T>(this Result<T> result, Func<Error, bool> predicate, Func<Result<T>> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        if (predicate(result.Error))
            return func();

        return result;
    }

    /// <summary>
    /// Compensates for a failed result by calling the given function with the error if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="func">The function that receives the error and returns a compensation result if the predicate is true.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the compensation function.</returns>
    public static Result<T> Compensate<T>(this Result<T> result, Func<Error, bool> predicate, Func<Error, Result<T>> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        if (predicate(result.Error))
            return func(result.Error);

        return result;
    }
}

/// <summary>
/// Provides asynchronous extension methods for compensating (recovering from) failed results.
/// </summary>
public static class CompensateExtensionsAsync
{
    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given function.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to compensate if it's a failure.</param>
    /// <param name="func">The function to call for compensation.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Result<T>> func)
    {
        Result<T> result = await resultTask;
        return result.Compensate(func);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to compensate if it's a failure.</param>
    /// <param name="funcAsync">The async function to call for compensation.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Result<T> result, Func<CancellationToken, Task<Result<T>>> funcAsync, CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        return await funcAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to compensate if it's a failure.</param>
    /// <param name="funcAsync">The async function to call for compensation.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Result<T> result, Func<Task<Result<T>>> funcAsync)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        return await funcAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to compensate if it's a failure.</param>
    /// <param name="funcAsync">The async function to call for compensation.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<CancellationToken, Task<Result<T>>> funcAsync, CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.CompensateAsync(funcAsync, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to compensate if it's a failure.</param>
    /// <param name="funcAsync">The async function to call for compensation.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Task<Result<T>>> funcAsync)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.CompensateAsync(funcAsync).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given function with the error.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to compensate if it's a failure.</param>
    /// <param name="func">The function that receives the error and returns a compensation result.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Error, Result<T>> func)
    {
        Result<T> result = await resultTask;
        return result.Compensate(func);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function with the error.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to compensate if it's a failure.</param>
    /// <param name="funcAsync">The async function that receives the error and cancellation token.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Result<T> result, Func<Error, CancellationToken, Task<Result<T>>> funcAsync, CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        return await funcAsync(result.Error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function with the error.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to compensate if it's a failure.</param>
    /// <param name="funcAsync">The async function that receives the error.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Result<T> result, Func<Error, Task<Result<T>>> funcAsync)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        return await funcAsync(result.Error).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function with the error.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to compensate if it's a failure.</param>
    /// <param name="funcAsync">The async function that receives the error and cancellation token.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Error, CancellationToken, Task<Result<T>>> funcAsync, CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        
        return await result.CompensateAsync(funcAsync, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function with the error.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to compensate if it's a failure.</param>
    /// <param name="funcAsync">The async function that receives the error.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Error, Task<Result<T>>> funcAsync)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        
        return await result.CompensateAsync(funcAsync).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="func">The function to call for compensation if the predicate is true.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Error, bool> predicate, Func<Result<T>> func)
    {
        Result<T> result = await resultTask;
        return result.Compensate(predicate, func);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given function with the error if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="func">The function that receives the error and returns a compensation result if the predicate is true.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Error, bool> predicate, Func<Error, Result<T>> func)
    {
        Result<T> result = await resultTask;
        return result.Compensate(predicate, func);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="funcAsync">The async function to call for compensation if the predicate is true.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Result<T> result, Func<Error, bool> predicate, Func<CancellationToken, Task<Result<T>>> funcAsync, CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        if (predicate(result.Error))
            return await funcAsync(cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="funcAsync">The async function to call for compensation if the predicate is true.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Result<T> result, Func<Error, bool> predicate, Func<Task<Result<T>>> funcAsync)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        if (predicate(result.Error))
            return await funcAsync().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function with the error if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="funcAsync">The async function that receives the error and cancellation token if the predicate is true.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Result<T> result, Func<Error, bool> predicate, Func<Error, CancellationToken, Task<Result<T>>> funcAsync, CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        if (predicate(result.Error))
            return await funcAsync(result.Error, cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function with the error if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="funcAsync">The async function that receives the error if the predicate is true.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Result<T> result, Func<Error, bool> predicate, Func<Error, Task<Result<T>>> funcAsync)
    {
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        if (predicate(result.Error))
            return await funcAsync(result.Error).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="funcAsync">The async function to call for compensation if the predicate is true.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Error, bool> predicate, Func<CancellationToken, Task<Result<T>>> funcAsync, CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.CompensateAsync(predicate, funcAsync, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="funcAsync">The async function to call for compensation if the predicate is true.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Error, bool> predicate, Func<Task<Result<T>>> funcAsync)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.CompensateAsync(predicate, funcAsync).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function with the error if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="funcAsync">The async function that receives the error and cancellation token if the predicate is true.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Error, bool> predicate, Func<Error, CancellationToken, Task<Result<T>>> funcAsync, CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.CompensateAsync(predicate, funcAsync, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function with the error if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="funcAsync">The async function that receives the error if the predicate is true.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the compensation function.</returns>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Error, bool> predicate, Func<Error, Task<Result<T>>> funcAsync)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.CompensateAsync(predicate, funcAsync).ConfigureAwait(false);
    }

    // ValueTask overloads

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given function.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">ValueTask containing the result to compensate if it's a failure.</param>
    /// <param name="func">The function to call for compensation.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static async ValueTask<Result<T>> CompensateAsync<T>(this ValueTask<Result<T>> resultTask, Func<Result<T>> func)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return result.Compensate(func);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">ValueTask containing the result to compensate if it's a failure.</param>
    /// <param name="funcAsync">The async function to call for compensation.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static async ValueTask<Result<T>> CompensateAsync<T>(this ValueTask<Result<T>> resultTask, Func<CancellationToken, ValueTask<Result<T>>> funcAsync, CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        return await funcAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">ValueTask containing the result to compensate if it's a failure.</param>
    /// <param name="funcAsync">The async function to call for compensation.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static async ValueTask<Result<T>> CompensateAsync<T>(this ValueTask<Result<T>> resultTask, Func<ValueTask<Result<T>>> funcAsync)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        return await funcAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given function with the error.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">ValueTask containing the result to compensate if it's a failure.</param>
    /// <param name="func">The function that receives the error and returns a compensation result.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static async ValueTask<Result<T>> CompensateAsync<T>(this ValueTask<Result<T>> resultTask, Func<Error, Result<T>> func)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return result.Compensate(func);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function with the error.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">ValueTask containing the result to compensate if it's a failure.</param>
    /// <param name="funcAsync">The async function that receives the error and cancellation token.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static async ValueTask<Result<T>> CompensateAsync<T>(this ValueTask<Result<T>> resultTask, Func<Error, CancellationToken, ValueTask<Result<T>>> funcAsync, CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        return await funcAsync(result.Error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function with the error.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">ValueTask containing the result to compensate if it's a failure.</param>
    /// <param name="funcAsync">The async function that receives the error.</param>
    /// <returns>The original result if success; otherwise the result from the compensation function.</returns>
    public static async ValueTask<Result<T>> CompensateAsync<T>(this ValueTask<Result<T>> resultTask, Func<Error, ValueTask<Result<T>>> funcAsync)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        return await funcAsync(result.Error).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">ValueTask containing the result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="func">The function to call for compensation if the predicate is true.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the compensation function.</returns>
    public static async ValueTask<Result<T>> CompensateAsync<T>(this ValueTask<Result<T>> resultTask, Func<Error, bool> predicate, Func<Result<T>> func)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return result.Compensate(predicate, func);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given function with the error if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">ValueTask containing the result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="func">The function that receives the error and returns a compensation result if the predicate is true.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the compensation function.</returns>
    public static async ValueTask<Result<T>> CompensateAsync<T>(this ValueTask<Result<T>> resultTask, Func<Error, bool> predicate, Func<Error, Result<T>> func)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return result.Compensate(predicate, func);
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">ValueTask containing the result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="funcAsync">The async function to call for compensation if the predicate is true.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the compensation function.</returns>
    public static async ValueTask<Result<T>> CompensateAsync<T>(this ValueTask<Result<T>> resultTask, Func<Error, bool> predicate, Func<CancellationToken, ValueTask<Result<T>>> funcAsync, CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        if (predicate(result.Error))
            return await funcAsync(cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Asynchronously compensates for a failed result by calling the given async function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">ValueTask containing the result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="funcAsync">The async function to call for compensation if the predicate is true.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the compensation function.</returns>
    public static async ValueTask<Result<T>> CompensateAsync<T>(this ValueTask<Result<T>> resultTask, Func<Error, bool> predicate, Func<ValueTask<Result<T>>> funcAsync)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        using var activity = RopTrace.ActivitySource.StartActivity(nameof(CompensateExtensions.Compensate));
        if (result.IsSuccess)
            return result;

        if (predicate(result.Error))
            return await funcAsync().ConfigureAwait(false);

        return result;
    }
}
