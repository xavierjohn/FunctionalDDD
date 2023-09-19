namespace FunctionalDDD.Domain.ValueObjects;

using FunctionalDDD.Domain;

public abstract class RequiredGuid<T> : SimpleValueObject<Guid>
    where T : RequiredGuid<T>
{

    protected RequiredGuid(Guid value) : base(value)
    {
    }
}
