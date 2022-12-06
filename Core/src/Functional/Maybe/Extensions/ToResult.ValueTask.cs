namespace FunctionalDDD.Core;

public static partial class MaybeExtensions
{
    public static async ValueTask<Result<T>> ToResult<T>(this ValueTask<Maybe<T>> maybeTask, Error error)
    {
        Maybe<T> maybe = await maybeTask;
        return maybe.ToResult(error);
    }
}
