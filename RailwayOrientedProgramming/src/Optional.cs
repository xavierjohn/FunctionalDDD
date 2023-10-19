namespace FunctionalDDD.Results;
using System;

public static class OptionalExtenstions
{
    public static Result<Maybe<TOut>> Optional<TIn, TOut>(TIn? value, Func<TIn, Result<TOut>> function)
        where TOut : notnull
    {
        if (value is null)
            return Maybe.None<TOut>();

        return function(value).Map(r => Maybe.From(r));
    }
}
