namespace FunctionalDdd;

using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FunctionalDdd;

public static class HttpResponseMessageJsonExtensionsAsync
{
    public static async Task<Result<T>> ReadResultWithNotFoundAsync<T>(
        this HttpResponseMessage response,
        NotFoundError notFoundError,
        JsonTypeInfo<T> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return Result.Failure<T>(notFoundError);

        response.EnsureSuccessStatusCode();

        var t = await response.Content.ReadFromJsonAsync<T>(jsonTypeInfo, cancellationToken)
            .ConfigureAwait(false);
        return (t is null) ? Result.Failure<T>(notFoundError) : Result.Success(t);
    }

    public static async Task<Result<T>> ReadResultWithNotFoundAsync<T>(
        this Task<HttpResponseMessage> responseTask,
        NotFoundError notFoundError,
        JsonTypeInfo<T> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        using var response = await responseTask.ConfigureAwait(false);
        return await response.ReadResultWithNotFoundAsync<T>(notFoundError, jsonTypeInfo, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<Result<TValue>> ReadResultAsync<TValue, TContext>(
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

        var t = await response.Content.ReadFromJsonAsync<TValue>(jsonTypeInfo, cancellationToken)
            .ConfigureAwait(false);
        return (t is null) ? throw new JsonException($"Failed to create {typeof(TValue).Name} from Json") : Result.Success(t);
    }

    public static async Task<Result<TValue>> ReadResultAsync<TValue, TContext>(
        this Task<HttpResponseMessage> responseTask,
        Func<HttpResponseMessage, TContext, Task<Error>> callbackFailedStatusCode,
        TContext context,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        using var response = await responseTask.ConfigureAwait(false);
        return await response.ReadResultAsync<TValue, TContext>(callbackFailedStatusCode, context, jsonTypeInfo, cancellationToken)
            .ConfigureAwait(false);
    }
}
