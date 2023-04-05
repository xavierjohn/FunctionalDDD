namespace FunctionalDDD;

using System.Net.Http.Json;
using System.Text.Json;

public static class HttpResponseMessageJsonExtensions
{
    public static async Task<Result<T>> ReadResultWithNotFoundAsync<T>(
        this HttpResponseMessage response,
        NotFoundError notFoundError,
        Func<HttpResponseMessage, Task>? callbackFailedStatusCode = null,
        JsonSerializerOptions? jsonSerializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return Result.Failure<T>(notFoundError);

        if (!response.IsSuccessStatusCode && callbackFailedStatusCode is not null)
            await callbackFailedStatusCode(response);

        response.EnsureSuccessStatusCode();

        var t = await response.Content.ReadFromJsonAsync<T>(jsonSerializerOptions, cancellationToken)
            .ConfigureAwait(false);
        return (t is null) ? Result.Failure<T>(notFoundError) : Result.Success(t);
    }


    public static async Task<Result<T>> ReadResultWithNotFoundAsync<T>(
    this Task<HttpResponseMessage> responseTask,
    NotFoundError notFoundError,
    Func<HttpResponseMessage, Task>? callbackFailedStatusCode = null,
    JsonSerializerOptions? jsonSerializerOptions = null,
    CancellationToken cancellationToken = default)
    {
        using var response = await responseTask.ConfigureAwait(false);
        return await response.ReadResultWithNotFoundAsync<T>(notFoundError, callbackFailedStatusCode, jsonSerializerOptions, cancellationToken)
            .ConfigureAwait(false);
    }
}
