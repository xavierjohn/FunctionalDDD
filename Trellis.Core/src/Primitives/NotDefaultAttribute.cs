namespace Trellis;

using System;

/// <summary>
/// <strong>Vestigial under the post-v3 defaults.</strong> Previously the opt-in to per-type
/// sentinel-rejection on Required value objects. After the v3 defaults realignment, sentinel
/// rejection is the default for every applicable base, so this attribute is now a no-op.
/// Existing decorations stay valid (the generator silently ignores them) so legacy fixtures
/// continue to compile; a generator diagnostic flags them as redundant.
/// </summary>
/// <remarks>
/// <para>
/// Migration: remove <c>[NotDefault]</c> from existing classes — the strict behavior it used
/// to opt into is now the default. To <em>opt out</em> of the strict default, use the per-type
/// attribute: <see cref="AllowEmptyAttribute"/> for <see cref="RequiredString{TSelf}"/> and
/// <see cref="RequiredGuid{TSelf}"/>, <see cref="AllowZeroAttribute"/> for
/// <see cref="RequiredInt{TSelf}"/> / <see cref="RequiredLong{TSelf}"/> /
/// <see cref="RequiredDecimal{TSelf}"/>, <see cref="AllowMinValueAttribute"/> for
/// <see cref="RequiredDateTime{TSelf}"/> / <see cref="RequiredDateTimeOffset{TSelf}"/>.
/// </para>
/// <para>
/// The attribute is kept defined (rather than deleted outright) so the v3 migration is a
/// no-op for tests and fixtures that previously decorated with <c>[NotDefault]</c> — they
/// continue to express the strict behavior they always wanted, just via the new default
/// instead of an attribute.
/// </para>
/// </remarks>
/// <seealso cref="AllowEmptyAttribute"/>
/// <seealso cref="AllowZeroAttribute"/>
/// <seealso cref="AllowMinValueAttribute"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NotDefaultAttribute : Attribute
{
}

