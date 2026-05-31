namespace Trellis.Asp.Idempotency;

using System;
using System.Collections.Generic;

/// <summary>
/// Captured representation of a previously emitted response, replayed verbatim when a retry
/// matches the original request fingerprint.
/// </summary>
/// <param name="StatusCode">The HTTP status code of the original response.</param>
/// <param name="Headers">
/// Response headers as written by the handler, after <see cref="IdempotencyOptions"/> filtering
/// (for example <c>Set-Cookie</c> stripped when <see cref="IdempotencyOptions.IncludeSetCookieInSnapshot"/>
/// is <c>false</c>). Header names are matched case-insensitively.
/// </param>
/// <param name="Body">The raw response body bytes captured from the handler.</param>
/// <param name="Fingerprint">
/// The request fingerprint that produced this snapshot. Stored alongside the response so a
/// replay can reject a key reused with a different body.
/// </param>
public sealed record IdempotencyResponseSnapshot(
    int StatusCode,
    IReadOnlyDictionary<string, string[]> Headers,
    byte[] Body,
    string Fingerprint);
