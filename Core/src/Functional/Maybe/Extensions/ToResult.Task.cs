namespace FunctionalDDD;

public static partial class MaybeExtensions
{
    public static async Task<Result<T>> ToResult<T, E>(this Task<Maybe<T>> maybeTask, Error error)
    {
        var maybe = await maybeTask.DefaultAwait();
        return maybe.ToResult(error);
    }
}
