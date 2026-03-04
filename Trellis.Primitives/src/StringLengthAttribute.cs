namespace Trellis;

/// <summary>
/// Specifies the minimum and maximum length of characters that are allowed in a
/// <see cref="RequiredString{TSelf}"/>-derived value object.
/// </summary>
/// <remarks>
/// <para>
/// When applied to a <c>partial class</c> inheriting from <see cref="RequiredString{TSelf}"/>,
/// the source generator automatically includes length validation in the generated <c>TryCreate</c> method.
/// The length check runs after the null/empty/whitespace validation, so the length is checked on a
/// non-whitespace string.
/// </para>
/// <para>
/// This attribute is designed specifically for Trellis value objects and is processed at compile time
/// by the PrimitiveValueObjectGenerator source generator. It does not rely on runtime reflection.
/// </para>
/// </remarks>
/// <example>
/// Maximum length only:
/// <code>
/// [StringLength(50)]
/// public partial class FirstName : RequiredString&lt;FirstName&gt; { }
///
/// // Generated TryCreate validates:
/// // - Not null/empty/whitespace
/// // - Length &lt;= 50
/// </code>
/// </example>
/// <example>
/// Both minimum and maximum length:
/// <code>
/// [StringLength(100, MinLength = 3)]
/// public partial class Description : RequiredString&lt;Description&gt; { }
///
/// // Generated TryCreate validates:
/// // - Not null/empty/whitespace
/// // - Length &gt;= 3
/// // - Length &lt;= 100
/// </code>
/// </example>
/// <seealso cref="RequiredString{TSelf}"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class StringLengthAttribute : Attribute
{
    /// <summary>
    /// Gets the maximum allowable length of the string.
    /// </summary>
    /// <value>The maximum length, inclusive.</value>
    public int MaximumLength { get; }

    /// <summary>
    /// Gets or sets the minimum allowable length of the string.
    /// </summary>
    /// <value>The minimum length, inclusive. Defaults to 0 (no minimum beyond non-empty).</value>
    public int MinimumLength { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringLengthAttribute"/> class
    /// with the specified maximum length.
    /// </summary>
    /// <param name="maximumLength">
    /// The maximum length, inclusive. Must be zero or greater.
    /// A value of zero means only empty strings are valid (which would conflict with RequiredString's
    /// non-empty requirement, so in practice use 1 or greater).
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maximumLength"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// [StringLength(50)]
    /// public partial class FirstName : RequiredString&lt;FirstName&gt; { }
    /// </code>
    /// </example>
    public StringLengthAttribute(int maximumLength)
    {
        if (maximumLength < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumLength), maximumLength, "Maximum length must be zero or greater.");

        MaximumLength = maximumLength;
    }
}
