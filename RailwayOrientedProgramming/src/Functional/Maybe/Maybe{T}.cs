namespace FunctionalDDD;
public readonly struct Maybe<T> : IEquatable<Maybe<T>>
{
    public bool HasValue => _isValueSet;
    public bool HasNoValue => !HasValue;
    public static Maybe<T> None => new();
    public static Maybe<T> From(T? obj) => new(obj);

    /// <summary>
    /// Returns the inner value if there's one, otherwise throws an InvalidOperationException with <paramref name="errorMessage"/>
    /// </summary>
    /// <exception cref="InvalidOperationException">Maybe has no value.</exception>
    public T GetValueOrThrow(string? errorMessage = null)
    {
        if (HasNoValue || _value is null)
            throw new InvalidOperationException(errorMessage ?? NoValueException);

        return _value;
    }

    public static implicit operator Maybe<T>(T? value)
    {
        if (value is Maybe<T> m)
            return m;

        return From(value);
    }

    public static implicit operator Maybe<T>(Maybe value) => None;

    public override string ToString() => _value?.ToString() ?? NoValue;

    public bool TryGetValue(out T? value)
    {
        value = _value;
        return HasValue;
    }

    public static bool operator ==(Maybe<T> maybe, T value)
    {
        if (value is Maybe<T>)
            return maybe.Equals(value);

        if (maybe.HasNoValue)
            return value is null;

        return maybe._value?.Equals(value) ?? false;
    }

    public static bool operator !=(Maybe<T> maybe, T value) => !(maybe == value);

    public static bool operator ==(Maybe<T> maybe, object other) => maybe.Equals(other);

    public static bool operator !=(Maybe<T> maybe, object other) => !(maybe == other);

    public static bool operator ==(Maybe<T> first, Maybe<T> second) => first.Equals(second);

    public static bool operator !=(Maybe<T> first, Maybe<T> second) => !(first == second);

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (obj is Maybe<T> other)
            return Equals(other);
        if (obj is T value)
            return Equals(value);
        return false;
    }

    public bool Equals(Maybe<T> other)
    {
        if (HasNoValue && other.HasNoValue)
            return true;

        if (HasNoValue || other.HasNoValue)
            return false;

        return EqualityComparer<T>.Default.Equals(_value, other._value);
    }

    public override int GetHashCode()
    {
        if (HasNoValue)
            return 0;

        return _value?.GetHashCode() ?? 0;
    }

    private readonly T _value;
    private readonly bool _isValueSet;

    public static readonly string NoValueException = "Maybe has no value.";
    private static readonly string NoValue = "No value";

    private Maybe(T? value)
    {
        if (value == null)
        {
            _isValueSet = false;
            _value = default!;
            return;
        }

        _isValueSet = true;
        _value = value;
    }
}

/// <summary>
/// Non-generic entrypoint for <see cref="Maybe{T}" /> members
/// </summary>
public readonly struct Maybe
{
    public static Maybe None => new();

    /// <summary>
    /// Creates a new <see cref="Maybe{T}" /> from the provided <paramref name="value"/>
    /// </summary>
    public static Maybe<T> From<T>(T value) => Maybe<T>.From(value);
}
