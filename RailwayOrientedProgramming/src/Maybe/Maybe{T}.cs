namespace FunctionalDDD;

using System.Diagnostics.CodeAnalysis;

public readonly struct Maybe<T> :
    IEquatable<T>,
    IEquatable<Maybe<T>>,
    IMaybe<T>
{
    private const string NoValue = "Maybe has no value.";
    private readonly (bool IsSet, T? Value) inner;

    public T GetValueOrThrow(string? errorMessage = null) =>
        inner.IsSet
        ? inner.Value!
        : throw new InvalidOperationException(errorMessage ?? NoValue);

    public T GetValueOrDefault(T defaultValue) => inner.IsSet ? inner.Value! : defaultValue;

    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = inner.Value;
        return inner.IsSet;
    }

    public T Value => GetValueOrThrow();

    public bool HasValue => inner.IsSet;

    public bool HasNoValue => !inner.IsSet;

    internal Maybe(T? value) => inner = (value is not null, value);

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
        inner.IsSet && other.inner.IsSet
        ? EqualityComparer<T>.Default.Equals(inner.Value!, other.inner.Value!)
        : !inner.IsSet && !other.inner.IsSet;

    public bool Equals(T? other) =>
        inner.IsSet
        ? EqualityComparer<T>.Default.Equals(inner.Value, other)
        : !inner.IsSet;

    public override int GetHashCode() => inner.Value?.GetHashCode() ?? 0;

    public override string ToString() => inner.Value?.ToString() ?? NoValue;
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
