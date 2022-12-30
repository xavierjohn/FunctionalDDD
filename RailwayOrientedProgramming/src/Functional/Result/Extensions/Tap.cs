namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<TResult> Tap<TResult>(this Result<TResult> result, Action action)
    {
        if (result.IsSuccess)
            action();

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
            action(result.Value);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async Task<Result<TResult>> TapAsync<TResult>(this Task<Result<TResult>> resultTask, Action action)
    {
        var result = await resultTask.DefaultAwait();
        if (result.IsSuccess)
            action();

        return result;
    }
}
