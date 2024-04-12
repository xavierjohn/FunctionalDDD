namespace FunctionalDdd;

using System.Diagnostics;

/// <summary>
/// Executes the given action if the calling <see cref="Result{TValue}"/> is a failure. Returns the calling <see cref="Result{TValue}"/>.
/// </summary>
public static class TapErrorExtensions
{
    public static Result<TValue> TapError<TValue>(this Result<TValue> result, Action action)
    {
        using var activity = Trace.ActivitySource.StartActivity();
        if (result.IsFailure)
        {
            action();
            activity?.SetStatus(ActivityStatusCode.Error);
        }

        return result;
    }

    public static Result<TValue> TapError<TValue>(this Result<TValue> result, Action<Error> action)
    {
        using var activity = Trace.ActivitySource.StartActivity();
        if (result.IsFailure)
        {
            action(result.Error);
            activity?.SetStatus(ActivityStatusCode.Error);
        }

        return result;
    }
}

/// <summary>
/// Executes the given action if the calling <see cref="Result{TValue}"/> is a failure. Returns the calling <see cref="Result{TValue}"/>.
/// </summary>
public static class TapErrorExtensionsAsync
{
    public static async Task<Result<TValue>> TapErrorAsync<TValue>(this Task<Result<TValue>> resultTask, Action<Error> action)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.TapError(action);
    }
}
