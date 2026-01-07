namespace FunctionalDdd;

using System.Diagnostics;
using System.Threading.Tasks;

/// <summary>
/// Provides extension methods for transforming errors in failed Results while leaving successful Results unchanged.
/// </summary>
/// <remarks>
/// <para>
/// MapError is useful when you need to convert, enrich, or translate errors between different layers of your application.
/// For example, converting domain errors to API-specific errors, or adding context to generic errors.
/// </para>
/// <para>
/// The error transformation only occurs if the Result is a failure; successful Results pass through unchanged.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Convert a generic error to a more specific one
/// var result = GetUser(id)
///     .MapError(err => Error.NotFound($"User {id} not found", id));
/// 
/// // Add context to an error
/// var result = ValidateEmail(email)
///     .MapError(err => Error.Validation($"Email validation failed: {err.Detail}", "email"));
/// 
/// // Convert between error types for different layers
/// var apiResult = domainResult
///     .MapError(domainErr => ConvertToApiError(domainErr));
/// </code>
/// </example>
public static class MapErrorExtensions
{
    /// <summary>
    /// Transforms the error of a failed Result using the provided mapping function.
    /// If the Result is successful, it is returned unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result whose error to potentially transform.</param>
    /// <param name="map">The function to transform the error.</param>
    /// <returns>The original result if successful; otherwise a new failure with the transformed error.</returns>
    public static Result<T> MapError<T>(this Result<T> result, Func<Error, Error> map)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsSuccess) return result;
        activity?.SetStatus(ActivityStatusCode.Error);
        return Result.Failure<T>(map(result.Error));
    }

    /// <summary>
    /// Asynchronously transforms the error of a failed Result using the provided mapping function.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">The task containing the result whose error to potentially transform.</param>
    /// <param name="map">The function to transform the error.</param>
    /// <returns>The original result if successful; otherwise a new failure with the transformed error.</returns>
    public static async Task<Result<T>> MapErrorAsync<T>(this Task<Result<T>> resultTask, Func<Error, Error> map)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return result.MapError(map);
    }

    /// <summary>
    /// Asynchronously transforms the error of a failed Result using the provided async mapping function.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result whose error to potentially transform.</param>
    /// <param name="mapAsync">The async function to transform the error.</param>
    /// <returns>The original result if successful; otherwise a new failure with the transformed error.</returns>
    public static async Task<Result<T>> MapErrorAsync<T>(this Result<T> result, Func<Error, Task<Error>> mapAsync)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsSuccess) return result;
        activity?.SetStatus(ActivityStatusCode.Error);
        Error newError = await mapAsync(result.Error).ConfigureAwait(false);
        return Result.Failure<T>(newError);
    }

    /// <summary>
    /// Asynchronously transforms the error of a failed Result using the provided async mapping function with cancellation support.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result whose error to potentially transform.</param>
    /// <param name="mapAsync">The async function to transform the error.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result if successful; otherwise a new failure with the transformed error.</returns>
    public static async Task<Result<T>> MapErrorAsync<T>(
        this Result<T> result,
        Func<Error, CancellationToken, Task<Error>> mapAsync,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsSuccess) return result;
        activity?.SetStatus(ActivityStatusCode.Error);
        Error newError = await mapAsync(result.Error, cancellationToken).ConfigureAwait(false);
        return Result.Failure<T>(newError);
    }

    /// <summary>
    /// Asynchronously transforms the error of a failed Result using the provided async mapping function.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">The task containing the result whose error to potentially transform.</param>
    /// <param name="mapAsync">The async function to transform the error.</param>
    /// <returns>The original result if successful; otherwise a new failure with the transformed error.</returns>
    public static async Task<Result<T>> MapErrorAsync<T>(this Task<Result<T>> resultTask, Func<Error, Task<Error>> mapAsync)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.MapErrorAsync(mapAsync).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously transforms the error of a failed Result using the provided async mapping function with cancellation support.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">The task containing the result whose error to potentially transform.</param>
    /// <param name="mapAsync">The async function to transform the error.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result if successful; otherwise a new failure with the transformed error.</returns>
    public static async Task<Result<T>> MapErrorAsync<T>(
        this Task<Result<T>> resultTask,
        Func<Error, CancellationToken, Task<Error>> mapAsync,
        CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.MapErrorAsync(mapAsync, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously transforms the error of a failed Result using the provided mapping function.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">The ValueTask containing the result whose error to potentially transform.</param>
    /// <param name="map">The function to transform the error.</param>
    /// <returns>The original result if successful; otherwise a new failure with the transformed error.</returns>
    public static async ValueTask<Result<T>> MapErrorAsync<T>(this ValueTask<Result<T>> resultTask, Func<Error, Error> map)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return result.MapError(map);
    }

    /// <summary>
    /// Asynchronously transforms the error of a failed Result using the provided async mapping function.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result whose error to potentially transform.</param>
    /// <param name="mapAsync">The async function to transform the error.</param>
    /// <returns>The original result if successful; otherwise a new failure with the transformed error.</returns>
    public static async ValueTask<Result<T>> MapErrorAsync<T>(this Result<T> result, Func<Error, ValueTask<Error>> mapAsync)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsSuccess) return result;
        activity?.SetStatus(ActivityStatusCode.Error);
        Error newError = await mapAsync(result.Error).ConfigureAwait(false);
        return Result.Failure<T>(newError);
    }

    /// <summary>
    /// Asynchronously transforms the error of a failed Result using the provided async mapping function with cancellation support.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result whose error to potentially transform.</param>
    /// <param name="mapAsync">The async function to transform the error.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result if successful; otherwise a new failure with the transformed error.</returns>
    public static async ValueTask<Result<T>> MapErrorAsync<T>(
        this Result<T> result,
        Func<Error, CancellationToken, ValueTask<Error>> mapAsync,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsSuccess) return result;
        activity?.SetStatus(ActivityStatusCode.Error);
        Error newError = await mapAsync(result.Error, cancellationToken).ConfigureAwait(false);
        return Result.Failure<T>(newError);
    }

    /// <summary>
    /// Asynchronously transforms the error of a failed Result using the provided async mapping function.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">The ValueTask containing the result whose error to potentially transform.</param>
    /// <param name="mapAsync">The async function to transform the error.</param>
    /// <returns>The original result if successful; otherwise a new failure with the transformed error.</returns>
    public static async ValueTask<Result<T>> MapErrorAsync<T>(this ValueTask<Result<T>> resultTask, Func<Error, ValueTask<Error>> mapAsync)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.MapErrorAsync(mapAsync).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously transforms the error of a failed Result using the provided async mapping function with cancellation support.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">The ValueTask containing the result whose error to potentially transform.</param>
    /// <param name="mapAsync">The async function to transform the error.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>The original result if successful; otherwise a new failure with the transformed error.</returns>
    public static async ValueTask<Result<T>> MapErrorAsync<T>(
        this ValueTask<Result<T>> resultTask,
        Func<Error, CancellationToken, ValueTask<Error>> mapAsync,
        CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.MapErrorAsync(mapAsync, cancellationToken).ConfigureAwait(false);
    }
}