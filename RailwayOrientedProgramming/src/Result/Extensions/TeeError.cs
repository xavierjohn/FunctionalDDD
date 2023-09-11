namespace FunctionalDDD.RailwayOrientedProgramming;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<TOk> TeeError<TOk>(this Result<TOk> result, Action action)
    {
        if (result.IsFailure)
            action();

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<TOk> TeeError<TOk>(this Result<TOk> result, Action<Error> action)
    {
        if (result.IsFailure)
            action(result.Error);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async Task<Result<TOk>> OnErrorTapAsync<TOk>(this Task<Result<TOk>> resultTask, Action<Error> action)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);
        return result.TeeError(action);
    }
}
