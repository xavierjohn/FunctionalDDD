﻿namespace FunctionalDDD;

public abstract class SimpleValueObject<T> : ValueObject
    where T : IComparable
{
    public T Value { get; }

    protected SimpleValueObject(T value) => Value = value;

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;

    public static implicit operator T(SimpleValueObject<T> valueObject) => valueObject.Value;
}
