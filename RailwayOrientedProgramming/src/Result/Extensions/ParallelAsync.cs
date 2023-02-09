namespace FunctionalDDD;

public static partial class ResultExtensions
{
    public static (Task<Result<T1, Error>>, Task<Result<T2, Error>>) ParallelAsync<T1, T2>(this Task<Result<T1, Error>> resultTask1, Task<Result<T2, Error>> resultTask2)
        => (resultTask1, resultTask2);

    public static async Task<Result<TResult, Error>> OnOkAsync<T1, T2, TResult>(
        this (Task<Result<T1, Error>>, Task<Result<T2, Error>>) tasks,
        Func<T1, T2, Result<TResult, Error>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2);
        return tasks.Item1.Result.Combine(tasks.Item2.Result).OnOk(func);
    }
}
