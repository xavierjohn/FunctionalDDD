namespace FunctionalDDD.Results;

/// <summary>
/// If the starting Result is a success, the Map function will call and  wrap the result of a given function with a new success result.
/// </summary>
public static partial class MapExtensions
{
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> func)
    {
        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error);

        return Result.Success<TOut>(func(result.Value));
    }

}
public static class MapExtensionsAsync
{

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<TOut>> func)
    {
        Result<TIn> result = await resultTask.ConfigureAwait(false);

        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error);

        TOut value = await func(result.Value).ConfigureAwait(false);

        return Result.Success<TOut>(value);
    }

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, TOut> func)
    {
        Result<TIn> result = await resultTask;

        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error);

        TOut value = func(result.Value);

        return Result.Success<TOut>(value);
    }

    public static async ValueTask<Result<TOut>> MapAsync<TIn, TOut>(this ValueTask<Result<TIn>> resultTask, Func<TIn, ValueTask<TOut>> valueTask)
    {
        Result<TIn> result = await resultTask;

        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error);

        TOut value = await valueTask(result.Value);

        return Result.Success<TOut>(value);
    }

    public static async ValueTask<Result<TOut>> MapAsync<TIn, TOut>(this ValueTask<Result<TIn>> resultTask, Func<TIn, TOut> func)
    {
        Result<TIn> result = await resultTask;

        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error);

        TOut value = func(result.Value);

        return Result.Success<TOut>(value);
    }
}
