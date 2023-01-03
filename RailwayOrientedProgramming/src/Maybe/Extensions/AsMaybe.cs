namespace FunctionalDDD;

public static partial class MaybeExtensions
{
    /// <summary>
    /// Converts the <see cref="Nullable"/> struct to a <see cref="Maybe{T}"/>.
    /// </summary>
    /// <returns>Returns the <see cref="Maybe{T}"/> equivalent to the <see cref="Nullable{T}"/>.</returns>
    public static Maybe<T> AsMaybe<T>(ref this T? value) where T : struct =>
         value.HasValue ? new(value.Value) : default;

    /// <summary>
    /// Wraps the class instance in a <see cref="Maybe{T}"/>.
    /// </summary>
    /// <returns>Returns <see cref="Maybe.None"/> if the class instance is null, otherwise returns <see cref="Maybe.From{T}(T)"/>.</returns>
    public static Maybe<T> AsMaybe<T>(this T? value) where T : class
    {
        if (value is not null)
            return value!;

        return default;
    }
}
