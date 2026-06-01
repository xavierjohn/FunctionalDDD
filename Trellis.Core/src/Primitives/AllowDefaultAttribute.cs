namespace Trellis;

using System;

/// <summary>
/// Reserved marker for the upcoming <c>Required&lt;TSelf&gt;</c> defaults realignment: when applied to a
/// partial <see cref="RequiredGuid{TSelf}"/>, <see cref="RequiredDateTime{TSelf}"/>, or
/// <see cref="RequiredDateTimeOffset{TSelf}"/>-derived class, the generator will permit the
/// CLR default value (<see cref="System.Guid.Empty"/>, <see cref="System.DateTime.MinValue"/>,
/// or <see cref="System.DateTimeOffset.MinValue"/>) to satisfy the "required" validation once
/// the strict-default emission flip lands. <b>This attribute has no effect in the current
/// release</b> — today, the listed Required bases already accept their CLR default. Declare it
/// now to lock in the lenient behavior across the upcoming flip without code churn.
/// </summary>
/// <remarks>
/// <para>
/// Useful for adapters at integration seams where an external system legitimately produces the
/// CLR default and the value must round-trip unchanged. For most domain types, leaving this
/// attribute off so the generator rejects the default (post-flip) will be the correct choice.
/// </para>
/// <para>
/// Not intended for use on <see cref="RequiredString{TSelf}"/> (use
/// <see cref="AllowEmptyAttribute"/> — the default for a CLR string is <c>null</c>, not the
/// empty string) or on numeric Required types (use <see cref="NotDefaultAttribute"/> to opt in
/// to numeric zero rejection).
/// </para>
/// </remarks>
/// <seealso cref="NotDefaultAttribute"/>
/// <seealso cref="AllowEmptyAttribute"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowDefaultAttribute : Attribute
{
}
