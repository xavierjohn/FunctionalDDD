namespace FunctionalDDD.Core;

public static partial class MaybeExtensions
{
    public static Result<T> ToResult<T>(in this Maybe<T> maybe, Error error)
    {
        if (maybe.HasNoValue)
            return Result.Failure<T>(error);

        return Result.Success(maybe.GetValueOrThrow());
    }
}
