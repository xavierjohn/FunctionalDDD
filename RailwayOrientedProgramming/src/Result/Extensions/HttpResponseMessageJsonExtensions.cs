namespace FunctionalDDD;

using System.Net.Http.Json;
using System.Text.Json;

public static class HttpResponseMessageJsonExtensions
{
    public static async Task<Result<T>> ReadResultWithNotFoundAsync<T>(
        this HttpResponseMessage response,
        NotFoundError notFoundError,
        JsonSerializerOptions? jsonSerializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return Result.Failure<T>(notFoundError);

        response.EnsureSuccessStatusCode();

        var t = await response.Content.ReadFromJsonAsync<T>(jsonSerializerOptions, cancellationToken)
            .ConfigureAwait(false);
        return (t is null) ? Result.Failure<T>(notFoundError) : Result.Success(t);
    }


    public static async Task<Result<T>> ReadResultWithNotFoundAsync<T>(
    this Task<HttpResponseMessage> responseTask,
    NotFoundError notFoundError,
    JsonSerializerOptions? jsonSerializerOptions = null,
    CancellationToken cancellationToken = default)
    {
        using var response = await responseTask.ConfigureAwait(false);
        return await response.ReadResultWithNotFoundAsync<T>(notFoundError, jsonSerializerOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<Result<T>> ReadResultAsync<T>(
    this HttpResponseMessage response,
    Func<HttpResponseMessage, Task<Error>> callbackFailedStatusCode,
    JsonSerializerOptions? jsonSerializerOptions = null,
    CancellationToken cancellationToken = default)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await callbackFailedStatusCode(response);
            return Result.Failure<T>(error);
        }

        var t = await response.Content.ReadFromJsonAsync<T>(jsonSerializerOptions, cancellationToken)
            .ConfigureAwait(false);
        return (t is null) ? throw new JsonException($"Failed to create {typeof(T).Name} from Json") : Result.Success(t);
    }


    public static async Task<Result<T>> ReadResultAsync<T>(
    this Task<HttpResponseMessage> responseTask,
    Func<HttpResponseMessage, Task<Error>> callbackFailedStatusCode,
    JsonSerializerOptions? jsonSerializerOptions = null,
    CancellationToken cancellationToken = default)
    {
        using var response = await responseTask.ConfigureAwait(false);
        return await response.ReadResultAsync<T>(callbackFailedStatusCode, jsonSerializerOptions, cancellationToken)
            .ConfigureAwait(false);
    }
}
