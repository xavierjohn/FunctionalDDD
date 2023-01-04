namespace FunctionalDDD;

public sealed class Maybe
{
    private Maybe()
    {
    }

    public static Maybe<T> None<T>() where T : notnull => new();

    public static Maybe<T> From<T>(T? value) where T : notnull => new(value);
}
