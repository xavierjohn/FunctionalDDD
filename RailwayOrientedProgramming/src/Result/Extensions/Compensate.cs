namespace FunctionalDDD.RailwayOrientedProgramming;

public static class CompensateExtensions
{
    /// <summary>
    /// Compensate for failed result by calling the given function.
    /// </summary>
    public static Result<T> Compensate<T>(this Result<T> result, Func<Result<T>> func)
    {
        if (result.IsSuccess)
            return result;

        return func();
    }
}
public static class CompensateAsyncExtensions
{
    /// <summary>
    /// Compensate for failed result by calling the given function.
    /// </summary>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Result<T>> func)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return result.Compensate(func);
    }

    /// <summary>
    /// Compensate for failed result by calling the given function.
    /// </summary>
    public static async Task<Result<T>> CompensateAsync<T>(this Task<Result<T>> resultTask, Func<Task<Result<T>>> funcAsync)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess)
            return result;

        return await funcAsync();
    }
}
