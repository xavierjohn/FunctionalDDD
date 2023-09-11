namespace FunctionalDDD.CommonValueObjects;

using FunctionalDDD.DomainDivenDesign;

public abstract class RequiredGuid<T> : SimpleValueObject<Guid>
    where T : RequiredGuid<T>
{

    protected RequiredGuid(Guid value) : base(value)
    {
    }
}
