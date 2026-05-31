namespace Trellis.Asp.Idempotency;

using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Computes a deterministic SHA-256 fingerprint over the request components that matter for
/// safe idempotent replay: method, path, canonicalized query, body bytes, <c>Content-Type</c>,
/// <c>Content-Encoding</c>, and any headers listed in
/// <see cref="IdempotencyOptions.AdditionalFingerprintHeaders"/>.
/// </summary>
public static class IdempotencyFingerprint
{
    private const byte UnitSeparator = 0x1F;
    private const byte RecordSeparator = 0x1E;

    /// <summary>Computes the canonical fingerprint of a request.</summary>
    /// <param name="context">The HTTP context whose request is being fingerprinted.</param>
    /// <param name="body">The fully-buffered request body bytes (may be empty).</param>
    /// <param name="options">The idempotency options driving which headers participate.</param>
    /// <returns>A URL-safe base64 (no padding) SHA-256 digest.</returns>
    public static string Compute(HttpContext context, ReadOnlyMemory<byte> body, IdempotencyOptions options)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);

        using var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        AppendString(sha, context.Request.Method ?? string.Empty);
        sha.AppendData([RecordSeparator]);
        AppendString(sha, context.Request.Path.HasValue ? context.Request.Path.Value! : string.Empty);
        sha.AppendData([RecordSeparator]);
        AppendCanonicalQuery(sha, context.Request.Query);
        sha.AppendData([RecordSeparator]);
        AppendString(sha, context.Request.ContentType ?? string.Empty);
        sha.AppendData([RecordSeparator]);
        AppendString(sha, context.Request.Headers.TryGetValue("Content-Encoding", out var enc) ? enc.ToString() : string.Empty);
        sha.AppendData([RecordSeparator]);
        AppendAdditionalHeaders(sha, context.Request.Headers, options);
        sha.AppendData([RecordSeparator]);
        sha.AppendData(body.Span);

        var digest = sha.GetHashAndReset();
        return ToUrlSafeBase64NoPadding(digest);
    }

    private static void AppendString(IncrementalHash sha, string value) =>
        sha.AppendData(Encoding.UTF8.GetBytes(value));

    private static void AppendCanonicalQuery(IncrementalHash sha, IQueryCollection query)
    {
        if (query.Count == 0)
        {
            return;
        }

        var sortedKeys = query.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray();
        for (var ki = 0; ki < sortedKeys.Length; ki++)
        {
            var key = sortedKeys[ki];
            AppendString(sha, key);
            sha.AppendData([UnitSeparator]);
            var values = query[key];
            for (var vi = 0; vi < values.Count; vi++)
            {
                AppendString(sha, values[vi] ?? string.Empty);
                if (vi < values.Count - 1)
                {
                    sha.AppendData([UnitSeparator]);
                }
            }

            if (ki < sortedKeys.Length - 1)
            {
                sha.AppendData([UnitSeparator]);
            }
        }
    }

    private static void AppendAdditionalHeaders(IncrementalHash sha, IHeaderDictionary headers, IdempotencyOptions options)
    {
        if (options.AdditionalFingerprintHeaders.Count == 0)
        {
            return;
        }

        var sortedNames = options.AdditionalFingerprintHeaders
            .OrderBy(h => h, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        for (var hi = 0; hi < sortedNames.Length; hi++)
        {
            var name = sortedNames[hi];
            AppendString(sha, name);
            sha.AppendData([UnitSeparator]);
            if (headers.TryGetValue(name, out var values))
            {
                for (var vi = 0; vi < values.Count; vi++)
                {
                    AppendString(sha, (values[vi] ?? string.Empty).Trim());
                    if (vi < values.Count - 1)
                    {
                        sha.AppendData([UnitSeparator]);
                    }
                }
            }

            if (hi < sortedNames.Length - 1)
            {
                sha.AppendData([UnitSeparator]);
            }
        }
    }

    private static string ToUrlSafeBase64NoPadding(byte[] digest)
    {
        var b64 = Convert.ToBase64String(digest);
        return b64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
