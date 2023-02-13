namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    /// Compensate for failed result by calling the given function.
    /// </summary>
    public static Result<T, Error> Compensate<T>(this Result<T, Error> result, Func<Result<T, Error>> func)
    {
        if (result.IsSuccess)
            return result;

        return func();
    }

    /// <summary>
    /// Compensate for failed result by calling the given function.
    /// </summary>
    public static async Task<Result<T, Error>> OnErrorAsync<T>(this Task<Result<T, Error>> resultTask, Func<Result<T, Error>> func)
    {
        Result<T, Error> result = await resultTask.ConfigureAwait(false);
        return result.Compensate(func);
    }

    /// <summary>
    /// Compensate for failed result by calling the given function.
    /// </summary>
    public static async Task<Result<T, Error>> OnErrorAsync<T>(this Task<Result<T, Error>> resultTask, Func<Task<Result<T, Error>>> funcAsync)
    {
        Result<T, Error> result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess)
            return result;

        return await funcAsync();
    }
}
