namespace FunctionalDDD.CommonValueObjects;

public abstract class RequiredString<T> : Required<string, T>
    where T : RequiredString<T>
{
    protected RequiredString(string value) : base(value)
    {
    }
    public static Result<T> Create(Maybe<string> requiredStringOrNothing)
    {
        return requiredStringOrNothing
            .EnsureNotNullOrWhiteSpace(CannotBeEmptyError)
            .Map(name => CreateInstance.Value(name));
    }
}
