namespace FunctionalDDD;

public static partial class MaybeExtensions
{
    public static Result<TOk, Error> ToResult<TOk>(in this Maybe<TOk> maybe, Error error)
        where TOk : notnull
    {
        if (maybe.HasNoValue)
            return Result.Failure<TOk, Error>(error);

        return Result.Success<TOk, Error>(maybe.GetValueOrThrow());
    }

    public static async Task<Result<TOk, Error>> ToResultAsync<TOk>(this Task<Maybe<TOk>> maybeTask, Error errors)
        where TOk : notnull
    {
        var maybe = await maybeTask.ConfigureAwait(false);
        return maybe.ToResult(errors);
    }

    public static async ValueTask<Result<TOk, Error>> ToResultAsync<TOk>(this ValueTask<Maybe<TOk>> maybeTask, Error errors)
        where TOk : notnull
    {
        Maybe<TOk> maybe = await maybeTask;
        return maybe.ToResult(errors);
    }
}
