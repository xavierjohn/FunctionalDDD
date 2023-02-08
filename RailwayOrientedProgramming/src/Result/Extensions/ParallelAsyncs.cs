
// Generated code
namespace FunctionalDDD;

public static partial class ResultExtensions
{

    public static (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>) ParallelAsync<T1, T2, T3>(
        this (Task<Result<T1, Error>>, Task<Result<T2, Error>>) tasks,
        Task<Result<T3, Error>> task
        ) => (tasks.Item1, tasks.Item2, task);

    public static async Task<Result<TResult, Error>> OnOkAsync<T1, T2, T3, TResult>(
        this (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>) tasks,
        Func<T1, T2, T3, Result<TResult, Error>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3);
        return tasks.Item1.Result.Combine(tasks.Item2.Result, tasks.Item3.Result).OnOk(func);
    }

    public static (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>) ParallelAsync<T1, T2, T3, T4>(
        this (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>) tasks,
        Task<Result<T4, Error>> task
        ) => (tasks.Item1, tasks.Item2, tasks.Item3, task);

    public static async Task<Result<TResult, Error>> OnOkAsync<T1, T2, T3, T4, TResult>(
        this (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>) tasks,
        Func<T1, T2, T3, T4, Result<TResult, Error>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4);
        return tasks.Item1.Result.Combine(tasks.Item2.Result, tasks.Item3.Result, tasks.Item4.Result).OnOk(func);
    }

    public static (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>, Task<Result<T5, Error>>) ParallelAsync<T1, T2, T3, T4, T5>(
        this (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>) tasks,
        Task<Result<T5, Error>> task
        ) => (tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, task);

    public static async Task<Result<TResult, Error>> OnOkAsync<T1, T2, T3, T4, T5, TResult>(
        this (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>, Task<Result<T5, Error>>) tasks,
        Func<T1, T2, T3, T4, T5, Result<TResult, Error>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5);
        return tasks.Item1.Result.Combine(tasks.Item2.Result, tasks.Item3.Result, tasks.Item4.Result, tasks.Item5.Result).OnOk(func);
    }

    public static (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>, Task<Result<T5, Error>>, Task<Result<T6, Error>>) ParallelAsync<T1, T2, T3, T4, T5, T6>(
        this (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>, Task<Result<T5, Error>>) tasks,
        Task<Result<T6, Error>> task
        ) => (tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, task);

    public static async Task<Result<TResult, Error>> OnOkAsync<T1, T2, T3, T4, T5, T6, TResult>(
        this (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>, Task<Result<T5, Error>>, Task<Result<T6, Error>>) tasks,
        Func<T1, T2, T3, T4, T5, T6, Result<TResult, Error>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6);
        return tasks.Item1.Result.Combine(tasks.Item2.Result, tasks.Item3.Result, tasks.Item4.Result, tasks.Item5.Result, tasks.Item6.Result).OnOk(func);
    }

    public static (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>, Task<Result<T5, Error>>, Task<Result<T6, Error>>, Task<Result<T7, Error>>) ParallelAsync<T1, T2, T3, T4, T5, T6, T7>(
        this (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>, Task<Result<T5, Error>>, Task<Result<T6, Error>>) tasks,
        Task<Result<T7, Error>> task
        ) => (tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6, task);

    public static async Task<Result<TResult, Error>> OnOkAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(
        this (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>, Task<Result<T5, Error>>, Task<Result<T6, Error>>, Task<Result<T7, Error>>) tasks,
        Func<T1, T2, T3, T4, T5, T6, T7, Result<TResult, Error>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6, tasks.Item7);
        return tasks.Item1.Result.Combine(tasks.Item2.Result, tasks.Item3.Result, tasks.Item4.Result, tasks.Item5.Result, tasks.Item6.Result, tasks.Item7.Result).OnOk(func);
    }

    public static (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>, Task<Result<T5, Error>>, Task<Result<T6, Error>>, Task<Result<T7, Error>>, Task<Result<T8, Error>>) ParallelAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
        this (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>, Task<Result<T5, Error>>, Task<Result<T6, Error>>, Task<Result<T7, Error>>) tasks,
        Task<Result<T8, Error>> task
        ) => (tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6, tasks.Item7, task);

    public static async Task<Result<TResult, Error>> OnOkAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(
        this (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>, Task<Result<T5, Error>>, Task<Result<T6, Error>>, Task<Result<T7, Error>>, Task<Result<T8, Error>>) tasks,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, Result<TResult, Error>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6, tasks.Item7, tasks.Item8);
        return tasks.Item1.Result.Combine(tasks.Item2.Result, tasks.Item3.Result, tasks.Item4.Result, tasks.Item5.Result, tasks.Item6.Result, tasks.Item7.Result, tasks.Item8.Result).OnOk(func);
    }

    public static (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>, Task<Result<T5, Error>>, Task<Result<T6, Error>>, Task<Result<T7, Error>>, Task<Result<T8, Error>>, Task<Result<T9, Error>>) ParallelAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        this (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>, Task<Result<T5, Error>>, Task<Result<T6, Error>>, Task<Result<T7, Error>>, Task<Result<T8, Error>>) tasks,
        Task<Result<T9, Error>> task
        ) => (tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6, tasks.Item7, tasks.Item8, task);

    public static async Task<Result<TResult, Error>> OnOkAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(
        this (Task<Result<T1, Error>>, Task<Result<T2, Error>>, Task<Result<T3, Error>>, Task<Result<T4, Error>>, Task<Result<T5, Error>>, Task<Result<T6, Error>>, Task<Result<T7, Error>>, Task<Result<T8, Error>>, Task<Result<T9, Error>>) tasks,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Result<TResult, Error>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6, tasks.Item7, tasks.Item8, tasks.Item9);
        return tasks.Item1.Result.Combine(tasks.Item2.Result, tasks.Item3.Result, tasks.Item4.Result, tasks.Item5.Result, tasks.Item6.Result, tasks.Item7.Result, tasks.Item8.Result, tasks.Item9.Result).OnOk(func);
    }

}
