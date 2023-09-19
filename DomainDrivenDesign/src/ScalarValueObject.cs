namespace FunctionalDDD.Domain;
public abstract class ScalarValueObject<T> : ValueObject
    where T : IComparable
{
    public T Value { get; }

    protected ScalarValueObject(T value) => Value = value;

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;

    public static implicit operator T(ScalarValueObject<T> valueObject) => valueObject.Value;
}
