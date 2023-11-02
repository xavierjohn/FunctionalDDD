namespace FunctionalDdd;

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

    /// <summary>
    /// Helps convert optional primite types to strongly typed object.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="value"></param>
    /// <param name="function">A function that can validate the input and return a <see cref="Result{TOut}" /></param>
    /// <returns>
    ///     <list type="table">
    ///         <listheader>
    ///             <term>State</term>
    ///             <description>Return</description>
    ///         </listheader>
    ///         <item>
    ///             <term>Value is null</term>
    ///             <description>Maybe.None&lt;<typeparamref name="TOut"/>&gt;</description>
    ///         </item>
    ///         <item>
    ///             <term>Value is not null and <paramref name="function"/> returned Result.Success</term>
    ///             <description>Maybe.From( the <typeparamref name="TOut"/> value from <paramref name="function"/>)</description>
    ///         </item>
    ///         <item>
    ///             <term>Value is not null and <paramref name="function"/> returned Result.Failure</term>
    ///             <description>The <see cref="Error" /> from the <paramref name="function"/> return value.</description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <example>
    /// This code snippet demonstrates how to transform an optional string representing a zipcode into a strongly typed Zipcode Maybe object.
    /// If the string is null, the method returns a Maybe of None.
    /// Otherwise, it invokes the specified function and, if the function's result is successful, it wraps the result in a Maybe object.
    /// If the function fails, the method returns the failure.  This lets the given function run validation on the input data.
    /// <code>
    /// var result = Maybe.Optional(zipCode, ZipCode.TryCreate);
    /// </code>
    /// </example>
    public static Result<Maybe<TOut>> Optional<TIn, TOut>(TIn? value, Func<TIn, Result<TOut>> function)
    where TOut : notnull
    {
        if (value is null)
            return Maybe.None<TOut>();

        return function(value).Map(r => Maybe.From(r));
    }
}
