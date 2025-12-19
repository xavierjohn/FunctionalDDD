namespace FunctionalDdd;

/// <summary>
/// If the starting Result is a success, the Map function will call and  wrap the result of a given function with a new success result.
/// </summary>
public static partial class MapExtensions
{
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error);

        return Result.Success<TOut>(func(result.Value));
    }
}

/// <summary>
/// If the starting Result is a success, the Map function will call and  wrap the result of a given function with a new success result.
/// </summary>
public static partial class MapExtensionsAsync
{
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, CancellationToken, Task<TOut>> func, CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity("map");
        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error);

        TOut value = await func(result.Value, cancellationToken).ConfigureAwait(false);

        return Result.Success<TOut>(value);
    }

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity("map");
        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error);

        TOut value = await func(result.Value).ConfigureAwait(false);

        return Result.Success<TOut>(value);
    }

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, TOut> func)
    {
        Result<TIn> result = await resultTask.ConfigureAwait(false);

        return result.Map(func);
    }

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, CancellationToken, Task<TOut>> func, CancellationToken cancellationToken = default)
    {
        Result<TIn> result = await resultTask.ConfigureAwait(false);

        return await result.MapAsync(func, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<TOut>> func)
    {
        Result<TIn> result = await resultTask.ConfigureAwait(false);

        return await result.MapAsync(func).ConfigureAwait(false);
    }

    public static async ValueTask<Result<TOut>> MapAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, CancellationToken, ValueTask<TOut>> func, CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity("map");
        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error);

        TOut value = await func(result.Value, cancellationToken).ConfigureAwait(false);

        return Result.Success<TOut>(value);
    }

    public static async ValueTask<Result<TOut>> MapAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, ValueTask<TOut>> func)
    {
        using var activity = RopTrace.ActivitySource.StartActivity("map");
        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error);

        TOut value = await func(result.Value).ConfigureAwait(false);

        return Result.Success<TOut>(value);
    }

    public static async ValueTask<Result<TOut>> MapAsync<TIn, TOut>(this ValueTask<Result<TIn>> resultTask, Func<TIn, TOut> func)
    {
        Result<TIn> result = await resultTask.ConfigureAwait(false);

        return result.Map(func);
    }

    public static async ValueTask<Result<TOut>> MapAsync<TIn, TOut>(this ValueTask<Result<TIn>> resultTask, Func<TIn, CancellationToken, ValueTask<TOut>> func, CancellationToken cancellationToken = default)
    {
        Result<TIn> result = await resultTask.ConfigureAwait(false);

        return await result.MapAsync(func, cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<Result<TOut>> MapAsync<TIn, TOut>(this ValueTask<Result<TIn>> resultTask, Func<TIn, ValueTask<TOut>> func)
    {
        Result<TIn> result = await resultTask.ConfigureAwait(false);

        return await result.MapAsync(func).ConfigureAwait(false);
    }
}
