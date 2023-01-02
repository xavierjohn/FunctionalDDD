namespace FunctionalDDD;

public abstract class RequiredString<T> : Required<string, T>
    where T : RequiredString<T>
{
    protected RequiredString(string value) : base(value)
    {
    }
}
