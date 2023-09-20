namespace FunctionalDDD.Results;

/// <summary>
///     Executes the given action if the starting result is a success. Returns the starting result.
/// It is useful to execute functions that don't have a return type or return type can be ignored.
/// </summary>
public static class TeeExtensions
{
    /// <summary>
    ///     Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static Result<TOk> Tee<TOk>(this Result<TOk> result, Action action)
    {
        if (result.IsSuccess)
            action();

        return result;
    }

    /// <summary>
    ///     Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static Result<TOk> Tee<TOk>(this Result<TOk> result, Action<TOk> action)
    {
        if (result.IsSuccess)
            action(result.Value);

        return result;
    }
}

/// <summary>
///     Executes the given action if the starting result is a success. Returns the starting result.
/// It is useful to execute functions that don't have a return type or return type can be ignored.
/// </summary>
public static class TeeExtensionsAsync
{
    /// <summary>
    ///     Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TOk>> TeeAsync<TOk>(this Task<Result<TOk>> resultTask, Action action)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);
        return result.Tee(action);
    }

    /// <summary>
    ///     Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TOk>> TeeAsync<TOk>(this Task<Result<TOk>> resultTask, Action<TOk> action)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);
        return result.Tee(action);
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TOk>> TeeAsync<TOk>(this Result<TOk> result, Func<Task> func)
    {
        if (result.IsSuccess)
            await func().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TOk>> TeeAsync<TOk>(this Task<Result<TOk>> resultTask, Func<Task> func)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);

        if (result.IsSuccess)
            await func().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TOk>> TeeAsync<TOk>(this Task<Result<TOk>> resultTask, Func<TOk, Task> func)
    {
        Result<TOk> result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess)
            await func(result.Value).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async Task<Result<TOk>> TeeAsync<TOk>(this Result<TOk> result, Func<TOk, Task> func)
    {
        if (result.IsSuccess)
            await func(result.Value).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> TeeAsync<TOk>(this Result<TOk> result, Func<ValueTask> func)
    {
        if (result.IsSuccess)
            await func().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> TeeAsync<TOk>(this Result<TOk> result, Func<TOk, ValueTask> func)
    {
        if (result.IsSuccess)
            await func(result.Value).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> TeeAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<ValueTask> valueTask)
    {
        Result<TOk> result = await resultTask;

        if (result.IsSuccess)
            await valueTask();

        return result;
    }

    /// <summary>
    ///     Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> TeeAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Func<TOk, ValueTask> valueTask)
    {
        Result<TOk> result = await resultTask;

        if (result.IsSuccess)
            await valueTask(result.Value);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> TeeAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Action action)
    {
        Result<TOk> result = await resultTask;
        return result.Tee(action);
    }

    /// <summary>
    ///     Executes the given action if the starting result is a success. Returns the starting result.
    /// </summary>
    public static async ValueTask<Result<TOk>> TeeAsync<TOk>(this ValueTask<Result<TOk>> resultTask, Action<TOk> action)
    {
        Result<TOk> result = await resultTask;
        return result.Tee(action);
    }
}
