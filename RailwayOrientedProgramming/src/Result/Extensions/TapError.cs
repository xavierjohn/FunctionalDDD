namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<T> TapError<T>(this Result<T> result, Action action)
    {
        if (result.IsFailure)
            action();

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<T> TapError<T>(this Result<T> result, Action<ErrorList> action)
    {
        if (result.IsFailure)
            action(result.Errors);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async Task<Result<T>> TapErrorAsync<T>(this Task<Result<T>> resultTask, Action<ErrorList> action)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return result.TapError(action);
    }
}
