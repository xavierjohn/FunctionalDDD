namespace Trellis;

using System;

/// <summary>
/// Skips the automatic trim step on a <see cref="RequiredString{TSelf}"/>-derived class.
/// </summary>
/// <remarks>
/// <para>
/// By default the generator trims input before storing it. Apply this attribute to preserve
/// leading/trailing whitespace verbatim — useful for machine identifiers carrying intentional
/// spacing, raw protocol fragments, or any case where whitespace is part of the stored value.
/// </para>
/// <para>
/// Does <em>not</em> by itself affect what input is <em>accepted</em>; it only affects what
/// is <em>stored</em>. Whitespace-only input is still rejected unless paired with
/// <see cref="AllowWhitespaceAttribute"/>; literal empty input is still rejected unless paired
/// with <see cref="AllowEmptyAttribute"/>.
/// </para>
/// <para>
/// Only applies to <see cref="RequiredString{TSelf}"/>. Applying to other Required bases
/// produces a generator error.
/// </para>
/// </remarks>
/// <seealso cref="AllowEmptyAttribute"/>
/// <seealso cref="AllowWhitespaceAttribute"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NoTrimAttribute : Attribute
{
}

