namespace FunctionalDDD.Domain;
public abstract class RequiredGuid<T> : ScalarValueObject<Guid>
    where T : RequiredGuid<T>
{

    protected RequiredGuid(Guid value) : base(value)
    {
    }
}
