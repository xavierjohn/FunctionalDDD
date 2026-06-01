namespace Trellis;

using System;

/// <summary>
/// Reserved marker for the upcoming <c>Required&lt;TSelf&gt;</c> defaults realignment: when applied to a
/// partial <see cref="RequiredString{TSelf}"/>-derived class, the generator will skip the
/// trim-before-validate step once the strict-default emission flip lands. <b>This attribute
/// has no effect in the current release</b> — today, trim only runs when
/// <see cref="TrimAttribute"/> is applied explicitly. Declare it now to lock in the verbatim
/// behavior across the upcoming flip without code churn.
/// </summary>
/// <remarks>
/// <para>
/// Once the flip lands, the generator trims by default; this attribute opts out so the input
/// string is stored verbatim. Use when leading/trailing whitespace is a meaningful part of the
/// stored value (machine identifiers carrying intentional spacing, raw protocol fragments,
/// etc.). For most domain strings — names, emails, codes — leaving this attribute off and
/// letting the generator trim will be the correct choice.
/// </para>
/// </remarks>
/// <seealso cref="TrimAttribute"/>
/// <seealso cref="AllowEmptyAttribute"/>
/// <seealso cref="AllowWhitespaceAttribute"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NoTrimAttribute : Attribute
{
}
