namespace FunctionalDDD.CommonValueObjects;

public abstract class RequiredGuid<T> : SimpleValueObject<Guid>
    where T : RequiredGuid<T>
{

    protected RequiredGuid(Guid value) : base(value)
    {
    }
}
