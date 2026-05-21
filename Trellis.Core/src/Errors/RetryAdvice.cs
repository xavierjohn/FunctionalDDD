namespace Trellis;

/// <summary>
/// Transport-neutral retry hint attached to <see cref="Error.RateLimited"/> and
/// <see cref="Error.Unavailable"/>. Boundary layers translate to wire formats
/// (e.g. RFC 9110 § 10.2.3 <c>Retry-After</c> for HTTP).
/// </summary>
/// <param name="After">Relative duration to wait before retrying.</param>
/// <param name="At">Absolute instant when the caller may retry.</param>
public readonly record struct RetryAdvice(TimeSpan? After = null, DateTimeOffset? At = null);
