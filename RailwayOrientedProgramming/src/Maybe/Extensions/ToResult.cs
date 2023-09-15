namespace FunctionalDDD.RailwayOrientedProgramming;

using FunctionalDDD.RailwayOrientedProgramming.Errors;

public static partial class MaybeExtensions
{
    /// <summary>
    /// If <see cref="Maybe{TValue}"/> has a value, return a <see cref="Result{TValue}"/><br/>
    /// Otherwise, return the <see cref="Error"/>
    /// </summary>
    /// <typeparam name="TValue">Type of the value contained in Maybe.</typeparam>
    /// <param name="maybe"></param>
    /// <param name="error">One of the classes derived from <see cref="Error"/></param>
    /// <returns></returns>
    public static Result<TValue> ToResult<TValue>(in this Maybe<TValue> maybe, Error error)
        where TValue : notnull
    {
        if (maybe.HasNoValue)
            return Result.Failure<TValue>(error);

        return Result.Success(maybe.GetValueOrThrow());
    }

    /// <summary>
    /// If <see cref="Maybe{TValue}"/> has a value, return a <see cref="Result{TValue}"/><br/>
    /// Otherwise, return the <see cref="Error"/>
    /// </summary>
    /// <typeparam name="TValue">Type of the value contained in Maybe.</typeparam>
    /// <param name="maybe"></param>
    /// <param name="error">One of the classes derived from <see cref="Error"/></param>
    /// <returns></returns>
    public static Result<TValue> ToResult<TValue>(in this Maybe<TValue> maybe, Func<Error> ferror)
    where TValue : notnull
    {
        if (maybe.HasNoValue)
            return Result.Failure<TValue>(ferror);

        return Result.Success(maybe.GetValueOrThrow());
    }
}

public static partial class MaybeExtensionsAsync
{
    /// <summary>
    /// If <see cref="Maybe{TValue}"/> has a value, return a <see cref="Result{TValue}"/><br/>
    /// Otherwise, return the <see cref="Error"/>
    /// </summary>
    /// <typeparam name="TValue">Type of the value contained in Maybe.</typeparam>
    /// <param name="maybe"></param>
    /// <param name="error">One of the classes derived from <see cref="Error"/></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> ToResultAsync<TValue>(this Task<Maybe<TValue>> maybeTask, Error error)
        where TValue : notnull
    {
        var maybe = await maybeTask.ConfigureAwait(false);
        return maybe.ToResult(error);
    }

    /// <summary>
    /// If <see cref="Maybe{TValue}"/> has a value, return a <see cref="Result{TValue}"/><br/>
    /// Otherwise, return the <see cref="Error"/>
    /// </summary>
    /// <typeparam name="TValue">Type of the value contained in Maybe.</typeparam>
    /// <param name="maybe"></param>
    /// <param name="error">One of the classes derived from <see cref="Error"/></param>
    /// <returns></returns>
    public static async ValueTask<Result<TValue>> ToResultAsync<TValue>(this ValueTask<Maybe<TValue>> maybeTask, Error error)
        where TValue : notnull
    {
        Maybe<TValue> maybe = await maybeTask;
        return maybe.ToResult(error);
    }

    /// <summary>
    /// If <see cref="Maybe{TValue}"/> has a value, return a <see cref="Result{TValue}"/><br/>
    /// Otherwise, return the <see cref="Error"/>
    /// </summary>
    /// <typeparam name="TValue">Type of the value contained in Maybe.</typeparam>
    /// <param name="maybe"></param>
    /// <param name="error">One of the classes derived from <see cref="Error"/></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> ToResultAsync<TValue>(this Task<Maybe<TValue>> maybeTask, Func<Error> ferror)
        where TValue : notnull
    {
        var maybe = await maybeTask.ConfigureAwait(false);
        return maybe.ToResult(ferror);
    }

    /// <summary>
    /// If <see cref="Maybe{TValue}"/> has a value, return a <see cref="Result{TValue}"/><br/>
    /// Otherwise, return the <see cref="Error"/>
    /// </summary>
    /// <typeparam name="TValue">Type of the value contained in Maybe.</typeparam>
    /// <param name="maybe"></param>
    /// <param name="error">One of the classes derived from <see cref="Error"/></param>
    /// <returns></returns>
    public static async ValueTask<Result<TValue>> ToResultAsync<TValue>(this ValueTask<Maybe<TValue>> maybeTask, Func<Error> ferror)
        where TValue : notnull
    {
        Maybe<TValue> maybe = await maybeTask;
        return maybe.ToResult(ferror);
    }
}
