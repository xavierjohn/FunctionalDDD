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
    /// Helps convert optional primitive types to strongly typed object.
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
    ///             <term><paramref name="value"/> is null</term>
    ///             <description>Maybe&lt;<typeparamref name="TOut"/>&gt; without value.</description>
    ///         </item>
    ///         <item>
    ///             <term><paramref name="value"/> is not null and <paramref name="function"/> returned Success</term>
    ///             <description>Maybe&lt;<typeparamref name="TOut"/>&gt; with value from <paramref name="function"/>.</description>
    ///         </item>
    ///         <item>
    ///             <term><paramref name="value"/> is not null and <paramref name="function"/> returned Failure</term>
    ///             <description>The <see cref="Error" /> from the <paramref name="function"/>.</description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <example>
    /// This code snippet demonstrates how to transform an optional string representing a zipcode into a strongly typed Zipcode Maybe object.
    /// <para>
    /// If the string is null, the method returns a Maybe&lt;Zipcode&gt; of None.<br/>
    /// Otherwise, it invokes the specified function and, if the function is successful, it wraps the result in a Maybe object.<br/>
    /// If the function fails, the method returns the failure. This is useful when you want to transform a string into a strongly typed object with validation.
    /// </para>
    /// <code>
    /// string? zipCode = "98052";
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
