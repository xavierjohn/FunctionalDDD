namespace FunctionalDdd;

using System.Threading.Tasks;

/// <summary>
/// Pattern matching helpers (Match / Switch) for Result.
/// </summary>
public static class MatchExtensions
{
    public static TOut Match<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);

    public static void Switch<TIn>(this Result<TIn> result, Action<TIn> onSuccess, Action<Error> onFailure)
    {
        if (result.IsSuccess) onSuccess(result.Value);
        else onFailure(result.Error);
    }
}

public static class MatchExtensionsAsync
{
    public static async Task<TOut> MatchAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Match(onSuccess, onFailure);
    }

    public static async Task<TOut> MatchAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> onSuccess, Func<Error, Task<TOut>> onFailure) =>
        result.IsSuccess
            ? await onSuccess(result.Value).ConfigureAwait(false)
            : await onFailure(result.Error).ConfigureAwait(false);

    public static async Task<TOut> MatchAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<TOut>> onSuccess, Func<Error, Task<TOut>> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        return await result.MatchAsync(onSuccess, onFailure).ConfigureAwait(false);
    }

    public static async Task SwitchAsync<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, Task> onSuccess, Func<Error, Task> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess) await onSuccess(result.Value).ConfigureAwait(false);
        else await onFailure(result.Error).ConfigureAwait(false);
    }
}