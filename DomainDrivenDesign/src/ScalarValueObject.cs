namespace FunctionalDdd;

/// <summary>
/// Create a typed class that represents a single scalar value object.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <example>
/// This example shows creating a strongly typed scalar value for temperature.
/// <code>
/// public class Temperature : ScalarValueObject&lt;decimal&gt;
/// {
///    public Temperature(decimal value) : base(value)
///    {
///    }
///    
///    // The comparison can be customized by overriding the GetEqualityComponents method.
///    protected override IEnumerable&lt;IComparable&gt; GetEqualityComponents()
///    {
///        yield return Math.Round(Value, 2);
///    }
/// }
/// </code>
/// </example>
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
