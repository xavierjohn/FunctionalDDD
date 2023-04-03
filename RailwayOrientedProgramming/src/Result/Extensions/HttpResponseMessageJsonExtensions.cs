namespace FunctionalDDD;

using System.Net.Http.Json;
using System.Text.Json;

public static class HttpResponseMessageJsonExtensions
{
    public static Task<Result<T>> ReadResultWithNotFoundAsync<T>(
        this HttpResponseMessage response,
        NotFoundError notFoundError,
        CancellationToken cancellationToken = default)
    => response.ReadResultWithNotFoundAsync<T>(notFoundError, null, cancellationToken);

    public static async Task<Result<T>> ReadResultWithNotFoundAsync<T>(
        this HttpResponseMessage response,
        NotFoundError notFoundError,
        JsonSerializerOptions? jsonSerializerOptions,
        CancellationToken cancellationToken = default)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return Result.Failure<T>(notFoundError);

        response.EnsureSuccessStatusCode();

        var t = await response.Content.ReadFromJsonAsync<T>(jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
        return (t is null) ? Result.Failure<T>(notFoundError) : Result.Success(t);
    }

    public static Task<Result<T>> ReadResultWithNotFoundAsync<T>(
    this Task<HttpResponseMessage> responseTask,
    NotFoundError notFoundError,
    CancellationToken cancellationToken = default)
        => responseTask.ReadResultWithNotFoundAsync<T>(notFoundError, null, cancellationToken);

    public static async Task<Result<T>> ReadResultWithNotFoundAsync<T>(
    this Task<HttpResponseMessage> responseTask,
    NotFoundError notFoundError,
    JsonSerializerOptions? jsonSerializerOptions,
    CancellationToken cancellationToken = default)
    {
        using var response = await responseTask.ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return Result.Failure<T>(notFoundError);

        response.EnsureSuccessStatusCode();

        var t = await response.Content.ReadFromJsonAsync<T>(jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
        return (t is null) ? Result.Failure<T>(notFoundError) : Result.Success(t);
    }
}
