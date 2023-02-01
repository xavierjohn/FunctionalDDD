namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    /// Compensate for failed result by calling the given function.
    /// </summary>
    public static Result<T, Err> BindError<T>(this Result<T, Err> result, Func<Result<T, Err>> func)
    {
        if (result.IsSuccess)
            return result;

        return func();
    }

    /// <summary>
    /// Compensate for failed result by calling the given function.
    /// </summary>
    public static async Task<Result<T, Err>> BindErrorAsync<T>(this Task<Result<T, Err>> resultTask, Func<Result<T, Err>> func)
    {
        Result<T, Err> result = await resultTask.ConfigureAwait(false);
        return result.BindError(func);
    }

    /// <summary>
    /// Compensate for failed result by calling the given function.
    /// </summary>
    public static async Task<Result<T, Err>> BindErrorAsync<T>(this Task<Result<T, Err>> resultTask, Func<Task<Result<T, Err>>> funcAsync)
    {
        Result<T, Err> result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess)
            return result;

        return await funcAsync();
    }
}
