namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<TOk, Error> Tap<TOk>(this Result<TOk, Error> result, Action action)
    {
        if (result.IsSuccess)
            action();

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<TOk, Error> Tap<TOk>(this Result<TOk, Error> result, Action<TOk> action)
    {
        if (result.IsSuccess)
            action(result.Ok);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async Task<Result<TOk, Error>> TapAsync<TOk>(this Task<Result<TOk, Error>> resultTask, Action<TOk> action)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);
        return result.Tap(action);
    }
}
