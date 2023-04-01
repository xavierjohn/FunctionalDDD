namespace FunctionalDDD;

using System.Net.Http.Json;

public static class HttpResponseMessageJsonExtensions
{
    public static async Task<Result<T>> ReadResultWithNotFoundAsync<T>(
        this HttpResponseMessage response,
        NotFoundError notFoundError,
        CancellationToken cancellationToken = default)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return Result.Failure<T>(notFoundError);

        response.EnsureSuccessStatusCode();

        var t = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (t is null)
            return Result.Failure<T>(notFoundError);

        return Result.Success(t);
    }
}
