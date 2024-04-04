namespace FunctionalDdd;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

public static class HttpResponseMessageJsonExtensionsAsync
{
    /// <summary>
    /// Make the http request and return a Result with the deserialized value.
    /// If the http response status code is 404, return a Result with the notFoundError.
    /// If the response content is null, throw a JsonException.
    /// </summary>
    /// <typeparam name="T">The type for the <see cref="Result{T}"></see> object.</typeparam>
    /// <param name="response">Http response.</param>
    /// <param name="notFoundError">The <see cref="NotFoundError"></see> to return if the http response status code is <see cref="HttpStatusCode.NotFound"></see> </param>
    /// <param name="jsonTypeInfo">Provides JSON serialization-related metadata about a type.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A <see cref="Result{T}"/> object with the value.</returns>
    public static async Task<Result<T>> ResultReadValueAsync<T>(
        this HttpResponseMessage response,
        NotFoundError notFoundError,
        JsonTypeInfo<T> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
            return Result.Failure<T>(notFoundError);

        response.EnsureSuccessStatusCode();

        return await ReadAndThrowIfResponseIsNull(response, jsonTypeInfo, cancellationToken);
    }

    /// <summary>
    /// Make the http request and return a Result with the deserialized value.
    /// If the http response status code is 404, return a Result with the notFoundError.
    /// If the response content is null, throw a JsonException.
    /// </summary>
    /// <typeparam name="T">The type for the <see cref="Result{T}"></see> object.</typeparam>
    /// <param name="responseTask">A <see cref="Task"></see> that returns a <see cref="HttpResponseMessage"></see></param>
    /// <param name="notFoundError">The <see cref="NotFoundError"></see> to return if the http response status code is <see cref="HttpStatusCode.NotFound"></see> </param>
    /// <param name="jsonTypeInfo">Provides JSON serialization-related metadata about a type.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A <see cref="Result{T}"/> object with the value.</returns>>
    public static async Task<Result<T>> ResultReadValueAsync<T>(
        this Task<HttpResponseMessage> responseTask,
        NotFoundError notFoundError,
        JsonTypeInfo<T> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        using var response = await responseTask;
        return await response.ResultReadValueAsync(notFoundError, jsonTypeInfo, cancellationToken);
    }

    /// <summary>
    /// Make the http request and return a Result with the deserialized value
    /// If the http response status code is not a success status code, return a Result with the error from the callbackFailedStatusCode.
    /// If the response content is null, throw a JsonException.
    /// </summary>
    /// <typeparam name="TValue">The type for the <see cref="Result{T}"></see> object.</typeparam>
    /// <typeparam name="TContext">The Http Context.</typeparam>
    /// <param name="response">Http response.</param>
    /// <param name="callbackFailedStatusCode">The function to call to get the error object if the response is in a failed state.</param>
    /// <param name="context">HTTP context that is passed to the callback function.</param>
    /// <param name="jsonTypeInfo">Provides JSON serialization-related metadata about a type.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A <see cref="Result{T}"/> object with the value.</returns>
    public static async Task<Result<TValue>> ResultReadValueAsync<TValue, TContext>(
        this HttpResponseMessage response,
        Func<HttpResponseMessage, TContext, Task<Error>> callbackFailedStatusCode,
        TContext context,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await callbackFailedStatusCode(response, context);
            return Result.Failure<TValue>(error);
        }

        return await ReadAndThrowIfResponseIsNull(response, jsonTypeInfo, cancellationToken);
    }

    /// <summary>
    /// Make the http request and return a Result with the deserialized value
    /// If the http response status code is not a success status code, return a Result with the error from the callbackFailedStatusCode.
    /// If the response content is null, throw a JsonException.
    /// </summary>
    /// <typeparam name="TValue">The type for the <see cref="Result{T}"></see> object.</typeparam>
    /// <typeparam name="TContext">The Http Context.</typeparam>
    /// <param name="responseTask">A <see cref="Task"></see> that returns a <see cref="HttpResponseMessage"></see></param>
    /// <param name="callbackFailedStatusCode">The function to call to get the error object if the response is in a failed state.</param>
    /// <param name="context">HTTP context that is passed to the callback function.</param>
    /// <param name="jsonTypeInfo">Provides JSON serialization-related metadata about a type.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A <see cref="Result{T}"/> object with the value.</returns>
    public static async Task<Result<TValue>> ResultReadValueAsync<TValue, TContext>(
        this Task<HttpResponseMessage> responseTask,
        Func<HttpResponseMessage, TContext, Task<Error>> callbackFailedStatusCode,
        TContext context,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        using var response = await responseTask;
        return await response.ResultReadValueAsync(callbackFailedStatusCode, context, jsonTypeInfo, cancellationToken);
    }
    private static async Task<Result<TValue>> ReadAndThrowIfResponseIsNull<TValue>(HttpResponseMessage response, JsonTypeInfo<TValue> jsonTypeInfo, CancellationToken cancellationToken)
    {
        var t = await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken);
        return (t is null) ? throw new JsonException($"Failed to create {typeof(TValue).Name} from Json") : Result.Success(t);
    }
}
