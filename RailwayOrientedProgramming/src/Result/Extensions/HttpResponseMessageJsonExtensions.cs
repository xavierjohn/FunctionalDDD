namespace FunctionalDdd;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

public static partial class HttpResponseMessageJsonExtensionsAsync
{
    public static Result<HttpResponseMessage> HandleNotFound(
        this HttpResponseMessage response,
        NotFoundError notFoundError)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
            return Result.Failure<HttpResponseMessage>(notFoundError);

        return Result.Success(response);
    }

    public static async Task<Result<HttpResponseMessage>> HandleNotFoundAsync(
               this Task<HttpResponseMessage> responseTask,
                      NotFoundError notFoundError)
    {
        var response = await responseTask;
        return response.HandleNotFound(notFoundError);
    }

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

    public static async Task<Result<HttpResponseMessage>> HandleFailureAsync<TContext>(
        this Task<HttpResponseMessage> responseTask,
        Func<HttpResponseMessage, TContext, CancellationToken, Task<Error>> callbackFailedStatusCode,
        TContext context,
        CancellationToken cancellationToken)
    {
        var response = await responseTask;
        return await response.HandleFailureAsync(callbackFailedStatusCode, context, cancellationToken);
    }

    public static async Task<Result<TValue>> ReadResultFromJsonAsync<TValue>(
        this HttpResponseMessage response,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken,
        bool noThrow = false)
    {
        if (noThrow && response.IsSuccessStatusCode == false)
            return Result.Failure<TValue>(Error.Unexpected($"Http Response is in a failed state for value {typeof(TValue).Name}. Status code: {response.StatusCode}"));

        response.EnsureSuccessStatusCode();
        var value = await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken);

        return (value is null) ? throw new JsonException($"Failed to create {typeof(TValue).Name} from Json") : Result.Success(value);
    }

    public static async Task<Result<TValue>> ReadResultFromJsonAsync<TValue>(
    this Task<HttpResponseMessage> responseTask,
    JsonTypeInfo<TValue> jsonTypeInfo,
    CancellationToken cancellationToken)
    {
        var response = await responseTask;
        return await response.ReadResultFromJsonAsync(jsonTypeInfo, cancellationToken);
    }

    public static async Task<Result<TValue>> ReadResultFromJsonAsync<TValue>(
        this Result<HttpResponseMessage> response,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken)
        => await response.BindAsync(response => response.ReadResultFromJsonAsync(jsonTypeInfo, cancellationToken));

    public static async Task<Result<TValue>> ReadResultFromJsonAsync<TValue>(
    this Task<Result<HttpResponseMessage>> responseTask,
    JsonTypeInfo<TValue> jsonTypeInfo,
    CancellationToken cancellationToken)
    {
        var response = await responseTask;
        return await response.ReadResultFromJsonAsync(jsonTypeInfo, cancellationToken);
    }

    public static async Task<Result<Maybe<TValue>>> ReadResultMayBeFromJsonAsync<TValue>(
    this HttpResponseMessage response,
    JsonTypeInfo<TValue> jsonTypeInfo,
    CancellationToken cancellationToken,
     bool noThrow = false )
    {
        if (noThrow && response.IsSuccessStatusCode == false)
            return Result.Failure<Maybe<TValue>>(Error.Unexpected($"Http Response is in a failed state for value {typeof(TValue).Name}. Status code: {response.StatusCode}"));

        var value = await response
            .EnsureSuccessStatusCode()
            .Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken);

        return Result.Success(value is null ? Maybe.None<TValue>() : Maybe.From(value));
    }

    public static async Task<Result<Maybe<TValue>>> ReadResultMayBeFromJsonAsync<TValue>(
        this Task<HttpResponseMessage> responseTask,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken)
    {
        var response = await responseTask;
        return await response.ReadResultMayBeFromJsonAsync(jsonTypeInfo, cancellationToken);
    }
}
