namespace Trellis;

using System;

/// <summary>
/// Marks a partial <see cref="RequiredString{TSelf}"/>-derived class so the source generator
/// trims the input string before any subsequent validation runs.
/// </summary>
/// <remarks>
/// <para>
/// Removed from the default <see cref="RequiredString{TSelf}"/> behavior (which previously
/// always trimmed) so the type does exactly what its attributes declare and nothing more.
/// Apply <c>[Trim]</c> when leading / trailing whitespace must be stripped before storage.
/// </para>
/// <para>
/// Validation order in the generated <c>TryCreate</c>:
/// <list type="number">
///   <item>Null check (always).</item>
///   <item><see cref="TrimAttribute"/> (if present).</item>
///   <item><see cref="NotDefaultAttribute"/> (if present).</item>
///   <item><see cref="StringLengthAttribute"/> (if present, measures the post-trim value).</item>
///   <item>Consumer <c>ValidateAdditional</c> override.</item>
/// </list>
/// </para>
/// <para>
/// <c>[Trim]</c> on its own (without <see cref="NotDefaultAttribute"/>) trims and stores the
/// result verbatim, even if the result is <see cref="string.Empty"/>. Combine with
/// <c>[NotDefault]</c> to reject empty / whitespace-only input — that is the recommended
/// default for any string mapped to a database column.
/// </para>
/// <para>
/// Not supported on non-string Required types (<c>RequiredInt</c>, <c>RequiredGuid</c>, etc.).
/// The source generator emits a diagnostic and the <c>TRLS</c> analyzer surfaces the same
/// error in the IDE.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [Trim, NotDefault]
/// public partial class FirstName : RequiredString&lt;FirstName&gt; { }
///
/// FirstName.TryCreate(" John ");  // Success: stored as "John"
/// FirstName.TryCreate("   ");     // Failure: trim -&gt; "" -&gt; [NotDefault]
/// </code>
/// </example>
/// <seealso cref="NotDefaultAttribute"/>
/// <seealso cref="StringLengthAttribute"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class TrimAttribute : Attribute
{
}
