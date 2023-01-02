namespace FunctionalDDD;

public abstract class RequiredGuid<T> : Required<Guid, T>
    where T : RequiredGuid<T>
{

    protected RequiredGuid(Guid value) : base(value)
    {
    }
}
