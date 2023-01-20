namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    /// Compensate for failure by calling the given function with the errors.
    /// </summary>
    public static Result<T> Compensate<T>(this Result<T> result, Func<ErrorList, Result<T>> func)
    {
        if (result.IsSuccess)
            return result;

        return func(result.Errors);
    }

    /// <summary>
    /// Compensate for failure by calling the given function with the errors.
    /// </summary>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<ErrorList, Result<T>> func)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return result.Compensate(func);
    }
}
