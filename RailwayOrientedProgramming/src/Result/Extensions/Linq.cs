namespace FunctionalDdd;

/// <summary>
/// LINQ query expression support (Select, SelectMany, Where).
/// </summary>
public static class ResultLinqExtensions
{
    // Select -> Map
    public static Result<TOut> Select<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> selector) =>
        result.Map(selector);

    // SelectMany (monadic bind with projection)
    public static Result<TResult> SelectMany<TSource, TCollection, TResult>(
        this Result<TSource> source,
        Func<TSource, Result<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
        => source.Bind(s => collectionSelector(s).Map(c => resultSelector(s, c)));

    // Where (filter). If predicate fails, convert to failure with a generic error.
    // NOTE: For richer errors prefer Ensure().
    public static Result<TSource> Where<TSource>(this Result<TSource> source, Func<TSource, bool> predicate)
        => source.Ensure(predicate, Error.Unexpected("Result filtered out by predicate."));
}