namespace Trellis;

/// <summary>
/// Transport-neutral retry hint attached to <see cref="Error.RateLimited"/> and
/// <see cref="Error.Unavailable"/>. Boundary layers translate to their own wire format
/// (e.g. a header on a synchronous response, an envelope field on a queued message,
/// or a structured field on a CLI exit envelope). Producers in the domain layer
/// describe <em>when</em> the caller may retry; how that is conveyed is the boundary's
/// concern.
/// </summary>
/// <param name="After">Relative duration to wait before retrying.</param>
/// <param name="At">Absolute instant when the caller may retry.</param>
public readonly record struct RetryAdvice(TimeSpan? After = null, DateTimeOffset? At = null);
