namespace Trellis;

using System;

/// <summary>
/// Permits whitespace-only input on a <see cref="RequiredString{TSelf}"/>-derived class.
/// </summary>
/// <remarks>
/// <para>
/// By default the generator rejects whitespace-only input. Apply this attribute when
/// whitespace-only strings are a legitimate domain value at integration seams.
/// </para>
/// <para>
/// Important interaction with trim: by default the generator trims input after validating
/// it. When <c>[AllowWhitespace]</c> is applied without <see cref="NoTrimAttribute"/>,
/// whitespace-only input is <em>accepted</em> but then normalized to <see cref="string.Empty"/>
/// by the trim step. If whitespace should be preserved verbatim, combine with
/// <see cref="NoTrimAttribute"/>.
/// </para>
/// <para>
/// Does <em>not</em> by itself permit literal empty input (<see cref="string.Empty"/>); combine
/// with <see cref="AllowEmptyAttribute"/> when both literal empty and whitespace-only should
/// be accepted.
/// </para>
/// <para>
/// Only applies to <see cref="RequiredString{TSelf}"/>. Applying to other Required bases
/// produces a generator error.
/// </para>
/// </remarks>
/// <seealso cref="AllowEmptyAttribute"/>
/// <seealso cref="NoTrimAttribute"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowWhitespaceAttribute : Attribute
{
}

