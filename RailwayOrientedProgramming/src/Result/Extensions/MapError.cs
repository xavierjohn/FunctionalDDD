namespace FunctionalDdd;

using System.Threading.Tasks;

/// <summary>
/// Transforms the Error of a failed Result while leaving successful Results unchanged.
/// </summary>
public static class MapErrorExtensions
{
    public static Result<T> MapError<T>(this Result<T> result, Func<Error, Error> map)
    {
        if (result.IsSuccess) return result;
        return Result.Failure<T>(map(result.Error));
    }

    public static async Task<Result<T>> MapErrorAsync<T>(this Task<Result<T>> resultTask, Func<Error, Error> map)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return result.MapError(map);
    }

    public static async Task<Result<T>> MapErrorAsync<T>(this Result<T> result, Func<Error, Task<Error>> mapAsync)
    {
        if (result.IsSuccess) return result;
        Error newError = await mapAsync(result.Error).ConfigureAwait(false);
        return Result.Failure<T>(newError);
    }

    public static async Task<Result<T>> MapErrorAsync<T>(this Task<Result<T>> resultTask, Func<Error, Task<Error>> mapAsync)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.MapErrorAsync(mapAsync).ConfigureAwait(false);
    }
}