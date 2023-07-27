namespace FunctionalDDD;

public static partial class MaybeExtensions
{
    public static Result<TOk> ToResult<TOk>(in this Maybe<TOk> maybe, Error ferror)
        where TOk : notnull
    {
        if (maybe.HasNoValue)
            return Result.Failure<TOk>(ferror);

        return Result.Success(maybe.GetValueOrThrow());
    }

    public static async Task<Result<TOk>> ToResultAsync<TOk>(this Task<Maybe<TOk>> maybeTask, Error errors)
        where TOk : notnull
    {
        var maybe = await maybeTask.ConfigureAwait(false);
        return maybe.ToResult(errors);
    }

    public static async ValueTask<Result<TOk>> ToResultAsync<TOk>(this ValueTask<Maybe<TOk>> maybeTask, Error ferror)
        where TOk : notnull
    {
        Maybe<TOk> maybe = await maybeTask;
        return maybe.ToResult(ferror);
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
