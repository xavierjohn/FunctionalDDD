namespace Trellis;

using System;

/// <summary>
/// Reserved marker for the upcoming <c>Required&lt;TSelf&gt;</c> defaults realignment: when applied to a
/// partial <see cref="RequiredString{TSelf}"/>-derived class, the generator will permit
/// whitespace-only strings to satisfy the "required" validation once the strict-default
/// emission flip lands. <b>This attribute has no effect in the current release</b> — declare it
/// now to keep existing code lenient through the upcoming flip without code churn.
/// </summary>
/// <remarks>
/// <para>
/// Once the flip lands, the generator's default rejects whitespace-only input (after any
/// <see cref="TrimAttribute"/> trim runs). Allowing whitespace-only input does not by itself
/// allow empty input — combine with <see cref="AllowEmptyAttribute"/> when both should be
/// accepted. Combining with <see cref="NoTrimAttribute"/> preserves whitespace verbatim instead
/// of trimming first.
/// </para>
/// </remarks>
/// <seealso cref="AllowEmptyAttribute"/>
/// <seealso cref="NoTrimAttribute"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowWhitespaceAttribute : Attribute
{
}
