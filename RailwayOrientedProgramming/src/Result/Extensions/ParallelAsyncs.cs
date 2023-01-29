
// Generated code
namespace FunctionalDDD;

public static partial class ResultExtensions
{

    public static (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>) ParallelAsync<T1, T2, T3>(
        this (Task<Result<T1, Err>>, Task<Result<T2, Err>>) tasks,
        Task<Result<T3, Err>> task
        ) => (tasks.Item1, tasks.Item2, task);

    public static async Task<Result<TResult, Err>> BindAsync<T1, T2, T3, TResult>(
        this (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>) tasks,
        Func<T1, T2, T3, Result<TResult, Err>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3);
        return tasks.Item1.Result.Combine(tasks.Item2.Result, tasks.Item3.Result).Bind(func);
    }

    public static (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>) ParallelAsync<T1, T2, T3, T4>(
        this (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>) tasks,
        Task<Result<T4, Err>> task
        ) => (tasks.Item1, tasks.Item2, tasks.Item3, task);

    public static async Task<Result<TResult, Err>> BindAsync<T1, T2, T3, T4, TResult>(
        this (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>) tasks,
        Func<T1, T2, T3, T4, Result<TResult, Err>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4);
        return tasks.Item1.Result.Combine(tasks.Item2.Result, tasks.Item3.Result, tasks.Item4.Result).Bind(func);
    }

    public static (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>, Task<Result<T5, Err>>) ParallelAsync<T1, T2, T3, T4, T5>(
        this (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>) tasks,
        Task<Result<T5, Err>> task
        ) => (tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, task);

    public static async Task<Result<TResult, Err>> BindAsync<T1, T2, T3, T4, T5, TResult>(
        this (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>, Task<Result<T5, Err>>) tasks,
        Func<T1, T2, T3, T4, T5, Result<TResult, Err>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5);
        return tasks.Item1.Result.Combine(tasks.Item2.Result, tasks.Item3.Result, tasks.Item4.Result, tasks.Item5.Result).Bind(func);
    }

    public static (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>, Task<Result<T5, Err>>, Task<Result<T6, Err>>) ParallelAsync<T1, T2, T3, T4, T5, T6>(
        this (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>, Task<Result<T5, Err>>) tasks,
        Task<Result<T6, Err>> task
        ) => (tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, task);

    public static async Task<Result<TResult, Err>> BindAsync<T1, T2, T3, T4, T5, T6, TResult>(
        this (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>, Task<Result<T5, Err>>, Task<Result<T6, Err>>) tasks,
        Func<T1, T2, T3, T4, T5, T6, Result<TResult, Err>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6);
        return tasks.Item1.Result.Combine(tasks.Item2.Result, tasks.Item3.Result, tasks.Item4.Result, tasks.Item5.Result, tasks.Item6.Result).Bind(func);
    }

    public static (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>, Task<Result<T5, Err>>, Task<Result<T6, Err>>, Task<Result<T7, Err>>) ParallelAsync<T1, T2, T3, T4, T5, T6, T7>(
        this (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>, Task<Result<T5, Err>>, Task<Result<T6, Err>>) tasks,
        Task<Result<T7, Err>> task
        ) => (tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6, task);

    public static async Task<Result<TResult, Err>> BindAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(
        this (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>, Task<Result<T5, Err>>, Task<Result<T6, Err>>, Task<Result<T7, Err>>) tasks,
        Func<T1, T2, T3, T4, T5, T6, T7, Result<TResult, Err>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6, tasks.Item7);
        return tasks.Item1.Result.Combine(tasks.Item2.Result, tasks.Item3.Result, tasks.Item4.Result, tasks.Item5.Result, tasks.Item6.Result, tasks.Item7.Result).Bind(func);
    }

    public static (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>, Task<Result<T5, Err>>, Task<Result<T6, Err>>, Task<Result<T7, Err>>, Task<Result<T8, Err>>) ParallelAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
        this (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>, Task<Result<T5, Err>>, Task<Result<T6, Err>>, Task<Result<T7, Err>>) tasks,
        Task<Result<T8, Err>> task
        ) => (tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6, tasks.Item7, task);

    public static async Task<Result<TResult, Err>> BindAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(
        this (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>, Task<Result<T5, Err>>, Task<Result<T6, Err>>, Task<Result<T7, Err>>, Task<Result<T8, Err>>) tasks,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, Result<TResult, Err>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6, tasks.Item7, tasks.Item8);
        return tasks.Item1.Result.Combine(tasks.Item2.Result, tasks.Item3.Result, tasks.Item4.Result, tasks.Item5.Result, tasks.Item6.Result, tasks.Item7.Result, tasks.Item8.Result).Bind(func);
    }

    public static (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>, Task<Result<T5, Err>>, Task<Result<T6, Err>>, Task<Result<T7, Err>>, Task<Result<T8, Err>>, Task<Result<T9, Err>>) ParallelAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        this (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>, Task<Result<T5, Err>>, Task<Result<T6, Err>>, Task<Result<T7, Err>>, Task<Result<T8, Err>>) tasks,
        Task<Result<T9, Err>> task
        ) => (tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6, tasks.Item7, tasks.Item8, task);

    public static async Task<Result<TResult, Err>> BindAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(
        this (Task<Result<T1, Err>>, Task<Result<T2, Err>>, Task<Result<T3, Err>>, Task<Result<T4, Err>>, Task<Result<T5, Err>>, Task<Result<T6, Err>>, Task<Result<T7, Err>>, Task<Result<T8, Err>>, Task<Result<T9, Err>>) tasks,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Result<TResult, Err>> func)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6, tasks.Item7, tasks.Item8, tasks.Item9);
        return tasks.Item1.Result.Combine(tasks.Item2.Result, tasks.Item3.Result, tasks.Item4.Result, tasks.Item5.Result, tasks.Item6.Result, tasks.Item7.Result, tasks.Item8.Result, tasks.Item9.Result).Bind(func);
    }

}
