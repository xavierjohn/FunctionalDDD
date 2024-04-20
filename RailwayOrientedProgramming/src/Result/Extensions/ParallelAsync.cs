namespace FunctionalDdd;

/// <summary>
/// Execute tasks in parallel and return a <see cref="Tuple"/> of <see cref="Result{TValue}"/> tasks.
/// </summary>
public static partial class ParallelExtensionsAsync
{
    /// <summary>
    /// Execute two tasks in parallel and return a tuple of <see cref="Result{TValue}"/> tasks.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="resultTask1"></param>
    /// <param name="resultTask2"></param>
    /// <returns></returns>
    public static (Task<Result<T1>>, Task<Result<T2>>) ParallelAsync<T1, T2>(this Task<Result<T1>> resultTask1, Task<Result<T2>> resultTask2)
        => (resultTask1, resultTask2);

    /// <summary>
    /// Execute two tasks in parallel and return a tuple of <see cref="Result{TValue}"/> tasks.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="resultTask1"></param>
    /// <param name="resultTask2"></param>
    /// <returns></returns>
    public static (ValueTask<Result<T1>>, ValueTask<Result<T2>>) ParallelAsync<T1, T2>(this ValueTask<Result<T1>> resultTask1, ValueTask<Result<T2>> resultTask2)
        => (resultTask1, resultTask2);

}
