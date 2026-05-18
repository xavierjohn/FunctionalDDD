namespace Trellis.Authorization;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Trellis;

/// <summary>
/// Strongly-typed identifier for an authenticated principal (the value behind
/// <see cref="Actor.Id"/>).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ActorId"/> wraps the raw claim string (typically the JWT <c>sub</c> or AAD
/// <c>oid</c>) so that authorization-layer APIs can accept and return a domain type instead
/// of an untyped <see cref="string"/>. Consumers that store the actor's identity at
/// aggregate boundaries (e.g. <c>Order.CreatedByActorId</c>) should reuse this type to
/// keep the principal-identity concept consistent across the framework, audit fields, and
/// any cross-aggregate comparison that asks "did the same actor do both of these things?".
/// </para>
/// <para>
/// Domain identifiers (a customer ID, a tenant member ID, a domain user aggregate's primary
/// key) are <i>not</i> the same concept as a principal ID. Those remain whatever VO the
/// domain models. Use <see cref="ActorId"/> for authentication-layer principal identity
/// only — what the auth pipeline produced — and resolve to domain identity at the
/// application service boundary if the two differ.
/// </para>
/// <para>
/// Construction trims surrounding whitespace and rejects null / empty / whitespace-only
/// values. This preserves the validation guard that previously lived on
/// <see cref="Actor"/>'s constructor (<c>ArgumentException.ThrowIfNullOrWhiteSpace</c>),
/// with the additional small normalization that <c>"  user-1  "</c> stores as
/// <c>"user-1"</c>.
/// </para>
/// </remarks>
[JsonConverter(typeof(ParsableJsonConverter<ActorId>))]
public sealed partial class ActorId
    : ScalarValueObject<ActorId, string>, IScalarValue<ActorId, string>, IParsable<ActorId>
{
    private ActorId(string value) : base(value) { }

    /// <summary>
    /// Attempts to create an <see cref="ActorId"/> from a raw string, trimming surrounding
    /// whitespace and rejecting null / empty / whitespace-only values.
    /// </summary>
    /// <param name="value">The raw principal id (e.g., a JWT <c>sub</c> claim value).</param>
    /// <param name="fieldName">Optional field name used in the validation error message.</param>
    public static Result<ActorId> TryCreate(string? value, string? fieldName = null)
    {
        var field = string.IsNullOrEmpty(fieldName) ? nameof(ActorId) : fieldName;
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Fail<ActorId>(
                Error.UnprocessableContent.ForField(field, "validation.error", $"{field} cannot be empty."));
        }

        return Result.Ok(new ActorId(value.Trim()));
    }

    /// <summary>
    /// Explicitly wraps a raw <see cref="string"/> value into a validated
    /// <see cref="ActorId"/>. Equivalent to calling <c>Create</c>.
    /// </summary>
    public static explicit operator ActorId(string value) => Create(value);

    /// <summary>
    /// Parses a raw <see cref="string"/> into an <see cref="ActorId"/>, or throws
    /// <see cref="FormatException"/> when validation fails.
    /// </summary>
    public static ActorId Parse(string s, IFormatProvider? provider)
    {
        var result = TryCreate(s);
        return result.Match(
            onSuccess: actorId => actorId,
            onFailure: error =>
            {
                var validation = (Error.UnprocessableContent)error;
                throw new FormatException(
                    validation.Fields.Items[0].Detail ?? validation.Fields.Items[0].ReasonCode);
            });
    }

    /// <summary>
    /// Attempts to parse a raw <see cref="string"/> into an <see cref="ActorId"/>.
    /// </summary>
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out ActorId result)
    {
        var r = TryCreate(s);
        if (r.TryGetValue(out var actorId))
        {
            result = actorId;
            return true;
        }

        result = default;
        return false;
    }
}
