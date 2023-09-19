namespace FunctionalDDD.Domain;
/// <summary>
/// Create a typed class that represents a value object.
/// The class calls the abstract method GetEqualityComponents to get the components to compare.
/// </summary>
/// <example>
/// <code>
/// class Address : ValueObject
/// {
///     public string Street { get; }
///     public string City { get; }
///
///     public Address(string street, string city)
///     {
///         Street = street;
///         City = city;
///     }
///
///     protected override IEnumerable&lt;IComparable&gt; GetEqualityComponents()
///     {
///         yield return Street;
///         yield return City;
///     }
/// }
/// </code>
/// </example>
/// <example>
/// <code>
/// class DerivedAddress : Address
/// {
///     public string Country { get; }
///     public DerivedAddress(string street, string city, string country) : base(street, city) => Country = country;
///
///     protected override IEnumerable&lt;IComparable&gt; GetEqualityComponents()
///     {
///         foreach (var s in base.GetEqualityComponents())
///             yield return s;
///         yield return City;
///     }
/// }
/// </code>
/// </example>
public abstract class ValueObject : IComparable<ValueObject>, IEquatable<ValueObject>
{
    private int? _cachedHashCode;

    protected abstract IEnumerable<IComparable> GetEqualityComponents();

    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    public bool Equals(ValueObject? other)
    {
        if (other is null) return false;
        if (GetType() != other.GetType())
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        if (!_cachedHashCode.HasValue)
        {
            _cachedHashCode = GetEqualityComponents()
                .Aggregate(1, (current, obj) => HashCode.Combine(current, (obj?.GetHashCode() ?? 0)));
        }

        return _cachedHashCode.Value;
    }


    public virtual int CompareTo(ValueObject? other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));
        var thisType = GetType();
        var otherType = other.GetType();

        if (thisType != otherType)
            throw new ArgumentException($"Cannot compare objects of different types: {thisType} and {otherType}");


        object[] components = GetEqualityComponents().ToArray();
        object[] otherComponents = other.GetEqualityComponents().ToArray();

        for (var i = 0; i < components.Length; i++)
        {
            var comparison = CompareComponents(components[i], otherComponents[i]);
            if (comparison != 0)
                return comparison;
        }

        return 0;
    }

    private static int CompareComponents(object? object1, object? object2)
    {
        if (object1 is null && object2 is null)
            return 0;

        if (object1 is null)
            return -1;

        if (object2 is null)
            return 1;

        if (object1 is IComparable comparable1 && object2 is IComparable comparable2)
            return comparable1.CompareTo(comparable2);

        return object1.Equals(object2) ? 0 : -1;
    }

    public static bool operator ==(ValueObject? a, ValueObject? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(ValueObject a, ValueObject b) => !(a == b);

    public static bool operator <(ValueObject left, ValueObject right)
    {
        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
    }

    public static bool operator <=(ValueObject left, ValueObject right)
    {
        return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    }

    public static bool operator >(ValueObject left, ValueObject right)
    {
        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
    }

    public static bool operator >=(ValueObject left, ValueObject right)
    {
        return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }
}
