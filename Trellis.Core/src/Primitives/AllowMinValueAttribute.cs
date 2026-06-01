namespace Trellis;

using System;

/// <summary>
/// Permits the BCL minimum value (<see cref="System.DateTime.MinValue"/> /
/// <see cref="System.DateTimeOffset.MinValue"/>, i.e., year 1 AD) on a
/// <see cref="RequiredDateTime{TSelf}"/> or <see cref="RequiredDateTimeOffset{TSelf}"/>-derived
/// class.
/// </summary>
/// <remarks>
/// <para>
/// By default the generator rejects <see cref="System.DateTime.MinValue"/> /
/// <see cref="System.DateTimeOffset.MinValue"/> as the CLR <c>default(T)</c> sentinel value.
/// Apply this attribute when the minimum value is a legitimate domain value — typically at
/// integration seams with legacy systems that round-trip uninitialized timestamps, or sparse
/// historical data where the absence of a timestamp is meaningfully encoded as the BCL
/// minimum.
/// </para>
/// <para>
/// The attribute name follows the BCL's own naming for the value (<c>DateTime.MinValue</c>,
/// <c>DateTimeOffset.MinValue</c>). It refers to the CLR sentinel, not a domain-supplied
/// minimum from <c>[Range]</c>-style constraints; the attribute does not bypass any
/// user-supplied minimum.
/// </para>
/// <para>
/// Only applies to <see cref="RequiredDateTime{TSelf}"/> and
/// <see cref="RequiredDateTimeOffset{TSelf}"/>. Applying to other Required bases produces a
/// generator error — use the per-type opt-out (<see cref="AllowEmptyAttribute"/> for strings
/// and <see cref="RequiredGuid{TSelf}"/>, <see cref="AllowZeroAttribute"/> for numerics).
/// </para>
/// </remarks>
/// <seealso cref="AllowEmptyAttribute"/>
/// <seealso cref="AllowZeroAttribute"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowMinValueAttribute : Attribute
{
}
