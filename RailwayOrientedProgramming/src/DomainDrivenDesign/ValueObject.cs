﻿namespace FunctionalDDD;

public abstract class ValueObject : IComparable, IComparable<ValueObject>
{
    private int? _cachedHashCode;

    protected abstract IEnumerable<IComparable> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (GetType() != obj.GetType())
            return false;

        var valueObject = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(valueObject.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        if (!_cachedHashCode.HasValue)
        {
            _cachedHashCode = GetEqualityComponents()
                .Aggregate(1, (current, obj) =>
                {
                    unchecked
                    {
                        return current * 23 + (obj?.GetHashCode() ?? 0);
                    }
                });
        }

        return _cachedHashCode.Value;
    }

    public virtual int CompareTo(object? obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        var thisType = GetType();
        var otherType = obj.GetType();

        if (thisType != otherType)
            throw new ArgumentException($"Cannot compare objects of different types: {thisType} and {otherType}");


        var other = (ValueObject)obj;

        object[] components = GetEqualityComponents().ToArray();
        object[] otherComponents = other.GetEqualityComponents().ToArray();

        for (int i = 0; i < components.Length; i++)
        {
            int comparison = CompareComponents(components[i], otherComponents[i]);
            if (comparison != 0)
                return comparison;
        }

        return 0;
    }

    public virtual int CompareTo(ValueObject? other)
    {
        return CompareTo(other as object);
    }

    private static int CompareComponents(object object1, object object2)
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

    public static bool operator ==(ValueObject a, ValueObject b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(ValueObject a, ValueObject b) => !(a == b);
}
