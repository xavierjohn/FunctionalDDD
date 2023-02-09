namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<TOk, Error> IfOkTap<TOk>(this Result<TOk, Error> result, Action action)
    {
        if (result.IsOk)
            action();

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<TOk, Error> IfOkTap<TOk>(this Result<TOk, Error> result, Action<TOk> action)
    {
        if (result.IsOk)
            action(result.Ok);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async Task<Result<TOk, Error>> OnOkTapAsync<TOk>(this Task<Result<TOk, Error>> resultTask, Action<TOk> action)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);
        return result.IfOkTap(action);
    }
}
