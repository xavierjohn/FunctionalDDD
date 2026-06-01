namespace Trellis;

using System;

/// <summary>
/// Permits the literal value <c>0</c> on a numeric <see cref="RequiredInt{TSelf}"/>,
/// <see cref="RequiredLong{TSelf}"/>, or <see cref="RequiredDecimal{TSelf}"/>-derived class.
/// </summary>
/// <remarks>
/// <para>
/// By default the generator rejects <c>0</c> as the CLR sentinel value on numeric Required
/// bases. Apply this attribute when zero is a legitimate domain value — counters,
/// offsets, balances that can settle to exactly zero, etc.
/// </para>
/// <para>
/// Conflicts with <see cref="PositiveAttribute"/> (which rejects values <c>&lt;= 0</c>) and
/// <see cref="NegativeAttribute"/> (which rejects values <c>&gt;= 0</c>); pairing this attribute
/// with either produces a generator diagnostic. Compatible with
/// <see cref="NonNegativeAttribute"/> (already permits <c>0</c>) and
/// <see cref="NonPositiveAttribute"/> (already permits <c>0</c>), in which case
/// <c>[AllowZero]</c> is redundant — a generator diagnostic flags this.
/// </para>
/// <para>
/// Only applies to numeric Required bases. Applying to <see cref="RequiredString{TSelf}"/>,
/// <see cref="RequiredGuid{TSelf}"/>, <see cref="RequiredDateTime{TSelf}"/>,
/// <see cref="RequiredDateTimeOffset{TSelf}"/>, or <see cref="RequiredBool{TSelf}"/>
/// produces a generator error — use the per-type opt-out
/// (<see cref="AllowEmptyAttribute"/> / <see cref="AllowMinValueAttribute"/>) instead.
/// </para>
/// </remarks>
/// <seealso cref="PositiveAttribute"/>
/// <seealso cref="NonNegativeAttribute"/>
/// <seealso cref="NegativeAttribute"/>
/// <seealso cref="NonPositiveAttribute"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowZeroAttribute : Attribute
{
}
