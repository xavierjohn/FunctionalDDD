namespace FunctionalDDD;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<TOk, Error> Tee<TOk>(this Result<TOk, Error> result, Action action)
    {
        if (result.IsSuccess)
            action();

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<TOk, Error> Tee<TOk>(this Result<TOk, Error> result, Action<TOk> action)
    {
        if (result.IsSuccess)
            action(result.Value);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async Task<Result<TOk, Error>> TeeAsync<TOk>(this Task<Result<TOk, Error>> resultTask, Action action)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);
        return result.Tee(action);
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async Task<Result<TOk, Error>> TeeAsync<TOk>(this Task<Result<TOk, Error>> resultTask, Action<TOk> action)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);
        return result.Tee(action);
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async Task<Result<TOk, Error>> TeeAsync<TOk>(this Result<TOk, Error> result, Func<Task> func)
    {
        if (result.IsSuccess)
            await func().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async Task<Result<TOk, Error>> TeeAsync<TOk>(this Task<Result<TOk, Error>> resultTask, Func<Task> func)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);

        if (result.IsSuccess)
            await func().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async Task<Result<TOk, Error>> TeeAsync<TOk>(this Task<Result<TOk, Error>> resultTask, Func<TOk, Task> func)
    {
        Result<TOk, Error> result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess)
            await func(result.Value).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async Task<Result<TOk, Error>> TeeAsync<TOk>(this Result<TOk, Error> result, Func<TOk, Task> func)
    {
        if (result.IsSuccess)
            await func(result.Value).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async ValueTask<Result<TOk, Error>> TeeAsync<TOk>(this Result<TOk, Error> result, Func<ValueTask> func)
    {
        if (result.IsSuccess)
            await func().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async ValueTask<Result<TOk, Error>> TeeAsync<TOk>(this Result<TOk, Error> result, Func<TOk, ValueTask> func)
    {
        if (result.IsSuccess)
            await func(result.Value).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async ValueTask<Result<TOk, Error>> TeeAsync<TOk>(this ValueTask<Result<TOk, Error>> resultTask, Func<ValueTask> valueTask)
    {
        Result<TOk, Error> result = await resultTask;

        if (result.IsSuccess)
            await valueTask();

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async ValueTask<Result<TOk, Error>> TeeAsync<TOk>(this ValueTask<Result<TOk, Error>> resultTask, Func<TOk, ValueTask> valueTask)
    {
        Result<TOk, Error> result = await resultTask;

        if (result.IsSuccess)
            await valueTask(result.Value);

        return result;
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async ValueTask<Result<TOk, Error>> TeeAsync<TOk>(this ValueTask<Result<TOk, Error>> resultTask, Action action)
    {
        Result<TOk, Error> result = await resultTask;
        return result.Tee(action);
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async ValueTask<Result<TOk, Error>> TeeAsync<TOk>(this ValueTask<Result<TOk, Error>> resultTask, Action<TOk> action)
    {
        Result<TOk, Error> result = await resultTask;
        return result.Tee(action);
    }
}
