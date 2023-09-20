namespace FunctionalDDD.Results;

/// <summary>
/// Contains static methods to create a <see cref="Maybe{T}"/> object.
/// </summary>
public static class Maybe
{
    /// <summary>
    /// Creates a new <see cref="Maybe{T}"/> with no value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns><see cref="Maybe{T}"/> object with no value.</returns>
    public static Maybe<T> None<T>() where T : notnull => new();

    /// <summary>
    /// Creates a new <see cref="Maybe{T}"/> with a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns>A <see cref="Maybe{T}"/> object with the value.</returns>
    public static Maybe<T> From<T>(T? value) where T : notnull => new(value);
}
