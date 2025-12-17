namespace FunctionalDdd;

using System.Diagnostics;
using System.Threading.Tasks;

/// <summary>
/// Transforms the Error of a failed Result while leaving successful Results unchanged.
/// </summary>
public static class MapErrorExtensions
{
    public static Result<T> MapError<T>(this Result<T> result, Func<Error, Error> map)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsSuccess) return result;
        activity?.SetStatus(ActivityStatusCode.Error);
        return Result.Failure<T>(map(result.Error));
    }

    public static async Task<Result<T>> MapErrorAsync<T>(this Task<Result<T>> resultTask, Func<Error, Error> map)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return result.MapError(map);
    }

    public static async Task<Result<T>> MapErrorAsync<T>(this Result<T> result, Func<Error, Task<Error>> mapAsync)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsSuccess) return result;
        activity?.SetStatus(ActivityStatusCode.Error);
        Error newError = await mapAsync(result.Error).ConfigureAwait(false);
        return Result.Failure<T>(newError);
    }

    public static async Task<Result<T>> MapErrorAsync<T>(
        this Result<T> result,
        Func<Error, CancellationToken, Task<Error>> mapAsync,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsSuccess) return result;
        activity?.SetStatus(ActivityStatusCode.Error);
        Error newError = await mapAsync(result.Error, cancellationToken).ConfigureAwait(false);
        return Result.Failure<T>(newError);
    }

    public static async Task<Result<T>> MapErrorAsync<T>(this Task<Result<T>> resultTask, Func<Error, Task<Error>> mapAsync)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.MapErrorAsync(mapAsync).ConfigureAwait(false);
    }

    public static async Task<Result<T>> MapErrorAsync<T>(
        this Task<Result<T>> resultTask,
        Func<Error, CancellationToken, Task<Error>> mapAsync,
        CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.MapErrorAsync(mapAsync, cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<Result<T>> MapErrorAsync<T>(this ValueTask<Result<T>> resultTask, Func<Error, Error> map)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return result.MapError(map);
    }

    public static async ValueTask<Result<T>> MapErrorAsync<T>(this Result<T> result, Func<Error, ValueTask<Error>> mapAsync)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsSuccess) return result;
        activity?.SetStatus(ActivityStatusCode.Error);
        Error newError = await mapAsync(result.Error).ConfigureAwait(false);
        return Result.Failure<T>(newError);
    }

    public static async ValueTask<Result<T>> MapErrorAsync<T>(
        this Result<T> result,
        Func<Error, CancellationToken, ValueTask<Error>> mapAsync,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsSuccess) return result;
        activity?.SetStatus(ActivityStatusCode.Error);
        Error newError = await mapAsync(result.Error, cancellationToken).ConfigureAwait(false);
        return Result.Failure<T>(newError);
    }

    public static async ValueTask<Result<T>> MapErrorAsync<T>(this ValueTask<Result<T>> resultTask, Func<Error, ValueTask<Error>> mapAsync)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.MapErrorAsync(mapAsync).ConfigureAwait(false);
    }

    public static async ValueTask<Result<T>> MapErrorAsync<T>(
        this ValueTask<Result<T>> resultTask,
        Func<Error, CancellationToken, ValueTask<Error>> mapAsync,
        CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.MapErrorAsync(mapAsync, cancellationToken).ConfigureAwait(false);
    }
}