namespace FunctionalDDD;

public static partial class MaybeExtensions
{
    public static Result<TOk, Err> ToResult<TOk>(in this Maybe<TOk> maybe, Err error)
        where TOk : notnull
    {
        if (maybe.HasNoValue)
            return Result.Failure<TOk, Err>(error);

        return Result.Success<TOk, Err>(maybe.GetValueOrThrow());
    }

    public static async Task<Result<TOk, Err>> ToResultAsync<TOk>(this Task<Maybe<TOk>> maybeTask, Err errors)
        where TOk : notnull
    {
        var maybe = await maybeTask.ConfigureAwait(false);
        return maybe.ToResult(errors);
    }

    public static async ValueTask<Result<TOk, Err>> ToResultAsync<TOk>(this ValueTask<Maybe<TOk>> maybeTask, Err errors)
        where TOk : notnull
    {
        Maybe<TOk> maybe = await maybeTask;
        return maybe.ToResult(errors);
    }
}
