namespace FunctionalDdd;

using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using FunctionalDdd;

public static class HttpResponseMessageJsonExtensionsAsync
{
    public static async Task<Result<T>> ReadResultWithNotFoundAsync<T>(
        this HttpResponseMessage response,
        NotFoundError notFoundError,
        JsonSerializerOptions? jsonSerializerOptions,
        CancellationToken cancellationToken = default)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return Result.Failure<T>(notFoundError);

        response.EnsureSuccessStatusCode();

        var t = await response.Content.ReadFromJsonAsync<T>(jsonSerializerOptions, cancellationToken)
            .ConfigureAwait(false);
        return (t is null) ? Result.Failure<T>(notFoundError) : Result.Success(t);
    }

    public static Task<Result<T>> ReadResultWithNotFoundAsync<T>(
        this HttpResponseMessage response,
        NotFoundError notFoundError,
        CancellationToken cancellationToken = default)
            => response.ReadResultWithNotFoundAsync<T>(notFoundError, null, cancellationToken);

    public static async Task<Result<T>> ReadResultWithNotFoundAsync<T>(
        this Task<HttpResponseMessage> responseTask,
        NotFoundError notFoundError,
        JsonSerializerOptions? jsonSerializerOptions,
        CancellationToken cancellationToken = default)
    {
        using var response = await responseTask.ConfigureAwait(false);
        return await response.ReadResultWithNotFoundAsync<T>(notFoundError, jsonSerializerOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    public static Task<Result<T>> ReadResultWithNotFoundAsync<T>(
        this Task<HttpResponseMessage> responseTask,
        NotFoundError notFoundError,
        CancellationToken cancellationToken = default)
        => responseTask.ReadResultWithNotFoundAsync<T>(notFoundError, null, cancellationToken);

    public static async Task<Result<TValue>> ReadResultAsync<TValue, TContext>(
        this HttpResponseMessage response,
        Func<HttpResponseMessage, TContext, Task<Error>> callbackFailedStatusCode,
        TContext context,
        JsonSerializerOptions? jsonSerializerOptions,
        CancellationToken cancellationToken = default)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await callbackFailedStatusCode(response, context);
            return Result.Failure<TValue>(error);
        }

        var t = await response.Content.ReadFromJsonAsync<TValue>(jsonSerializerOptions, cancellationToken)
            .ConfigureAwait(false);
        return (t is null) ? throw new JsonException($"Failed to create {typeof(TValue).Name} from Json") : Result.Success(t);
    }

    public static Task<Result<TValue>> ReadResultAsync<TValue, TContext>(
        this HttpResponseMessage response,
        Func<HttpResponseMessage, TContext, Task<Error>> callbackFailedStatusCode,
        TContext context,
        CancellationToken cancellationToken = default)
            => response.ReadResultAsync<TValue, TContext>(callbackFailedStatusCode, context, null, cancellationToken);

    public static async Task<Result<TValue>> ReadResultAsync<TValue, TContext>(
        this Task<HttpResponseMessage> responseTask,
        Func<HttpResponseMessage, TContext, Task<Error>> callbackFailedStatusCode,
        TContext context,
        JsonSerializerOptions? jsonSerializerOptions,
        CancellationToken cancellationToken = default)
    {
        using var response = await responseTask.ConfigureAwait(false);
        return await response.ReadResultAsync<TValue, TContext>(callbackFailedStatusCode, context, jsonSerializerOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    public static Task<Result<TValue>> ReadResultAsync<TValue, TContext>(
        this Task<HttpResponseMessage> responseTask,
        Func<HttpResponseMessage, TContext, Task<Error>> callbackFailedStatusCode,
        TContext context,
        CancellationToken cancellationToken = default)
                => responseTask.ReadResultAsync<TValue, TContext>(callbackFailedStatusCode, context, null, cancellationToken);
}
