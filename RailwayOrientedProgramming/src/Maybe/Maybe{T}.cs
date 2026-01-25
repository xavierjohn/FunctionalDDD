namespace FunctionalDdd;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents an optional value that may or may not exist. Similar to <see cref="Nullable{T}"/> but works with both value and reference types.
/// Use this type to make optionality explicit in your domain model and avoid null reference exceptions.
/// </summary>
/// <typeparam name="T">The type of the optional value. Must be a non-null type.</typeparam>
/// <example>
/// <code>
/// // Create a Maybe with a value
/// Maybe&lt;string&gt; name = "John";
/// if (name.HasValue) Console.WriteLine(name.Value);
/// 
/// // Create an empty Maybe
/// Maybe&lt;string&gt; noName = Maybe.None&lt;string&gt;();
/// string result = noName.GetValueOrDefault("Default");
/// </code>
/// </example>
public readonly struct Maybe<T> :
    IEquatable<T>,
    IEquatable<Maybe<T>>
{
    private readonly bool _isValueSet;
    private readonly T? _value;

    private const string NoValue = "Maybe has no value.";

    /// <summary>
    /// Gets the underlying value if present, otherwise throws an exception.
    /// </summary>
    /// <param name="errorMessage">Optional custom error message to use when throwing.</param>
    /// <returns>The underlying value of type <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="HasValue"/> is false.</exception>
    /// <remarks>
    /// Prefer <see cref="GetValueOrDefault"/> or <see cref="TryGetValue"/> to avoid exceptions.
    /// </remarks>
    public T GetValueOrThrow(string? errorMessage = null)
    {
        if (_isValueSet)
            return _value!;

        throw new InvalidOperationException(errorMessage ?? NoValue);
    }

    /// <summary>
    /// Gets the underlying value if present, otherwise returns the specified default value.
    /// </summary>
    /// <param name="defaultValue">The value to return when <see cref="HasValue"/> is false.</param>
    /// <returns>The underlying value or <paramref name="defaultValue"/>.</returns>
    /// <remarks>
    /// This is the recommended way to safely extract values from Maybe.
    /// </remarks>
    public T GetValueOrDefault(T defaultValue)
    {
        if (_isValueSet)
            return _value!;

        return defaultValue;
    }

    /// <summary>
    /// Attempts to get the underlying value without throwing an exception.
    /// </summary>
    /// <param name="value">When this method returns true, contains the underlying value; otherwise, the default value for type <typeparamref name="T"/>.</param>
    /// <returns>True if a value is present; otherwise false.</returns>
    /// <remarks>
    /// Similar to the TryParse pattern in .NET, this provides a safe way to check for and retrieve values.
    /// </remarks>
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return _isValueSet;
    }

    /// <summary>
    /// Gets the underlying value if present; otherwise throws an exception.
    /// </summary>
    /// <value>The underlying value of type <typeparamref name="T"/>.</value>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="HasValue"/> is false.</exception>
    /// <remarks>
    /// Always check <see cref="HasValue"/> before accessing this property, or use <see cref="GetValueOrDefault"/> instead.
    /// </remarks>
    public T Value => GetValueOrThrow();

    /// <summary>
    /// Gets a value indicating whether this instance contains a value.
    /// </summary>
    /// <value>True if a value is present; otherwise false.</value>
    public bool HasValue => _isValueSet;

    /// <summary>
    /// Gets a value indicating whether this instance contains no value.
    /// </summary>
    /// <value>True if no value is present; otherwise false.</value>
    /// <remarks>
    /// This is the logical inverse of <see cref="HasValue"/>. Use whichever makes your code more readable.
    /// </remarks>
    public bool HasNoValue => !_isValueSet;

    internal Maybe(T? value)
    {
        _isValueSet = value is not null;
        _value = value!;
    }

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/> to a <see cref="Maybe{T}"/>.
    /// </summary>
    /// <param name="value">The value to wrap. If null, creates an empty Maybe.</param>
    /// <returns>A Maybe containing the value, or an empty Maybe if value is null.</returns>
    public static implicit operator Maybe<T>(T? value) =>
        value is Maybe<T> maybe ? maybe : new(value);

    /// <summary>
    /// Determines whether a <see cref="Maybe{T}"/> equals a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="maybe">The Maybe instance to compare.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>True if the Maybe has a value equal to <paramref name="value"/>; otherwise false.</returns>
    public static bool operator ==(Maybe<T> maybe, T value) => maybe.Equals(value);

    /// <summary>
    /// Determines whether a <see cref="Maybe{T}"/> does not equal a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="maybe">The Maybe instance to compare.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>True if the Maybe does not have a value equal to <paramref name="value"/>; otherwise false.</returns>
    public static bool operator !=(Maybe<T> maybe, T value) => !maybe.Equals(value);

    /// <summary>
    /// Determines whether a <see cref="Maybe{T}"/> equals another object.
    /// </summary>
    /// <param name="maybe">The Maybe instance to compare.</param>
    /// <param name="other">The object to compare against.</param>
    /// <returns>True if equal; otherwise false.</returns>
    public static bool operator ==(Maybe<T> maybe, object? other) => maybe.Equals(other);

    /// <summary>
    /// Determines whether a <see cref="Maybe{T}"/> does not equal another object.
    /// </summary>
    /// <param name="maybe">The Maybe instance to compare.</param>
    /// <param name="other">The object to compare against.</param>
    /// <returns>True if not equal; otherwise false.</returns>
    public static bool operator !=(Maybe<T> maybe, object? other) => !maybe.Equals(other);

    /// <summary>
    /// Determines whether two <see cref="Maybe{T}"/> instances are equal.
    /// </summary>
    /// <param name="first">The first Maybe instance.</param>
    /// <param name="second">The second Maybe instance.</param>
    /// <returns>True if both have no value, or both have equal values; otherwise false.</returns>
    public static bool operator ==(Maybe<T> first, Maybe<T> second) => first.Equals(second);

    /// <summary>
    /// Determines whether two <see cref="Maybe{T}"/> instances are not equal.
    /// </summary>
    /// <param name="first">The first Maybe instance.</param>
    /// <param name="second">The second Maybe instance.</param>
    /// <returns>True if the instances are not equal; otherwise false.</returns>
    public static bool operator !=(Maybe<T> first, Maybe<T> second) => !first.Equals(second);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj switch
        {
            Maybe<T> other => Equals(other),
            T other => Equals(other),
            _ => false,
        };

    /// <inheritdoc />
    public bool Equals(Maybe<T> other) =>
        _isValueSet && other._isValueSet
            ? EqualityComparer<T>.Default.Equals(_value, other._value)
            : !_isValueSet && !other._isValueSet;

    /// <inheritdoc />
    public bool Equals(T? other) =>
        (_isValueSet && EqualityComparer<T>.Default.Equals(_value, other))
        || (!_isValueSet && other is null);

    /// <inheritdoc />
    public override int GetHashCode() => _isValueSet
        ? (_value?.GetHashCode() ?? 0)
        : 0;

    /// <inheritdoc />
    public override string ToString() => _value?.ToString() ?? string.Empty;
}