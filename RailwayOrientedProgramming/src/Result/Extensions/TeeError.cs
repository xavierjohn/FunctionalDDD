namespace FunctionalDDD.RailwayOrientedProgramming;

using FunctionalDDD.RailwayOrientedProgramming.Errors;

/// <summary>
/// Executes the given action if the calling <see cref="Result{TValue}"/> is a failure. Returns the calling <see cref="Result{TValue}"/>.
/// </summary>
public static class TeeErrorExtensions
{
    public static Result<TValue> TeeError<TValue>(this Result<TValue> result, Action action)
    {
        if (result.IsFailure)
            action();

        return result;
    }

    public static Result<TValue> TeeError<TValue>(this Result<TValue> result, Action<Error> action)
    {
        if (result.IsFailure)
            action(result.Error);

        return result;
    }
}

/// <summary>
/// Executes the given action if the calling <see cref="Result{TValue}"/> is a failure. Returns the calling <see cref="Result{TValue}"/>.
/// </summary>
public static class TeeErrorExtensionsAsync
{
    public static async Task<Result<TValue>> TeeErrorAsync<TValue>(this Task<Result<TValue>> resultTask, Action<Error> action)
    {
        Result<TValue> result = await resultTask.ConfigureAwait(false);
        return result.TeeError(action);
    }
}
