namespace Trellis;

using System;

/// <summary>
/// Marks a partial <see cref="RequiredString{TSelf}"/> / <see cref="RequiredInt{TSelf}"/> /
/// <see cref="RequiredLong{TSelf}"/> / <see cref="RequiredDecimal{TSelf}"/> /
/// <see cref="RequiredGuid{TSelf}"/> / <see cref="RequiredDateTime{TSelf}"/> -derived class so
/// the source generator emits an additional check that rejects the type's "zero value":
/// <see cref="string.Empty"/> for strings, <c>0</c> for numerics,
/// <see cref="Guid.Empty"/> for GUIDs, <see cref="DateTime.MinValue"/> for date-times.
/// </summary>
/// <remarks>
/// <para>
/// The default behavior of every <c>RequiredXxx&lt;T&gt;</c> base is to reject only <c>null</c>;
/// the per-type "zero value" rejection is opt-in via this attribute. Mirrors the BCL
/// <c>[NotNull]</c> / <c>[NotNullWhen]</c> naming pattern.
/// </para>
/// <para>
/// The attribute name is shorthand for "reject this type's zero/sentinel value", not literally
/// <c>default(T)</c> (which for <see cref="string"/> is <c>null</c>, not empty). The generator
/// picks the rejection rule from the underlying primitive:
/// <list type="bullet">
///   <item><c>RequiredString&lt;T&gt;</c>: rejects <see cref="string.Empty"/>. When combined
///     with <see cref="TrimAttribute"/>, the trim runs first, so whitespace-only input also
///     becomes a rejection.</item>
///   <item><c>RequiredInt&lt;T&gt;</c> / <c>RequiredLong&lt;T&gt;</c> / <c>RequiredDecimal&lt;T&gt;</c>:
///     rejects <c>0</c>.</item>
///   <item><c>RequiredGuid&lt;T&gt;</c>: rejects <see cref="Guid.Empty"/>.</item>
///   <item><c>RequiredDateTime&lt;T&gt;</c>: rejects <see cref="DateTime.MinValue"/>.</item>
/// </list>
/// </para>
/// <para>
/// Not supported on <c>RequiredBool&lt;T&gt;</c> (a bool that rejects <c>false</c> has only one
/// constructible value, which is degenerate) or on <c>RequiredEnum&lt;T&gt;</c> (smart-enum
/// members are always declared values; there is no CLR default to reject). The source generator
/// emits a diagnostic for both invalid combinations, and the <c>TRLS</c> analyzer surfaces the
/// same error in the IDE.
/// </para>
/// <para>
/// Validation order in the generated <c>TryCreate</c>:
/// <list type="number">
///   <item>Null check (always).</item>
///   <item><see cref="TrimAttribute"/> (string only; if present).</item>
///   <item><see cref="NotDefaultAttribute"/> (if present).</item>
///   <item><see cref="StringLengthAttribute"/> / <see cref="RangeAttribute"/> (if present).</item>
///   <item>Consumer <c>ValidateAdditional</c> override.</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Preserve today's "reject empty + whitespace" behavior on a string value object:
/// <code>
/// [Trim, NotDefault]
/// public partial class FirstName : RequiredString&lt;FirstName&gt; { }
///
/// FirstName.TryCreate("John");      // Success
/// FirstName.TryCreate("");          // Failure: "First Name cannot be empty."
/// FirstName.TryCreate("   ");       // Failure: [Trim] -&gt; "" -&gt; [NotDefault]
/// FirstName.TryCreate(" John ");    // Success: stored as "John"
/// </code>
/// </example>
/// <example>
/// Reject the zero ID on a numeric domain identifier:
/// <code>
/// [NotDefault]
/// public partial class OrderNumber : RequiredInt&lt;OrderNumber&gt; { }
///
/// OrderNumber.TryCreate(0);   // Failure: "Order Number cannot be zero."
/// OrderNumber.TryCreate(42);  // Success
/// </code>
/// </example>
/// <example>
/// Reject the empty GUID on an entity identifier:
/// <code>
/// [NotDefault]
/// public partial class CustomerId : RequiredGuid&lt;CustomerId&gt; { }
///
/// CustomerId.TryCreate(Guid.Empty); // Failure: "Customer Id cannot be Guid.Empty."
/// CustomerId.NewUniqueV7();         // Success
/// </code>
/// </example>
/// <seealso cref="TrimAttribute"/>
/// <seealso cref="StringLengthAttribute"/>
/// <seealso cref="RangeAttribute"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NotDefaultAttribute : Attribute
{
}
