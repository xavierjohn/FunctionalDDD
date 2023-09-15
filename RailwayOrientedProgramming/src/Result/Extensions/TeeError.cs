namespace FunctionalDDD.RailwayOrientedProgramming;

using FunctionalDDD.RailwayOrientedProgramming.Errors;

/// <summary>
/// Executes the given action if the calling result is a failure. Returns the calling result.
/// </summary>
public static class TeeErrorExtensions
{
    public static Result<TOk> TeeError<TOk>(this Result<TOk> result, Action action)
    {
        if (result.IsFailure)
            action();

        return result;
    }

    public static Result<TOk> TeeError<TOk>(this Result<TOk> result, Action<Error> action)
    {
        if (result.IsFailure)
            action(result.Error);

        return result;
    }

    public static async Task<Result<TOk>> TeeErrorAsync<TOk>(this Task<Result<TOk>> resultTask, Action<Error> action)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);
        return result.TeeError(action);
    }
}
