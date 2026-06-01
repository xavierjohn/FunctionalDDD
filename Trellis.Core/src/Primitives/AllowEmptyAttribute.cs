namespace Trellis;

using System;

/// <summary>
/// Permits the per-base "empty" sentinel on a <see cref="RequiredString{TSelf}"/> or
/// <see cref="RequiredGuid{TSelf}"/>-derived class:
/// <list type="bullet">
///   <item><see cref="RequiredString{TSelf}"/> — permits <see cref="string.Empty"/> as the final value.</item>
///   <item><see cref="RequiredGuid{TSelf}"/> — permits <see cref="System.Guid.Empty"/> as the final value.</item>
/// </list>
/// </summary>
/// <remarks>
/// <para>
/// By default the generator rejects each base's BCL "empty" sentinel. Apply this attribute
/// when the empty value is a legitimate domain value (sparse extension fields at integration
/// seams, response bodies that may be empty by protocol, etc.).
/// </para>
/// <para>
/// On <see cref="RequiredString{TSelf}"/>: permits the post-trim final string to be empty.
/// Does <em>not</em> by itself permit whitespace-only input — combine with
/// <see cref="AllowWhitespaceAttribute"/> when whitespace-only strings should also be
/// accepted. The trim step still runs unless <see cref="NoTrimAttribute"/> is also applied.
/// </para>
/// <para>
/// On <see cref="RequiredGuid{TSelf}"/>: permits <c>Guid.Empty</c> (the all-zero GUID) as a
/// valid stored value.
/// </para>
/// <para>
/// Only applies to <see cref="RequiredString{TSelf}"/> and <see cref="RequiredGuid{TSelf}"/>.
/// Applying to numeric Required bases (use <see cref="AllowZeroAttribute"/>) or
/// date Required bases (use <see cref="AllowMinValueAttribute"/>) produces a generator error.
/// </para>
/// </remarks>
/// <seealso cref="AllowWhitespaceAttribute"/>
/// <seealso cref="NoTrimAttribute"/>
/// <seealso cref="AllowZeroAttribute"/>
/// <seealso cref="AllowMinValueAttribute"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowEmptyAttribute : Attribute
{
}

