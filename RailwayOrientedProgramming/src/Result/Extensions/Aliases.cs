namespace FunctionalDdd.Aliases;

/// <summary>
/// Provides alias extension methods for developers more familiar with C#/LINQ naming conventions.
/// These are syntactic sugar - they call the standard Railway Oriented Programming methods.
/// </summary>
/// <remarks>
/// <para>
/// This namespace is OPTIONAL. The standard FunctionalDdd namespace provides the canonical
/// Railway Oriented Programming API (Bind, Map, Tap, etc.).
/// </para>
/// <para>
/// These aliases are provided for developers transitioning from imperative C# or LINQ patterns.
/// We recommend learning the standard ROP terminology for better alignment with functional
/// programming literature and other FP languages (F#, Haskell, Scala, Rust).
/// </para>
/// <para>
/// Example usage:
/// <code>
/// using FunctionalDdd.Aliases; // Opt-in to aliases
/// 
/// var result = GetUser(id)
///     .Then(user => ValidateUser(user))    // Alias for Bind
///     .OrElse(() => GetDefaultUser())      // Alias for Compensate
///     .Peek(user => Log(user));            // Alias for Tap
/// </code>
/// </para>
/// </remarks>
public static class ResultAliases
{
    /// <summary>
    /// Alias for <see cref="BindExtensions.Bind{TValue, TResult}"/>.
    /// Chains an operation that can fail. If current result is success, calls the function.
    /// </summary>
    /// <typeparam name="TIn">Type of the input result value.</typeparam>
    /// <typeparam name="TOut">Type of the output result value.</typeparam>
    /// <param name="result">The result to bind.</param>
    /// <param name="func">The function to call if the result is successful.</param>
    /// <returns>A new result from the function if success; otherwise the original failure.</returns>
    /// <remarks>
    /// This is an alias for developers familiar with promise/continuation patterns.
    /// In functional programming, this operation is called "Bind" or "FlatMap".
    /// </remarks>
    public static Result<TOut> Then<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> func)
        => result.Bind(func);

    /// <summary>
    /// Alias for <see cref="TapExtensions.Tap{TValue}(Result{TValue}, Action{TValue})"/>.
    /// Executes a side effect on success without transforming the value.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="action">The action to execute with the value if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    /// <remarks>
    /// This is an alias for developers familiar with stream processing or debugging patterns (Peek in Rx, Java Streams).
    /// In functional programming, this operation is called "Tap".
    /// The name "Peek" is used instead of "Do" to avoid conflicts with Visual Basic .NET keywords.
    /// </remarks>
    public static Result<T> Peek<T>(
        this Result<T> result,
        Action<T> action)
        => result.Tap(action);

    /// <summary>
    /// Alias for <see cref="CompensateExtensions.Compensate{T}(Result{T}, Func{Result{T}})"/>.
    /// Provides a fallback result when the current result is failure.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to compensate if it's a failure.</param>
    /// <param name="fallback">The function to call for compensation.</param>
    /// <returns>The original result if success; otherwise the result from the fallback function.</returns>
    /// <remarks>
    /// This is an alias for developers familiar with LINQ's null-coalescing or default patterns.
    /// In functional programming, this operation is called "Compensate" or "OrElse".
    /// </remarks>
    public static Result<T> OrElse<T>(
        this Result<T> result,
        Func<Result<T>> fallback)
        => result.Compensate(fallback);

    /// <summary>
    /// Alias for <see cref="CompensateExtensions.Compensate{T}(Result{T}, Func{Error, bool}, Func{Result{T}})"/>.
    /// Provides a fallback result when failure matches the predicate.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to compensate if it's a failure.</param>
    /// <param name="predicate">The predicate to test the error.</param>
    /// <param name="fallback">The function to call for compensation if the predicate is true.</param>
    /// <returns>The original result if success or predicate is false; otherwise the result from the fallback function.</returns>
    public static Result<T> OrElse<T>(
        this Result<T> result,
        Func<Error, bool> predicate,
        Func<Result<T>> fallback)
        => result.Compensate(predicate, fallback);

    /// <summary>
    /// Alias for <see cref="EnsureExtensions.Ensure{TValue}(Result{TValue}, Func{TValue, bool}, Error)"/>.
    /// Validates a condition, returning failure if false.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to validate.</param>
    /// <param name="predicate">The predicate function to test the value.</param>
    /// <param name="error">The error to return if the predicate is false.</param>
    /// <returns>The original result if success and predicate is true; otherwise a failure with the specified error.</returns>
    /// <remarks>
    /// This is an alias for developers familiar with assertion or guard clause patterns.
    /// In functional programming, this operation is called "Ensure" or "Filter".
    /// </remarks>
    public static Result<T> Require<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error error)
        => result.Ensure(predicate, error);

    // Async variants

    /// <summary>
    /// Alias for BindAsync.
    /// Async version of Then.
    /// </summary>
    /// <typeparam name="TIn">Type of the input result value.</typeparam>
    /// <typeparam name="TOut">Type of the output result value.</typeparam>
    /// <param name="result">The result to bind.</param>
    /// <param name="func">The async function to call if the result is successful.</param>
    /// <returns>A new result from the function if success; otherwise the original failure.</returns>
    public static Task<Result<TOut>> ThenAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> func)
        => result.BindAsync(func);

    /// <summary>
    /// Alias for BindAsync with Task-wrapped results.
    /// Async version of Then for Task-wrapped results.
    /// </summary>
    /// <typeparam name="TIn">Type of the input result value.</typeparam>
    /// <typeparam name="TOut">Type of the output result value.</typeparam>
    /// <param name="resultTask">The task containing the result to bind.</param>
    /// <param name="func">The async function to call if the result is successful.</param>
    /// <returns>A new result from the function if success; otherwise the original failure.</returns>
    public static Task<Result<TOut>> ThenAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> func)
        => resultTask.BindAsync(func);

    /// <summary>
    /// Alias for TapAsync.
    /// Async version of Peek.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="action">The async action to execute with the value if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    public static Task<Result<T>> PeekAsync<T>(
        this Result<T> result,
        Func<T, Task> action)
        => result.TapAsync(action);

    /// <summary>
    /// Alias for TapAsync with Task-wrapped results.
    /// Async version of Peek for Task-wrapped results.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">The task containing the result to tap.</param>
    /// <param name="action">The async action to execute with the value if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    public static Task<Result<T>> PeekAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task> action)
        => resultTask.TapAsync(action);

    /// <summary>
    /// Alias for CompensateAsync.
    /// Async version of OrElse.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to compensate if it's a failure.</param>
    /// <param name="fallbackAsync">The async function to call for compensation.</param>
    /// <returns>The original result if success; otherwise the result from the fallback function.</returns>
    public static Task<Result<T>> OrElseAsync<T>(
        this Result<T> result,
        Func<Task<Result<T>>> fallbackAsync)
        => result.CompensateAsync(fallbackAsync);

    /// <summary>
    /// Alias for EnsureAsync.
    /// Async version of Require.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to validate.</param>
    /// <param name="predicateAsync">The async predicate function to test the value.</param>
    /// <param name="error">The error to return if the predicate is false.</param>
    /// <returns>The original result if success and predicate is true; otherwise a failure with the specified error.</returns>
    public static Task<Result<T>> RequireAsync<T>(
        this Result<T> result,
        Func<T, Task<bool>> predicateAsync,
        Error error)
        => result.EnsureAsync(predicateAsync, error);
}
