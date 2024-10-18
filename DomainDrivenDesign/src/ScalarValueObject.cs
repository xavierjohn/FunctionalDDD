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
public abstract class ScalarValueObject<T> : ValueObject, IConvertible
    where T : IComparable
{
    public T Value { get; }

    protected ScalarValueObject(T value) => Value = value;

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;
    public TypeCode GetTypeCode() => Type.GetTypeCode(typeof(T));
    public bool ToBoolean(IFormatProvider? provider) => Convert.ToBoolean(Value, provider);
    public byte ToByte(IFormatProvider? provider) => Convert.ToByte(Value, provider);
    public char ToChar(IFormatProvider? provider) => Convert.ToChar(Value, provider);
    public DateTime ToDateTime(IFormatProvider? provider) => Convert.ToDateTime(Value, provider);
    public decimal ToDecimal(IFormatProvider? provider) => Convert.ToDecimal(Value, provider);
    public double ToDouble(IFormatProvider? provider) => Convert.ToDouble(Value, provider);
    public short ToInt16(IFormatProvider? provider) => Convert.ToInt16(Value, provider);
    public int ToInt32(IFormatProvider? provider) => Convert.ToInt32(Value, provider);
    public long ToInt64(IFormatProvider? provider) => Convert.ToInt64(Value, provider);
    public sbyte ToSByte(IFormatProvider? provider) => Convert.ToSByte(Value, provider);
    public float ToSingle(IFormatProvider? provider) => Convert.ToSingle(Value, provider);
    public string ToString(IFormatProvider? provider) => Value?.ToString() ?? string.Empty;
    public object ToType(Type conversionType, IFormatProvider? provider) => Convert.ChangeType(Value, conversionType, provider);
    public ushort ToUInt16(IFormatProvider? provider) => Convert.ToUInt16(Value, provider);
    public uint ToUInt32(IFormatProvider? provider) => Convert.ToUInt32(Value, provider);
    public ulong ToUInt64(IFormatProvider? provider) => Convert.ToUInt64(Value, provider);

    public static implicit operator T(ScalarValueObject<T> valueObject) => valueObject.Value;
}
