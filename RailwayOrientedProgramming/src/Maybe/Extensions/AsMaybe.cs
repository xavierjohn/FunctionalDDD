namespace FunctionalDDD;

using System.Diagnostics.CodeAnalysis;

public static partial class MaybeExtensions
{
    /// <summary>
    /// Converts the <see cref="Nullable"/> struct to a <see cref="Maybe{T}"/>.
    /// </summary>
    /// <returns>Returns the <see cref="Maybe{T}"/> equivalent to the <see cref="Nullable{T}"/>.</returns>
    public static Maybe<T> AsMaybe<T>(ref this T? value) where T : struct =>
        value is null ? default : new(value.Value);

    /// <summary>
    /// Wraps the class instance in a <see cref="Maybe{T}"/>.
    /// </summary>
    /// <returns>Returns <see cref="Maybe.None"/> if the class instance is null, otherwise returns <see cref="Maybe.From{T}(T)"/>.</returns>
    public static Maybe<T> AsMaybe<T>([MaybeNull] this T value) where T : class =>
            value is null ? default : new(value);
}
