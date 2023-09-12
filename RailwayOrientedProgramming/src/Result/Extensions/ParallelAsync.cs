namespace FunctionalDDD.RailwayOrientedProgramming;

public static partial class ParallelExtensions
{
    public static (Task<Result<T1>>, Task<Result<T2>>) ParallelAsync<T1, T2>(this Task<Result<T1>> resultTask1, Task<Result<T2>> resultTask2)
        => (resultTask1, resultTask2);

    public static async Task<Result<TResult>> BindAsync<T1, T2, TResult>(
        this (Task<Result<T1>>, Task<Result<T2>>) tasks,
        Func<T1, T2, Result<TResult>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2);
        return tasks.Item1.Result.Combine(tasks.Item2.Result).Bind(func);
    }
}
