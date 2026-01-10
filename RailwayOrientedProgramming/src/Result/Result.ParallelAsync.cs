namespace FunctionalDdd;

using System;
using System.Threading.Tasks;

/// <summary>
/// Partial Result struct containing ParallelAsync static methods for guaranteed parallel execution.
/// </summary>
public readonly partial struct Result
{
    #region ParallelAsync - Static Methods for True Parallel Execution

    /// <summary>
    /// Starts two async operations in parallel and returns a tuple of tasks.
    /// All operations start immediately and execute concurrently.
    /// </summary>
    /// <typeparam name="T1">The type of the result value from the first operation.</typeparam>
    /// <typeparam name="T2">The type of the result value from the second operation.</typeparam>
    /// <param name="taskFactory1">Factory function that creates and starts the first task.</param>
    /// <param name="taskFactory2">Factory function that creates and starts the second task.</param>
    /// <returns>A tuple of 2 tasks that can be awaited using AwaitAsync.</returns>
    /// <remarks>
    /// <para>
    /// This method invokes both factory functions immediately to start both tasks in parallel.
    /// The factory pattern prevents sequential evaluation that occurs when passing tasks directly.
    /// </para>
    /// <para>
    /// All tasks execute concurrently from the moment they are created.
    /// ParallelAsync builds the tuple structure without awaiting any tasks.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Start 2 operations in parallel
    /// var result = await Result.ParallelAsync(
    ///     () => GetUserAsync(userId),
    ///     () => GetOrdersAsync(userId)
    /// ).AwaitAsync();
    /// 
    /// // Both tasks start immediately and run in parallel
    /// </code>
    /// </example>
    public static (Task<Result<T1>>, Task<Result<T2>>) ParallelAsync<T1, T2>(
        Func<Task<Result<T1>>> taskFactory1,
        Func<Task<Result<T2>>> taskFactory2)
    {
        var task1 = taskFactory1();
        var task2 = taskFactory2();
        return (task1, task2);
    }

    /// <summary>
    /// Starts three async operations in parallel and returns a tuple of tasks.
    /// All operations start immediately and execute concurrently.
    /// </summary>
    /// <typeparam name="T1">The type of the result value from the first operation.</typeparam>
    /// <typeparam name="T2">The type of the result value from the second operation.</typeparam>
    /// <typeparam name="T3">The type of the result value from the third operation.</typeparam>
    /// <param name="taskFactory1">Factory function that creates and starts the first task.</param>
    /// <param name="taskFactory2">Factory function that creates and starts the second task.</param>
    /// <param name="taskFactory3">Factory function that creates and starts the third task.</param>
    /// <returns>A tuple of 3 tasks that can be awaited using AwaitAsync.</returns>
    /// <example>
    /// <code>
    /// var result = await Result.ParallelAsync(
    ///     () => GetUserAsync(userId),
    ///     () => GetOrdersAsync(userId),
    ///     () => GetPreferencesAsync(userId)
    /// ).AwaitAsync();
    /// </code>
    /// </example>
    public static (Task<Result<T1>>, Task<Result<T2>>, Task<Result<T3>>) ParallelAsync<T1, T2, T3>(
        Func<Task<Result<T1>>> taskFactory1,
        Func<Task<Result<T2>>> taskFactory2,
        Func<Task<Result<T3>>> taskFactory3)
    {
        var task1 = taskFactory1();
        var task2 = taskFactory2();
        var task3 = taskFactory3();
        return (task1, task2, task3);
    }

    /// <summary>
    /// Starts four async operations in parallel and returns a tuple of tasks.
    /// All operations start immediately and execute concurrently.
    /// </summary>
    /// <typeparam name="T1">The type of the result value from the first operation.</typeparam>
    /// <typeparam name="T2">The type of the result value from the second operation.</typeparam>
    /// <typeparam name="T3">The type of the result value from the third operation.</typeparam>
    /// <typeparam name="T4">The type of the result value from the fourth operation.</typeparam>
    /// <param name="taskFactory1">Factory function that creates and starts the first task.</param>
    /// <param name="taskFactory2">Factory function that creates and starts the second task.</param>
    /// <param name="taskFactory3">Factory function that creates and starts the third task.</param>
    /// <param name="taskFactory4">Factory function that creates and starts the fourth task.</param>
    /// <returns>A tuple of 4 tasks that can be awaited using AwaitAsync.</returns>
    /// <example>
    /// <code>
    /// var result = await Result.ParallelAsync(
    ///     () => GetData1Async(),
    ///     () => GetData2Async(),
    ///     () => GetData3Async(),
    ///     () => GetData4Async()
    /// ).AwaitAsync();
    /// </code>
    /// </example>
    public static (Task<Result<T1>>, Task<Result<T2>>, Task<Result<T3>>, Task<Result<T4>>) ParallelAsync<T1, T2, T3, T4>(
        Func<Task<Result<T1>>> taskFactory1,
        Func<Task<Result<T2>>> taskFactory2,
        Func<Task<Result<T3>>> taskFactory3,
        Func<Task<Result<T4>>> taskFactory4)
    {
        var task1 = taskFactory1();
        var task2 = taskFactory2();
        var task3 = taskFactory3();
        var task4 = taskFactory4();
        return (task1, task2, task3, task4);
    }

    #endregion
}
