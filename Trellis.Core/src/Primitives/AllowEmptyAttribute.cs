namespace Trellis;

using System;

/// <summary>
/// Reserved marker for the upcoming <c>Required&lt;TSelf&gt;</c> defaults realignment: when applied to a
/// partial <see cref="RequiredString{TSelf}"/>-derived class, the generator will permit empty
/// strings (<c>""</c>) to satisfy the "required" validation once the strict-default emission
/// flip lands. <b>This attribute has no effect in the current release</b> — declare it now to
/// keep existing code lenient through the upcoming flip without code churn.
/// </summary>
/// <remarks>
/// <para>
/// Once the flip lands, the generator's default rejects empty strings on
/// <see cref="RequiredString{TSelf}"/>; with this attribute, only <c>null</c> is rejected and
/// the empty string is accepted as a valid value. Useful for boundary types that legitimately
/// carry an empty payload (sparse extension fields at integration seams, response bodies that
/// may be empty by protocol, etc.). For most domain types, leaving this attribute off and
/// letting the generator reject empty strings will be the correct choice.
/// </para>
/// <para>
/// Does <em>not</em> permit whitespace-only input on its own; combine with
/// <see cref="AllowWhitespaceAttribute"/> when whitespace-only strings should also be accepted.
/// </para>
/// </remarks>
/// <seealso cref="AllowWhitespaceAttribute"/>
/// <seealso cref="NoTrimAttribute"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowEmptyAttribute : Attribute
{
}
