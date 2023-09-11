namespace FunctionalDDD.CommonValueObjects;

public abstract class RequiredString<T> : SimpleValueObject<string>
    where T : RequiredString<T>
{
    protected RequiredString(string value) : base(value)
    {
    }
}
