namespace FunctionalDDD;

using System.Diagnostics.CodeAnalysis;

public readonly struct Maybe<T> :
    IEquatable<T>,
    IEquatable<Maybe<T>>,
    IMaybe<T>
{
    private readonly bool _isValueSet;
    private readonly T _value;

    private const string NoValue = "Maybe has no value.";

    public T GetValueOrThrow(string? errorMessage = null) =>
        _isValueSet
        ? _value
        : throw new InvalidOperationException(errorMessage ?? NoValue);

    public T GetValueOrDefault(T defaultValue) => _isValueSet ? _value : defaultValue;

    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return _isValueSet;
    }

    public T Value => GetValueOrThrow();

    public bool HasValue => _isValueSet;

    public bool HasNoValue => !_isValueSet;

    internal Maybe(T? value)
    {
        _isValueSet = value is not null;
        _value = value!;
    }

    public static implicit operator Maybe<T>(T? value) =>
        value is Maybe<T> maybe ? maybe : new(value);

    public static bool operator ==(Maybe<T> maybe, T value) => maybe.Equals(value);

    public static bool operator !=(Maybe<T> maybe, T value) => !maybe.Equals(value);

    public static bool operator ==(Maybe<T> maybe, object other) => maybe.Equals(other);

    public static bool operator !=(Maybe<T> maybe, object other) => !maybe.Equals(other);

    public static bool operator ==(Maybe<T> first, Maybe<T> second) => first.Equals(second);

    public static bool operator !=(Maybe<T> first, Maybe<T> second) => !first.Equals(second);

    public override bool Equals(object? obj) =>
        obj switch
        {
            Maybe<T> other => Equals(other),
            T other => Equals(other),
            _ => false,
        };

    public bool Equals(Maybe<T> other) =>
        _isValueSet && other._isValueSet
        ? EqualityComparer<T>.Default.Equals(_value, other._value)
        : !_isValueSet && !other._isValueSet;

    public bool Equals(T? other) =>
        _isValueSet
        ? EqualityComparer<T>.Default.Equals(_value, other)
        : !_isValueSet;

    public override int GetHashCode() => _value?.GetHashCode() ?? 0;

    public override string ToString() => _value?.ToString() ?? NoValue;
}

/// <summary>
/// Useful in scenarios where you need to determine if a value is Maybe or not
/// </summary>
public interface IMaybe<out T>
{
    T Value { get; }
    bool HasValue { get; }
    bool HasNoValue { get; }
}
