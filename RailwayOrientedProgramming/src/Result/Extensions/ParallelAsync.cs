namespace FunctionalDDD;

public static partial class ResultExtensions
{
    public static (Task<Result<T1>>, Task<Result<T2>>) ParallelAsync<T1, T2>(this Task<Result<T1>> resultTask1, Task<Result<T2>> resultTask2)
        => (resultTask1, resultTask2);

    public static async Task<Result<(T1, T2)>> ParallelWhenAllAsync<T1, T2>(this (Task<Result<T1>>, Task<Result<T2>>) tasks)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2);
        return tasks.Item1.Result.Combine(tasks.Item2.Result);
    }
}
