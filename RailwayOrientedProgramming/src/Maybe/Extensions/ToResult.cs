namespace FunctionalDDD;

public static partial class MaybeExtensions
{
    public static Result<T> ToResult<T>(in this Maybe<T> maybe, Error error)
        where T : notnull
    {
        if (maybe.HasNoValue)
            return Result.Failure<T>(error);

        return Result.Success(maybe.GetValueOrThrow());
    }

    public static async Task<Result<T>> ToResultAsync<T>(this Task<Maybe<T>> maybeTask, Error error)
        where T : notnull
    {
        var maybe = await maybeTask.ConfigureAwait(false);
        return maybe.ToResult(error);
    }

    public static async ValueTask<Result<T>> ToResultAsync<T>(this ValueTask<Maybe<T>> maybeTask, Error error)
        where T : notnull
    {
        Maybe<T> maybe = await maybeTask;
        return maybe.ToResult(error);
    }
}
