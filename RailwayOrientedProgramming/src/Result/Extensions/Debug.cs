namespace FunctionalDdd;

using System;
using System.Diagnostics;

/// <summary>
/// Debug extension methods for inspecting Result values during development.
/// These methods execute only in DEBUG builds and become no-ops in RELEASE builds (with zero overhead).
/// </summary>
/// <remarks>
/// The methods are always available to prevent compilation errors in RELEASE builds,
/// but their implementation is conditionally compiled and becomes empty in RELEASE mode.
/// The compiler will inline and optimize away these empty methods, resulting in zero runtime overhead.
/// </remarks>
public static class ResultDebugExtensions
{
    /// <summary>
    /// Writes debug information about the Result to the console and returns the same Result.
    /// This method only executes in DEBUG builds.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the Result.</typeparam>
    /// <param name="result">The Result to debug.</param>
    /// <param name="message">Optional message to prefix the debug output.</param>
    /// <returns>The same Result that was passed in.</returns>
    /// <example>
    /// <code>
    /// var result = GetUser(id)
    ///     .Debug("After GetUser")
    ///     .Ensure(u => u.IsActive, Error.Validation("Inactive"))
    ///     .Debug("After Ensure")
    ///     .Bind(ProcessUser)
    ///     .Debug("After ProcessUser");
    /// </code>
    /// </example>
    public static Result<TValue> Debug<TValue>(this Result<TValue> result, string message = "")
    {
#if DEBUG
        var prefix = string.IsNullOrEmpty(message) ? "[DEBUG]" : $"[DEBUG] {message}";
        
        if (result.IsSuccess)
        {
            System.Diagnostics.Debug.WriteLine($"{prefix} Success: {result.Value}");
            Console.WriteLine($"{prefix} Success: {result.Value}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"{prefix} Failure: {result.Error.Code} - {result.Error.Detail}");
            Console.WriteLine($"{prefix} Failure: {result.Error.Code} - {result.Error.Detail}");
        }
#endif
        
        return result;
    }

    /// <summary>
    /// Writes detailed debug information about the Result to the console, including error properties.
    /// This method only executes in DEBUG builds.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the Result.</typeparam>
    /// <param name="result">The Result to debug.</param>
    /// <param name="message">Optional message to prefix the debug output.</param>
    /// <returns>The same Result that was passed in.</returns>
    /// <example>
    /// <code>
    /// var result = EmailAddress.TryCreate(email)
    ///     .Combine(FirstName.TryCreate(firstName))
    ///     .DebugDetailed("After validation");
    /// </code>
    /// </example>
    public static Result<TValue> DebugDetailed<TValue>(this Result<TValue> result, string message = "")
    {
#if DEBUG
        var prefix = string.IsNullOrEmpty(message) ? "[DEBUG]" : $"[DEBUG] {message}";
        
        if (result.IsSuccess)
        {
            var valueType = typeof(TValue).Name;
            var output = $"{prefix} Success\n  Type: {valueType}\n  Value: {result.Value}";
            System.Diagnostics.Debug.WriteLine(output);
            Console.WriteLine(output);
        }
        else
        {
            var error = result.Error;
            var output = $"{prefix} Failure\n" +
                        $"  Error Type: {error.GetType().Name}\n" +
                        $"  Code: {error.Code}\n" +
                        $"  Detail: {error.Detail}\n" +
                        $"  Instance: {error.Instance ?? "(none)"}";
            
            if (error is ValidationError validationError)
            {
                output += $"\n  Field Errors: {validationError.FieldErrors.Length}";
                for (int i = 0; i < validationError.FieldErrors.Length; i++)
                {
                    var fieldError = validationError.FieldErrors[i];
                    output += $"\n    [{i + 1}] {fieldError.FieldName}: {string.Join(", ", fieldError.Details)}";
                }
            }
            else if (error is AggregateError aggregated)
            {
                output += $"\n  Aggregated Errors: {aggregated.Errors.Count}";
                for (int i = 0; i < aggregated.Errors.Count; i++)
                {
                    var err = aggregated.Errors[i];
                    output += $"\n    [{i + 1}] {err.Code}: {err.Detail} (Instance: {err.Instance ?? "none"})";
                }
            }
            
            System.Diagnostics.Debug.WriteLine(output);
            Console.WriteLine(output);
        }
#endif
        
        return result;
    }

    /// <summary>
    /// Writes debug information with a stack trace for detailed debugging.
    /// Useful for understanding the call chain when debugging complex ROP operations.
    /// This method only executes in DEBUG builds.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the Result.</typeparam>
    /// <param name="result">The Result to debug.</param>
    /// <param name="message">Optional message to prefix the debug output.</param>
    /// <param name="includeStackTrace">Whether to include the stack trace. Default is true.</param>
    /// <returns>The same Result that was passed in.</returns>
    public static Result<TValue> DebugWithStack<TValue>(this Result<TValue> result, string message = "", bool includeStackTrace = true)
    {
#if DEBUG
        result.Debug(message);
        
        if (includeStackTrace)
        {
            var stackTrace = new StackTrace(true);
            var output = $"[DEBUG] Stack Trace:\n{stackTrace}";
            System.Diagnostics.Debug.WriteLine(output);
            Console.WriteLine(output);
        }
#endif
        
        return result;
    }

    /// <summary>
    /// Executes a custom debug action if the result is a success.
    /// This method only executes in DEBUG builds.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the Result.</typeparam>
    /// <param name="result">The Result to debug.</param>
    /// <param name="action">The action to execute with the success value.</param>
    /// <returns>The same Result that was passed in.</returns>
    /// <example>
    /// <code>
    /// var result = GetUser(id)
    ///     .DebugOnSuccess(user => 
    ///     {
    ///         Console.WriteLine($"User: {user.Id}, Email: {user.Email}");
    ///         Console.WriteLine($"IsActive: {user.IsActive}");
    ///     });
    /// </code>
    /// </example>
    public static Result<TValue> DebugOnSuccess<TValue>(this Result<TValue> result, Action<TValue> action)
    {
#if DEBUG
        if (result.IsSuccess)
        {
            action(result.Value);
        }
#endif

        return result;
    }

    /// <summary>
    /// Executes a custom debug action if the result is a failure.
    /// This method only executes in DEBUG builds.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the Result.</typeparam>
    /// <param name="result">The Result to debug.</param>
    /// <param name="action">The action to execute with the error.</param>
    /// <returns>The same Result that was passed in.</returns>
    /// <example>
    /// <code>
    /// var result = GetUser(id)
    ///     .DebugOnFailure(error => 
    ///     {
    ///         Console.WriteLine($"Error Type: {error.GetType().Name}");
    ///         Console.WriteLine($"Message: {error.Message}");
    ///     });
    /// </code>
    /// </example>
    public static Result<TValue> DebugOnFailure<TValue>(this Result<TValue> result, Action<Error> action)
    {
#if DEBUG
        if (result.IsFailure)
        {
            action(result.Error);
        }
#endif

        return result;
    }
}

/// <summary>
/// Debug-only async extension methods for inspecting Result values during development.
/// These methods are only available in DEBUG builds and are automatically excluded from RELEASE builds.
/// </summary>
public static class ResultDebugExtensionsAsync
{
    /// <summary>
    /// Writes debug information about the Result to the console and returns the same Result.
    /// This method only executes in DEBUG builds.
    /// </summary>
    public static async Task<Result<TValue>> DebugAsync<TValue>(this Task<Result<TValue>> resultTask, string message = "")
    {
        var result = await resultTask.ConfigureAwait(false);
#if DEBUG
        return result.Debug(message);
#else
        return result;
#endif
    }

    /// <summary>
    /// Writes detailed debug information about the Result to the console.
    /// This method only executes in DEBUG builds.
    /// </summary>
    public static async Task<Result<TValue>> DebugDetailedAsync<TValue>(this Task<Result<TValue>> resultTask, string message = "")
    {
        var result = await resultTask.ConfigureAwait(false);
#if DEBUG
        return result.DebugDetailed(message);
#else
        return result;
#endif
    }

    /// <summary>
    /// Writes debug information with a stack trace for detailed debugging.
    /// This method only executes in DEBUG builds.
    /// </summary>
    public static async Task<Result<TValue>> DebugWithStackAsync<TValue>(this Task<Result<TValue>> resultTask, string message = "", bool includeStackTrace = true)
    {
        var result = await resultTask.ConfigureAwait(false);
#if DEBUG
        return result.DebugWithStack(message, includeStackTrace);
#else
        return result;
#endif
    }

    /// <summary>
    /// Executes a custom debug action if the result is a success.
    /// This method only executes in DEBUG builds.
    /// </summary>
    public static async Task<Result<TValue>> DebugOnSuccessAsync<TValue>(this Task<Result<TValue>> resultTask, Action<TValue> action)
    {
        var result = await resultTask.ConfigureAwait(false);
#if DEBUG
        return result.DebugOnSuccess(action);
#else
        return result;
#endif
    }

    /// <summary>
    /// Executes a custom debug action if the result is a failure.
    /// This method only executes in DEBUG builds.
    /// </summary>
    public static async Task<Result<TValue>> DebugOnFailureAsync<TValue>(this Task<Result<TValue>> resultTask, Action<Error> action)
    {
        var result = await resultTask.ConfigureAwait(false);
#if DEBUG
        return result.DebugOnFailure(action);
#else
        return result;
#endif
    }

    /// <summary>
    /// Executes an async custom debug action if the result is a success.
    /// This method only executes in DEBUG builds.
    /// </summary>
    public static async Task<Result<TValue>> DebugOnSuccessAsync<TValue>(this Task<Result<TValue>> resultTask, Func<TValue, Task> action)
    {
        var result = await resultTask.ConfigureAwait(false);
#if DEBUG
        if (result.IsSuccess)
        {
            await action(result.Value).ConfigureAwait(false);
        }
#endif

        return result;
    }

    /// <summary>
    /// Executes an async custom debug action if the result is a failure.
    /// This method only executes in DEBUG builds.
    /// </summary>
    public static async Task<Result<TValue>> DebugOnFailureAsync<TValue>(this Task<Result<TValue>> resultTask, Func<Error, Task> action)
    {
        var result = await resultTask.ConfigureAwait(false);
#if DEBUG
        if (result.IsFailure)
        {
            await action(result.Error).ConfigureAwait(false);
        }
#endif

        return result;
    }
}

