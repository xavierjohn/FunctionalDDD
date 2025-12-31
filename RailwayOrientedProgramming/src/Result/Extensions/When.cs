namespace FunctionalDdd;

using System.Diagnostics;

/// <summary>
/// Conditionally executes an operation based on a predicate, maintaining the railway.
/// Useful for executing operations only when certain conditions are met without breaking the Result chain.
/// </summary>
/// <example>
/// <code>
/// // Apply discount only for premium users
/// var result = order
///     .When(o => o.IsPremiumUser, o => ApplyDiscount(o))
///     .Bind(o => ProcessPayment(o));
/// 
/// // Validate only if amount exceeds threshold
/// await GetTransactionAsync(id)
///     .WhenAsync(
///         t => t.Amount > 10000,
///         async (t, ct) => await PerformAdditionalValidationAsync(t, ct),
///         cancellationToken
///     );
/// </code>
/// </example>
public static class WhenExtensions
{
    /// <summary>
    /// Conditionally executes an operation if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to test.</param>
    /// <param name="predicate">Predicate function to test the value.</param>
    /// <param name="operation">Operation to execute if predicate is true.</param>
    /// <returns>Result from the operation if predicate is true and result is success; otherwise the original result.</returns>
    public static Result<T> When<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Func<T, Result<T>> operation)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure) return result;
        
        if (predicate(result.Value))
        {
            return operation(result.Value);
        }
        
        return result;
    }

    /// <summary>
    /// Conditionally executes an operation if the condition is true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to test.</param>
    /// <param name="condition">Boolean condition.</param>
    /// <param name="operation">Operation to execute if condition is true.</param>
    /// <returns>Result from the operation if condition is true and result is success; otherwise the original result.</returns>
    public static Result<T> When<T>(
        this Result<T> result,
        bool condition,
        Func<T, Result<T>> operation)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure) return result;
        
        if (condition)
        {
            return operation(result.Value);
        }
        
        return result;
    }

    /// <summary>
    /// Conditionally executes an operation if the predicate returns false.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to test.</param>
    /// <param name="predicate">Predicate function to test the value.</param>
    /// <param name="operation">Operation to execute if predicate is false.</param>
    /// <returns>Result from the operation if predicate is false and result is success; otherwise the original result.</returns>
    public static Result<T> Unless<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Func<T, Result<T>> operation)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure) return result;
        
        if (!predicate(result.Value))
        {
            return operation(result.Value);
        }
        
        return result;
    }

    /// <summary>
    /// Conditionally executes an operation if the condition is false.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to test.</param>
    /// <param name="condition">Boolean condition.</param>
    /// <param name="operation">Operation to execute if condition is false.</param>
    /// <returns>Result from the operation if condition is false and result is success; otherwise the original result.</returns>
    public static Result<T> Unless<T>(
        this Result<T> result,
        bool condition,
        Func<T, Result<T>> operation)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure) return result;
        
        if (!condition)
        {
            return operation(result.Value);
        }
        
        return result;
    }
}

/// <summary>
/// Asynchronous conditional execution operations for Result.
/// </summary>
public static class WhenExtensionsAsync
{
    /// <summary>
    /// Conditionally executes an async operation if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to test.</param>
    /// <param name="predicate">Predicate function to test the value.</param>
    /// <param name="operation">Async operation to execute if predicate is true.</param>
    /// <returns>Result from the operation if predicate is true and result is success; otherwise the original result.</returns>
    public static async Task<Result<T>> WhenAsync<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Func<T, Task<Result<T>>> operation)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure) return result;
        
        if (predicate(result.Value))
        {
            return await operation(result.Value).ConfigureAwait(false);
        }
        
        return result;
    }

    /// <summary>
    /// Conditionally executes an async operation if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to test.</param>
    /// <param name="predicate">Predicate function to test the value.</param>
    /// <param name="operation">Async operation to execute if predicate is true.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>Result from the operation if predicate is true and result is success; otherwise the original result.</returns>
    public static async Task<Result<T>> WhenAsync<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Func<T, CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure) return result;
        
        if (predicate(result.Value))
        {
            return await operation(result.Value, cancellationToken).ConfigureAwait(false);
        }
        
        return result;
    }

    /// <summary>
    /// Conditionally executes an async operation if the condition is true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to test.</param>
    /// <param name="condition">Boolean condition.</param>
    /// <param name="operation">Async operation to execute if condition is true.</param>
    /// <returns>Result from the operation if condition is true and result is success; otherwise the original result.</returns>
    public static async Task<Result<T>> WhenAsync<T>(
        this Result<T> result,
        bool condition,
        Func<T, Task<Result<T>>> operation)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure) return result;
        
        if (condition)
        {
            return await operation(result.Value).ConfigureAwait(false);
        }
        
        return result;
    }

    /// <summary>
    /// Conditionally executes an async operation if the condition is true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to test.</param>
    /// <param name="condition">Boolean condition.</param>
    /// <param name="operation">Async operation to execute if condition is true.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>Result from the operation if condition is true and result is success; otherwise the original result.</returns>
    public static async Task<Result<T>> WhenAsync<T>(
        this Result<T> result,
        bool condition,
        Func<T, CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure) return result;
        
        if (condition)
        {
            return await operation(result.Value, cancellationToken).ConfigureAwait(false);
        }
        
        return result;
    }

    /// <summary>
    /// Conditionally executes an async operation if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to test.</param>
    /// <param name="predicate">Predicate function to test the value.</param>
    /// <param name="operation">Async operation to execute if predicate is true.</param>
    /// <returns>Result from the operation if predicate is true and result is success; otherwise the original result.</returns>
    public static async Task<Result<T>> WhenAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, bool> predicate,
        Func<T, Task<Result<T>>> operation)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.WhenAsync(predicate, operation).ConfigureAwait(false);
    }

    /// <summary>
    /// Conditionally executes an async operation if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to test.</param>
    /// <param name="predicate">Predicate function to test the value.</param>
    /// <param name="operation">Async operation to execute if predicate is true.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>Result from the operation if predicate is true and result is success; otherwise the original result.</returns>
    public static async Task<Result<T>> WhenAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, bool> predicate,
        Func<T, CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.WhenAsync(predicate, operation, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Conditionally executes an async operation if the condition is true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to test.</param>
    /// <param name="condition">Boolean condition.</param>
    /// <param name="operation">Async operation to execute if condition is true.</param>
    /// <returns>Result from the operation if condition is true and result is success; otherwise the original result.</returns>
    public static async Task<Result<T>> WhenAsync<T>(
        this Task<Result<T>> resultTask,
        bool condition,
        Func<T, Task<Result<T>>> operation)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.WhenAsync(condition, operation).ConfigureAwait(false);
    }

    /// <summary>
    /// Conditionally executes an async operation if the condition is true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to test.</param>
    /// <param name="condition">Boolean condition.</param>
    /// <param name="operation">Async operation to execute if condition is true.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>Result from the operation if condition is true and result is success; otherwise the original result.</returns>
    public static async Task<Result<T>> WhenAsync<T>(
        this Task<Result<T>> resultTask,
        bool condition,
        Func<T, CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.WhenAsync(condition, operation, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Conditionally executes an async operation if the predicate returns false.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to test.</param>
    /// <param name="predicate">Predicate function to test the value.</param>
    /// <param name="operation">Async operation to execute if predicate is false.</param>
    /// <returns>Result from the operation if predicate is false and result is success; otherwise the original result.</returns>
    public static async Task<Result<T>> UnlessAsync<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Func<T, Task<Result<T>>> operation)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure) return result;
        
        if (!predicate(result.Value))
        {
            return await operation(result.Value).ConfigureAwait(false);
        }
        
        return result;
    }

    /// <summary>
    /// Conditionally executes an async operation if the predicate returns false.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to test.</param>
    /// <param name="predicate">Predicate function to test the value.</param>
    /// <param name="operation">Async operation to execute if predicate is false.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>Result from the operation if predicate is false and result is success; otherwise the original result.</returns>
    public static async Task<Result<T>> UnlessAsync<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Func<T, CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure) return result;
        
        if (!predicate(result.Value))
        {
            return await operation(result.Value, cancellationToken).ConfigureAwait(false);
        }
        
        return result;
    }

    /// <summary>
    /// Conditionally executes an async operation if the condition is false.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to test.</param>
    /// <param name="condition">Boolean condition.</param>
    /// <param name="operation">Async operation to execute if condition is false.</param>
    /// <returns>Result from the operation if condition is false and result is success; otherwise the original result.</returns>
    public static async Task<Result<T>> UnlessAsync<T>(
        this Result<T> result,
        bool condition,
        Func<T, Task<Result<T>>> operation)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure) return result;
        
        if (!condition)
        {
            return await operation(result.Value).ConfigureAwait(false);
        }
        
        return result;
    }

    /// <summary>
    /// Conditionally executes an async operation if the condition is false.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="result">The result to test.</param>
    /// <param name="condition">Boolean condition.</param>
    /// <param name="operation">Async operation to execute if condition is false.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>Result from the operation if condition is false and result is success; otherwise the original result.</returns>
    public static async Task<Result<T>> UnlessAsync<T>(
        this Result<T> result,
        bool condition,
        Func<T, CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure) return result;
        
        if (!condition)
        {
            return await operation(result.Value, cancellationToken).ConfigureAwait(false);
        }
        
        return result;
    }

    /// <summary>
    /// Conditionally executes an async operation if the predicate returns false.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to test.</param>
    /// <param name="predicate">Predicate function to test the value.</param>
    /// <param name="operation">Async operation to execute if predicate is false.</param>
    /// <returns>Result from the operation if predicate is false and result is success; otherwise the original result.</returns>
    public static async Task<Result<T>> UnlessAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, bool> predicate,
        Func<T, Task<Result<T>>> operation)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.UnlessAsync(predicate, operation).ConfigureAwait(false);
    }

    /// <summary>
    /// Conditionally executes an async operation if the predicate returns false.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to test.</param>
    /// <param name="predicate">Predicate function to test the value.</param>
    /// <param name="operation">Async operation to execute if predicate is false.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>Result from the operation if predicate is false and result is success; otherwise the original result.</returns>
    public static async Task<Result<T>> UnlessAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, bool> predicate,
        Func<T, CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.UnlessAsync(predicate, operation, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Conditionally executes an async operation if the condition is false.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to test.</param>
    /// <param name="condition">Boolean condition.</param>
    /// <param name="operation">Async operation to execute if condition is false.</param>
    /// <returns>Result from the operation if condition is false and result is success; otherwise the original result.</returns>
    public static async Task<Result<T>> UnlessAsync<T>(
        this Task<Result<T>> resultTask,
        bool condition,
        Func<T, Task<Result<T>>> operation)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.UnlessAsync(condition, operation).ConfigureAwait(false);
    }

    /// <summary>
    /// Conditionally executes an async operation if the condition is false.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">Task containing the result to test.</param>
    /// <param name="condition">Boolean condition.</param>
    /// <param name="operation">Async operation to execute if condition is false.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>Result from the operation if condition is false and result is success; otherwise the original result.</returns>
    public static async Task<Result<T>> UnlessAsync<T>(
        this Task<Result<T>> resultTask,
        bool condition,
        Func<T, CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return await result.UnlessAsync(condition, operation, cancellationToken).ConfigureAwait(false);
    }

    // ValueTask overloads for zero-allocation scenarios
    
    /// <summary>
    /// Conditionally executes an async operation if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">ValueTask containing the result to test.</param>
    /// <param name="predicate">Predicate function to test the value.</param>
    /// <param name="operation">Async operation to execute if predicate is true.</param>
    /// <returns>Result from the operation if predicate is true and result is success; otherwise the original result.</returns>
    public static async ValueTask<Result<T>> WhenAsync<T>(
        this ValueTask<Result<T>> resultTask,
        Func<T, bool> predicate,
        Func<T, ValueTask<Result<T>>> operation)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure) return result;
        
        if (predicate(result.Value))
        {
            return await operation(result.Value).ConfigureAwait(false);
        }
        
        return result;
    }

    /// <summary>
    /// Conditionally executes an async operation if the predicate returns true.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">ValueTask containing the result to test.</param>
    /// <param name="predicate">Predicate function to test the value.</param>
    /// <param name="operation">Async operation to execute if predicate is true.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>Result from the operation if predicate is true and result is success; otherwise the original result.</returns>
    public static async ValueTask<Result<T>> WhenAsync<T>(
        this ValueTask<Result<T>> resultTask,
        Func<T, bool> predicate,
        Func<T, CancellationToken, ValueTask<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure) return result;
        
        if (predicate(result.Value))
        {
            return await operation(result.Value, cancellationToken).ConfigureAwait(false);
        }
        
        return result;
    }

    /// <summary>
    /// Conditionally executes an async operation if the predicate returns false.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">ValueTask containing the result to test.</param>
    /// <param name="predicate">Predicate function to test the value.</param>
    /// <param name="operation">Async operation to execute if predicate is false.</param>
    /// <returns>Result from the operation if predicate is false and result is success; otherwise the original result.</returns>
    public static async ValueTask<Result<T>> UnlessAsync<T>(
        this ValueTask<Result<T>> resultTask,
        Func<T, bool> predicate,
        Func<T, ValueTask<Result<T>>> operation)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure) return result;
        
        if (!predicate(result.Value))
        {
            return await operation(result.Value).ConfigureAwait(false);
        }
        
        return result;
    }

    /// <summary>
    /// Conditionally executes an async operation if the predicate returns false.
    /// </summary>
    /// <typeparam name="T">Type of the result value.</typeparam>
    /// <param name="resultTask">ValueTask containing the result to test.</param>
    /// <param name="predicate">Predicate function to test the value.</param>
    /// <param name="operation">Async operation to execute if predicate is false.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>Result from the operation if predicate is false and result is success; otherwise the original result.</returns>
    public static async ValueTask<Result<T>> UnlessAsync<T>(
        this ValueTask<Result<T>> resultTask,
        Func<T, bool> predicate,
        Func<T, CancellationToken, ValueTask<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        using var activity = RopTrace.ActivitySource.StartActivity();
        if (result.IsFailure) return result;
        
        if (!predicate(result.Value))
        {
            return await operation(result.Value, cancellationToken).ConfigureAwait(false);
        }
        
        return result;
    }
}
