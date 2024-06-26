﻿namespace FunctionalDdd;

using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;

public static partial class HttpResponseMessageJsonExtensionsAsync
{
    /// <summary>
    /// Handles the case when the HTTP response has a status code of NotFound.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="notFoundError">The error to return if the response has a status code of NotFound.</param>
    /// <returns>A Result object containing the HTTP response message.</returns>
    public static Result<HttpResponseMessage> HandleNotFound(
        this HttpResponseMessage response,
        NotFoundError notFoundError)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
            return Result.Failure<HttpResponseMessage>(notFoundError);

        return Result.Success(response);
    }

    /// <summary>
    /// Handles the case when the HTTP response has a status code of NotFound asynchronously.
    /// </summary>
    /// <param name="responseTask">The task representing the HTTP response message.</param>
    /// <param name="notFoundError">The error to return if the response has a status code of NotFound.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object containing the HTTP response message.</returns>
    public static async Task<Result<HttpResponseMessage>> HandleNotFoundAsync(
        this Task<HttpResponseMessage> responseTask,
        NotFoundError notFoundError)
    {
        var response = await responseTask;
        return response.HandleNotFound(notFoundError);
    }

    /// <summary>
    /// Handles the case when the HTTP response is not successful.
    /// </summary>
    /// <typeparam name="TContext">The type of the context object.</typeparam>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="callbackFailedStatusCode">The callback function to handle the failed status code.</param>
    /// <param name="context">The context object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object containing the HTTP response message.</returns>
    public static async Task<Result<HttpResponseMessage>> HandleFailureAsync<TContext>(
        this HttpResponseMessage response,
        Func<HttpResponseMessage, TContext, CancellationToken, Task<Error>> callbackFailedStatusCode,
        TContext context,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await callbackFailedStatusCode(response, context, cancellationToken);
            return Result.Failure<HttpResponseMessage>(error);
        }

        return Result.Success(response);
    }

    /// <summary>
    /// Handles the case when the HTTP response is not successful asynchronously.
    /// </summary>
    /// <typeparam name="TContext">The type of the context object.</typeparam>
    /// <param name="responseTask">The task representing the HTTP response message.</param>
    /// <param name="callbackFailedStatusCode">The callback function to handle the failed status code.</param>
    /// <param name="context">The context object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object containing the HTTP response message.</returns>
    public static async Task<Result<HttpResponseMessage>> HandleFailureAsync<TContext>(
        this Task<HttpResponseMessage> responseTask,
        Func<HttpResponseMessage, TContext, CancellationToken, Task<Error>> callbackFailedStatusCode,
        TContext context,
        CancellationToken cancellationToken)
    {
        var response = await responseTask;
        return await response.HandleFailureAsync(callbackFailedStatusCode, context, cancellationToken);
    }

    /// <summary>
    /// Reads the HTTP response content as JSON and deserializes it to the specified type.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the JSON content to.</typeparam>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="jsonTypeInfo">The JSON type information for deserialization.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object containing the deserialized value.</returns>
    public static async Task<Result<TValue>> ReadResultFromJsonAsync<TValue>(
        this HttpResponseMessage response,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken)
        => await response.ReadResultMaybeFromJsonAsync(jsonTypeInfo, cancellationToken)
            .BindAsync(maybe => maybe.HasValue ? Result.Success(maybe.Value) : Result.Failure<TValue>(Error.Unexpected($"Http Response was null for value {typeof(TValue).Name}.")));

    /// <summary>
    /// Reads the HTTP response content as JSON and deserializes it to the specified type asynchronously.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the JSON content to.</typeparam>
    /// <param name="responseTask">The task representing the HTTP response message.</param>
    /// <param name="jsonTypeInfo">The JSON type information for deserialization.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task{Result{TValue}}"/> object.</returns>
    public static async Task<Result<TValue>> ReadResultFromJsonAsync<TValue>(
        this Task<HttpResponseMessage> responseTask,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken)
    {
        var response = await responseTask;
        return await response.ReadResultFromJsonAsync(jsonTypeInfo, cancellationToken);
    }

    /// <summary>
    /// Reads the HTTP response content as JSON and deserializes it to the specified type using the Result monad.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the JSON content to.</typeparam>
    /// <param name="response">The Result object containing the HTTP response message.</param>
    /// <param name="jsonTypeInfo">The JSON type information for deserialization.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task{Result{TValue}}"/> object.</returns>
    public static async Task<Result<TValue>> ReadResultFromJsonAsync<TValue>(
        this Result<HttpResponseMessage> response,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken)
        => await response.BindAsync(response => response.ReadResultFromJsonAsync(jsonTypeInfo, cancellationToken));

    /// <summary>
    /// Reads the HTTP response content as JSON and deserializes it to the specified type using the Result monad asynchronously.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the JSON content to.</typeparam>
    /// <param name="responseTask">The task representing the Result object containing the HTTP response message.</param>
    /// <param name="jsonTypeInfo">The JSON type information for deserialization.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task{Result{TValue}}"/> object.</returns>
    public static async Task<Result<TValue>> ReadResultFromJsonAsync<TValue>(
        this Task<Result<HttpResponseMessage>> responseTask,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken)
    {
        var response = await responseTask;
        return await response.ReadResultFromJsonAsync(jsonTypeInfo, cancellationToken);
    }

    /// <summary>
    /// Reads the HTTP response content as JSON and deserializes it to the specified type using the Maybe monad.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the JSON content to.</typeparam>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="jsonTypeInfo">The JSON type information for deserialization.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task{Result{Maybe{TValue}}}"/> object.</returns>
    public static async Task<Result<Maybe<TValue>>> ReadResultMaybeFromJsonAsync<TValue>(
        this HttpResponseMessage response,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode == false)
            return Result.Failure<Maybe<TValue>>(Error.Unexpected($"Http Response is in a failed state for value {typeof(TValue).Name}. Status code: {response.StatusCode}"));

        var value = await response
            .Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken);

        return Result.Success(value is null ? Maybe.None<TValue>() : Maybe.From(value));
    }

    /// <summary>
    /// Reads the HTTP response content as JSON and deserializes it to the specified type using the Maybe monad asynchronously.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the JSON content to.</typeparam>
    /// <param name="responseTask">The task representing the HTTP response message.</param>
    /// <param name="jsonTypeInfo">The JSON type information for deserialization.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task{Result{Maybe{TValue}}}"/> object.</returns>
    public static async Task<Result<Maybe<TValue>>> ReadResultMaybeFromJsonAsync<TValue>(
        this Task<HttpResponseMessage> responseTask,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken)
    {
        var response = await responseTask;
        return await response.ReadResultMaybeFromJsonAsync(jsonTypeInfo, cancellationToken);
    }

    /// <summary>
    /// Reads the HTTP response content as JSON and deserializes it to the specified type using the Maybe monad and the Result monad.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the JSON content to.</typeparam>
    /// <param name="response">The Result object containing the HTTP response message.</param>
    /// <param name="jsonTypeInfo">The JSON type information for deserialization.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task{Result{Maybe{TValue}}}"/> object.</returns>
    public static async Task<Result<Maybe<TValue>>> ReadResultMaybeFromJsonAsync<TValue>(
        this Result<HttpResponseMessage> response,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken)
        => await response.BindAsync(response => response.ReadResultMaybeFromJsonAsync(jsonTypeInfo, cancellationToken));

    /// <summary>
    /// Reads the HTTP response content as JSON and deserializes it to the specified type using the Maybe monad and the Result monad asynchronously.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the JSON content to.</typeparam>
    /// <param name="responseTask">The task representing the Result object containing the HTTP response message.</param>
    /// <param name="jsonTypeInfo">The JSON type information for deserialization.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object containing the Maybe value.</returns>
    /// <returns>A <see cref="Task{Result{Maybe{TValue}}}"/> object.</returns>
    public static async Task<Result<Maybe<TValue>>> ReadResultMaybeFromJsonAsync<TValue>(
        this Task<Result<HttpResponseMessage>> responseTask,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken)
    {
        var response = await responseTask;
        return await response.ReadResultMaybeFromJsonAsync(jsonTypeInfo, cancellationToken);
    }
}
