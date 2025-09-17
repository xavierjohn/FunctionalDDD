namespace FunctionalDdd;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// The Maybe type used in functional programming languages to represent an optional value.
/// </summary>
public readonly struct Maybe<T> :
    IEquatable<T>,
    IEquatable<Maybe<T>>
{
    private readonly bool _isValueSet;
    private readonly T? _value;

    private const string NoValue = "Maybe has no value.";

    public T GetValueOrThrow(string? errorMessage = null)
    {
        if (_isValueSet)
            return _value!;

        throw new InvalidOperationException(errorMessage ?? NoValue);
    }

    public T GetValueOrDefault(T defaultValue)
    {
        if (_isValueSet)
            return _value!;

        return defaultValue;
    }

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

    public static bool operator ==(Maybe<T> maybe, object? other) => maybe.Equals(other);

    public static bool operator !=(Maybe<T> maybe, object? other) => !maybe.Equals(other);

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
        (_isValueSet && EqualityComparer<T>.Default.Equals(_value, other))
        || (!_isValueSet && other is null);

    public override int GetHashCode() => _isValueSet
        ? (_value?.GetHashCode() ?? 0)
        : 0;

    public override string ToString() => _value?.ToString() ?? string.Empty;
}
