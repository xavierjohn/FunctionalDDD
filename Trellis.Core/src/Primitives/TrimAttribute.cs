namespace Trellis;

using System;

/// <summary>
/// <strong>Vestigial under the post-v3 defaults.</strong> Previously the opt-in to trim
/// before validation on <see cref="RequiredString{TSelf}"/>. After the v3 defaults
/// realignment, trim runs by default on <see cref="RequiredString{TSelf}"/>, so this
/// attribute is now a no-op. Existing decorations stay valid (the generator silently ignores
/// them) so legacy fixtures continue to compile; a generator diagnostic flags them as
/// redundant.
/// </summary>
/// <remarks>
/// <para>
/// Migration: remove <c>[Trim]</c> from existing classes — the trim behavior it used to opt
/// into is now the default. To <em>opt out</em> of automatic trim, use
/// <see cref="NoTrimAttribute"/>.
/// </para>
/// <para>
/// The attribute is kept defined (rather than deleted outright) so the v3 migration is a
/// no-op for fixtures that previously decorated with <c>[Trim]</c> — they continue to express
/// the trim behavior they always wanted, just via the new default instead of an attribute.
/// </para>
/// </remarks>
/// <seealso cref="NoTrimAttribute"/>
/// <seealso cref="AllowEmptyAttribute"/>
/// <seealso cref="AllowWhitespaceAttribute"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class TrimAttribute : Attribute
{
}

