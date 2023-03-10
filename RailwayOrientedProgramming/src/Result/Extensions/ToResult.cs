namespace FunctionalDDD;

public static partial class NullableExtensions
{
    public static bool HasNoValue<T>(in this T? nullable)
        where T : struct => !nullable.HasValue;
    public static bool HasNoValue<T>(this T? obj) => obj == null;

    public static Result<T, Error> ToResult<T>(in this T? nullable, Error error)
        where T : struct
    {
        if (!nullable.HasValue)
            return Result.Failure<T, Error>(error);

        return Result.Success<T, Error>(nullable.Value);
    }
    public static Result<T, Error> ToResult<T>(this T? obj, Error error)
        where T : struct
    {
        if (obj == null)
            return Result.Failure<T, Error>(error);

        return Result.Success<T, Error>(obj.Value);
    }

    //public static async Task<Result<TOk, Error>> ToResultAsync<TOk>(this Task<Maybe<TOk>> maybeTask, Error errors)
    //    where TOk : notnull
    //{
    //    var maybe = await maybeTask.ConfigureAwait(false);
    //    return maybe.ToResult(errors);
    //}

    //public static async ValueTask<Result<TOk, Error>> ToResultAsync<TOk>(this ValueTask<Maybe<TOk>> maybeTask, Error errors)
    //    where TOk : notnull
    //{
    //    Maybe<TOk> maybe = await maybeTask;
    //    return maybe.ToResult(errors);
    //}
}
