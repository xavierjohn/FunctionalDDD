namespace FunctionalDDD;

public static partial class ResultExtensions
{
    public static (Task<Result<T1, Err>>, Task<Result<T2, Err>>) ParallelAsync<T1, T2>(this Task<Result<T1, Err>> resultTask1, Task<Result<T2, Err>> resultTask2)
        => (resultTask1, resultTask2);

    public static async Task<Result<TResult, Err>> BindAsync<T1, T2, TResult>(
        this (Task<Result<T1, Err>>, Task<Result<T2, Err>>) tasks,
        Func<T1, T2, Result<TResult, Err>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2);
        return tasks.Item1.Result.Combine(tasks.Item2.Result).Bind(func);
    }
}
