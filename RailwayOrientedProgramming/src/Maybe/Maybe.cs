namespace FunctionalDDD;

public sealed class Maybe
{
    private Maybe()
    {
    }

    public static Maybe<T> None<T>() => new();

    public static Maybe<T> From<T>(T? value) => new(value);
}
