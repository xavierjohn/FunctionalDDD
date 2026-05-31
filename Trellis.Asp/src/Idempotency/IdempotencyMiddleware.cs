namespace Trellis.Asp.Idempotency;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Middleware that implements the IETF Idempotency-Key header semantics. When an endpoint is
/// marked with <c>IdempotentAttribute</c> and the configured method matches, the middleware
/// buffers the request body, computes a deterministic fingerprint, and consults an
/// <c>IIdempotencyStore</c> to either reserve, replay, conflict, or reject the request.
/// </summary>
/// <remarks>
/// The middleware is fail-open by design for endpoints that did not opt in: if the
/// <c>IdempotentAttribute</c> metadata is absent the request flows through untouched. The
/// store is consulted exclusively for opted-in endpoints carrying a parseable Idempotency-Key
/// header.
/// </remarks>
public sealed partial class IdempotencyMiddleware
{
    private static readonly TimeSpan FinalizationTimeout = TimeSpan.FromSeconds(5);

    private readonly RequestDelegate next;
    private readonly IdempotencyOptions options;
    private readonly IIdempotencyStore store;
    private readonly IIdempotencyScopeResolver scopeResolver;
    private readonly ILogger<IdempotencyMiddleware> logger;

    /// <summary>Initializes a new instance of the <see cref="IdempotencyMiddleware"/> class.</summary>
    public IdempotencyMiddleware(
        RequestDelegate next,
        IOptions<IdempotencyOptions> options,
        IIdempotencyStore store,
        IIdempotencyScopeResolver scopeResolver,
        ILogger<IdempotencyMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(scopeResolver);
        ArgumentNullException.ThrowIfNull(logger);

        this.next = next;
        this.options = options.Value;
        this.store = store;
        this.scopeResolver = scopeResolver;
        this.logger = logger;
    }

    /// <summary>Processes a single HTTP request.</summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var endpoint = context.GetEndpoint();
        var idempotentMeta = endpoint?.Metadata.GetMetadata<IdempotentAttribute>();
        if (idempotentMeta is null)
        {
            await this.next(context).ConfigureAwait(false);
            return;
        }

        if (!this.options.Methods.Contains(context.Request.Method))
        {
            await this.next(context).ConfigureAwait(false);
            return;
        }

        if (!context.Request.Headers.TryGetValue(this.options.HeaderName, out var rawValues) || rawValues.Count == 0)
        {
            if (this.options.RequireKeyOnOptedInEndpoints)
            {
                await WriteProblemAsync(context, 400, "idempotency.key_required",
                    $"This endpoint requires the {this.options.HeaderName} header.").ConfigureAwait(false);
                return;
            }

            await this.next(context).ConfigureAwait(false);
            return;
        }

        if (rawValues.Count > 1)
        {
            await WriteProblemAsync(context, 400, "idempotency.key_duplicate",
                "Multiple Idempotency-Key headers were received.").ConfigureAwait(false);
            return;
        }

        if (!IdempotencyKeyParser.TryParse(rawValues[0], out var key, out var parseError))
        {
            await WriteProblemAsync(context, 400, "idempotency.key_invalid", parseError ?? "Invalid Idempotency-Key header.").ConfigureAwait(false);
            return;
        }

        if (key.Length > this.options.MaxKeyLength)
        {
            await WriteProblemAsync(context, 400, "idempotency.key_too_long",
                $"Idempotency-Key length {key.Length} exceeds maximum {this.options.MaxKeyLength}.").ConfigureAwait(false);
            return;
        }

        var bodyBuffer = await BufferRequestBodyAsync(context, this.options.MaxRequestBodyBytes).ConfigureAwait(false);
        if (bodyBuffer is null)
        {
            await WriteProblemAsync(context, 413, "idempotency.request_body_too_large",
                $"Request body exceeds idempotency size limit of {this.options.MaxRequestBodyBytes} bytes.").ConfigureAwait(false);
            return;
        }

        context.Request.Body = new MemoryStream(bodyBuffer);

        var fingerprint = IdempotencyFingerprint.Compute(context, bodyBuffer, this.options);
        var scope = await this.scopeResolver.ResolveAsync(context, context.RequestAborted).ConfigureAwait(false);

        var outcome = await this.store.TryReserveAsync(scope, key, fingerprint, context.RequestAborted).ConfigureAwait(false);

        switch (outcome)
        {
            case IdempotencyReservationOutcome.Reserved reserved:
                await this.ExecuteAndCaptureAsync(context, scope, key, reserved.ReservationId, fingerprint).ConfigureAwait(false);
                return;

            case IdempotencyReservationOutcome.AlreadyInFlight inFlight:
                await WriteInFlightAsync(context, inFlight.RetryAfter).ConfigureAwait(false);
                return;

            case IdempotencyReservationOutcome.Replay replay:
                await this.WriteReplayAsync(context, replay.Snapshot).ConfigureAwait(false);
                return;

            case IdempotencyReservationOutcome.BodyHashMismatch:
                await WriteProblemAsync(
                    context,
                    this.options.MismatchStatusCode,
                    "idempotency.key_reused_with_different_body",
                    "An Idempotency-Key was reused with a different request body or headers.").ConfigureAwait(false);
                return;

            default:
                throw new InvalidOperationException($"Unhandled idempotency outcome {outcome.GetType().Name}.");
        }
    }

    private static async Task<byte[]?> BufferRequestBodyAsync(HttpContext context, long maxBytes)
    {
        context.Request.EnableBuffering();
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        long total = 0;
        while (true)
        {
            var read = await context.Request.Body.ReadAsync(chunk.AsMemory(0, chunk.Length), context.RequestAborted).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            total += read;
            if (total > maxBytes)
            {
                return null;
            }

            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string code, string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var json = SerializeProblemDocument(statusCode, ReasonPhrase(statusCode), detail, context.Request.GetEncodedPathAndQuery(), code);
        await context.Response.Body.WriteAsync(json, context.RequestAborted).ConfigureAwait(false);
    }

    private static async Task WriteInFlightAsync(HttpContext context, TimeSpan retryAfter)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = 409;
        context.Response.ContentType = "application/problem+json";
        var seconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
        context.Response.Headers["Retry-After"] = seconds.ToString(CultureInfo.InvariantCulture);

        var json = SerializeProblemDocument(409, "Conflict",
            "A request with this Idempotency-Key is already in flight.",
            context.Request.GetEncodedPathAndQuery(),
            "idempotency.in_flight");
        await context.Response.Body.WriteAsync(json, context.RequestAborted).ConfigureAwait(false);
    }

    private void WriteReplayPreamble(HttpContext context, IdempotencyResponseSnapshot snapshot)
    {
        context.Response.Clear();
        context.Response.StatusCode = snapshot.StatusCode;
        foreach (var header in snapshot.Headers)
        {
            context.Response.Headers[header.Key] = header.Value;
        }

        context.Response.Headers[this.options.ReplayHeaderName] = "true";
    }

    private async Task WriteReplayAsync(HttpContext context, IdempotencyResponseSnapshot snapshot)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        this.WriteReplayPreamble(context, snapshot);

        if (snapshot.Body.Length > 0)
        {
            await context.Response.Body.WriteAsync(snapshot.Body, context.RequestAborted).ConfigureAwait(false);
        }
    }

    private static byte[] SerializeProblemDocument(int status, string title, string detail, string instance, string code)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("type", "about:blank");
            writer.WriteString("title", title);
            writer.WriteNumber("status", status);
            writer.WriteString("detail", detail);
            writer.WriteString("instance", instance);
            writer.WriteString("code", code);
            writer.WriteEndObject();
        }

        return stream.ToArray();
    }

    private static readonly System.Collections.Generic.HashSet<string> ResponseHeadersExcludedFromSnapshot = new(StringComparer.OrdinalIgnoreCase)
    {
        "Date",
        "Server",
        "Transfer-Encoding",
        "Connection",
        "Keep-Alive",
        "Upgrade",
        "Proxy-Authenticate",
        "Proxy-Authorization",
        "TE",
        "Trailer",
    };

    private async Task ExecuteAndCaptureAsync(HttpContext context, string scope, string key, string reservationId, string fingerprint)
    {
        var originalBodyFeature = context.Features.Get<IHttpResponseBodyFeature>();
        if (originalBodyFeature is null)
        {
            LogCaptureSkippedNoFeature(this.logger, context.Request.Path);
            try
            {
                await this.next(context).ConfigureAwait(false);
            }
            catch
            {
                await this.SafeAbandonAsync(scope, key, reservationId).ConfigureAwait(false);
                throw;
            }

            await this.SafeAbandonAsync(scope, key, reservationId).ConfigureAwait(false);
            return;
        }

        using var capture = new CapturingResponseBodyFeature(originalBodyFeature, this.options.MaxResponseBodyBytes);
        context.Features.Set<IHttpResponseBodyFeature>(capture);

        var headerSnapshot = new HeaderSnapshot();
        var includeSetCookie = this.options.IncludeSetCookieInSnapshot;
        context.Response.OnStarting(state =>
        {
            var snap = (HeaderSnapshot)state;
            snap.StatusCode = context.Response.StatusCode;
            foreach (var h in context.Response.Headers)
            {
                if (ResponseHeadersExcludedFromSnapshot.Contains(h.Key))
                    continue;
                if (!includeSetCookie && string.Equals(h.Key, "Set-Cookie", StringComparison.OrdinalIgnoreCase))
                    continue;
                snap.Headers[h.Key] = h.Value.Select(v => v ?? string.Empty).ToArray();
            }

            snap.Captured = true;
            return Task.CompletedTask;
        }, headerSnapshot);

        try
        {
            await this.next(context).ConfigureAwait(false);
        }
        catch
        {
            context.Features.Set<IHttpResponseBodyFeature>(originalBodyFeature);
            await this.SafeAbandonAsync(scope, key, reservationId).ConfigureAwait(false);
            throw;
        }

        context.Features.Set<IHttpResponseBodyFeature>(originalBodyFeature);

        var bytes = capture.GetCapturedBytes();
        if (bytes is null)
        {
            // Capture aborted (response too large, SendFileAsync, or explicit abort) — abandon
            // the reservation so the next retry can re-execute.
            LogCaptureAbandoned(this.logger, key);
            await this.SafeAbandonAsync(scope, key, reservationId).ConfigureAwait(false);
            return;
        }

        if (!headerSnapshot.Captured)
        {
            // Handler completed without flushing the response (typical for 204 No Content or
            // any endpoint that writes only headers/status before returning). Snapshot the
            // current Response state directly so the next retry replays the same outcome.
            headerSnapshot.StatusCode = context.Response.StatusCode;
            foreach (var h in context.Response.Headers)
            {
                if (ResponseHeadersExcludedFromSnapshot.Contains(h.Key))
                    continue;
                if (!includeSetCookie && string.Equals(h.Key, "Set-Cookie", StringComparison.OrdinalIgnoreCase))
                    continue;
                headerSnapshot.Headers[h.Key] = h.Value.Select(v => v ?? string.Empty).ToArray();
            }
        }

        var trailersFeature = context.Features.Get<IHttpResponseTrailersFeature>();
        if (trailersFeature?.Trailers is { Count: > 0 })
        {
            // Response trailers cannot be replayed by the snapshot writer (which only restores
            // status + headers + body), so any response that wrote trailers is treated as
            // non-cacheable and the reservation is released for retry.
            LogTrailersAbandoned(this.logger, key);
            await this.SafeAbandonAsync(scope, key, reservationId).ConfigureAwait(false);
            return;
        }

        if (headerSnapshot.StatusCode is >= 500 and <= 599)
        {
            // 5xx responses are treated as transient per the IIdempotencyStore.AbandonAsync
            // contract: caching them would deny the client a real retry that might succeed.
            LogServerErrorAbandoned(this.logger, key, headerSnapshot.StatusCode);
            await this.SafeAbandonAsync(scope, key, reservationId).ConfigureAwait(false);
            return;
        }

        var snapshot = new IdempotencyResponseSnapshot(
            StatusCode: headerSnapshot.StatusCode,
            Headers: headerSnapshot.Headers,
            Body: bytes,
            Fingerprint: fingerprint);

        using var cts = new CancellationTokenSource(FinalizationTimeout);
        try
        {
            await this.store.CompleteAsync(scope, key, reservationId, snapshot, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            LogCompleteTimedOut(this.logger, key);
        }
        catch (Exception ex)
        {
            LogCompleteFailed(this.logger, ex, key);
        }
    }

    private async Task SafeAbandonAsync(string scope, string key, string reservationId)
    {
        using var cts = new CancellationTokenSource(FinalizationTimeout);
        try
        {
            await this.store.AbandonAsync(scope, key, reservationId, cts.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogAbandonFailed(this.logger, ex, key);
        }
    }

    private static string ReasonPhrase(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        409 => "Conflict",
        413 => "Payload Too Large",
        422 => "Unprocessable Entity",
        _ => "Error",
    };

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Idempotency capture skipped: no IHttpResponseBodyFeature on request {Path}")]
    static partial void LogCaptureSkippedNoFeature(ILogger logger, string path);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Idempotency snapshot not persisted (capture aborted or response never started) for key {Key}")]
    static partial void LogCaptureAbandoned(ILogger logger, string key);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Idempotency store CompleteAsync timed out for key {Key}")]
    static partial void LogCompleteTimedOut(ILogger logger, string key);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Idempotency store CompleteAsync failed for key {Key}")]
    static partial void LogCompleteFailed(ILogger logger, Exception ex, string key);

    [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "Idempotency store AbandonAsync failed for key {Key}")]
    static partial void LogAbandonFailed(ILogger logger, Exception ex, string key);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Idempotency reservation abandoned for key {Key}: response wrote trailers, which cannot be replayed")]
    static partial void LogTrailersAbandoned(ILogger logger, string key);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Idempotency reservation abandoned for key {Key}: handler returned status {StatusCode} (5xx responses are treated as transient)")]
    static partial void LogServerErrorAbandoned(ILogger logger, string key, int statusCode);

    private sealed class HeaderSnapshot
    {
        public int StatusCode { get; set; } = 200;

        public Dictionary<string, string[]> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

        public bool Captured { get; set; }
    }
}
