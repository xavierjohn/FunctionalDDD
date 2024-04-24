namespace FunctionalDdd;

/// <summary>
/// If the starting Result is a success, the Map function will call and  wrap the result of a given function with a new success result.
/// </summary>
public static partial class MapExtensions
{
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> func)
    {
        using var activity = Trace.ActivitySource.StartActivity();
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
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> func)
    {
        using var activity = Trace.ActivitySource.StartActivity("map");
        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error);

        TOut value = await func(result.Value);

        return Result.Success<TOut>(value);
    }

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, TOut> func)
    {
        Result<TIn> result = await resultTask;

        return result.Map(func);
    }

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<TOut>> func)
    {
        Result<TIn> result = await resultTask;

        return await result.MapAsync(func);
    }

    public static async ValueTask<Result<TOut>> MapAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, ValueTask<TOut>> func)
    {
        using var activity = Trace.ActivitySource.StartActivity("map");
        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error);

        TOut value = await func(result.Value);

        return Result.Success<TOut>(value);
    }

    public static async ValueTask<Result<TOut>> MapAsync<TIn, TOut>(this ValueTask<Result<TIn>> resultTask, Func<TIn, TOut> func)
    {
        Result<TIn> result = await resultTask;

        return result.Map(func);
    }

    public static async ValueTask<Result<TOut>> MapAsync<TIn, TOut>(this ValueTask<Result<TIn>> resultTask, Func<TIn, ValueTask<TOut>> func)
    {
        Result<TIn> result = await resultTask;

        return await result.MapAsync(func);
    }
}
