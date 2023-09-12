namespace FunctionalDDD.RailwayOrientedProgramming;

using FunctionalDDD.RailwayOrientedProgramming.Errors;

public static partial class MaybeExtensions
{
    public static Result<TOk> ToResult<TOk>(in this Maybe<TOk> maybe, Error error)
        where TOk : notnull
    {
        if (maybe.HasNoValue)
            return Result.Failure<TOk>(error);

        return Result.Success(maybe.GetValueOrThrow());
    }

    public static async Task<Result<TOk>> ToResultAsync<TOk>(this Task<Maybe<TOk>> maybeTask, Error error)
        where TOk : notnull
    {
        var maybe = await maybeTask.ConfigureAwait(false);
        return maybe.ToResult(error);
    }

    public static async ValueTask<Result<TOk>> ToResultAsync<TOk>(this ValueTask<Maybe<TOk>> maybeTask, Error error)
        where TOk : notnull
    {
        Maybe<TOk> maybe = await maybeTask;
        return maybe.ToResult(error);
    }

    public static Result<TOk> ToResult<TOk>(in this Maybe<TOk> maybe, Func<Error> ferror)
    where TOk : notnull
    {
        if (maybe.HasNoValue)
            return Result.Failure<TOk>(ferror);

        return Result.Success(maybe.GetValueOrThrow());
    }

    public static async Task<Result<TOk>> ToResultAsync<TOk>(this Task<Maybe<TOk>> maybeTask, Func<Error> ferror)
        where TOk : notnull
    {
        var maybe = await maybeTask.ConfigureAwait(false);
        return maybe.ToResult(ferror);
    }

    public static async ValueTask<Result<TOk>> ToResultAsync<TOk>(this ValueTask<Maybe<TOk>> maybeTask, Func<Error> ferror)
        where TOk : notnull
    {
        Maybe<TOk> maybe = await maybeTask;
        return maybe.ToResult(ferror);
    }
}
