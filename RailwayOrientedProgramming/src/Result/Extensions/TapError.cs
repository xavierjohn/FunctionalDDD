namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<TOk, Err> TapError<TOk>(this Result<TOk, Err> result, Action action)
    {
        if (result.IsFailure)
            action();

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<TOk, Err> TapError<TOk>(this Result<TOk, Err> result, Action<Errs<Err>> action)
    {
        if (result.IsFailure)
            action(result.Errs);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async Task<Result<TOk, Err>> TapErrorAsync<TOk>(this Task<Result<TOk, Err>> resultTask, Action<Errs<Err>> action)
    {
        Result<TOk, Err> result = await resultTask.ConfigureAwait(false);
        return result.TapError(action);
    }
}
