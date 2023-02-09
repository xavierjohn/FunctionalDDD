namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<TOk, Error> IfErrorTap<TOk>(this Result<TOk, Error> result, Action action)
    {
        if (result.IsError)
            action();

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<TOk, Error> IfErrorTap<TOk>(this Result<TOk, Error> result, Action<Error> action)
    {
        if (result.IsError)
            action(result.Error);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async Task<Result<TOk, Error>> OnErrorTapAsync<TOk>(this Task<Result<TOk, Error>> resultTask, Action<Error> action)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);
        return result.IfErrorTap(action);
    }
}
