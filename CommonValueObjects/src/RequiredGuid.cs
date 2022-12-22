namespace FunctionalDDD;
public abstract class RequiredGuid<T> : Required<Guid, T>
    where T : RequiredGuid<T>
{

    protected RequiredGuid(Guid value) : base(value)
    {
    }


    public static T CreateUnique() => CreateInstance.Value(Guid.NewGuid());

    public static Result<T> Create(Maybe<Guid> requiredGuidOrNothing)
    {
        return requiredGuidOrNothing
            .ToResult(CannotBeEmptyError)
            .Ensure(x => x != Guid.Empty, CannotBeEmptyError)
            .Map(guid => CreateInstance.Value(guid));
    }

}
