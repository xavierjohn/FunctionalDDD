﻿namespace Trellis.Asp;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Trellis;

/// <summary>
/// Internal helper that writes failure responses (ProblemDetails / ValidationProblem) and emits
/// RFC-required companion headers (<c>Allow</c>, <c>Content-Range</c>, <c>Retry-After</c>,
/// <c>WWW-Authenticate</c>) using the supplied per-call status code.
/// </summary>
internal static class ResponseFailureWriter
{
    public static async Task WriteAsync(HttpContext httpContext, Error error, int statusCode)
    {
        EmitCompanionHeaders(httpContext.Response, error);

        // Conflict + concurrent_modification + If-Match → 412 / precondition-failed.
        // Applied locally here (not in GetStatusCode) because the override depends on the
        // request's If-Match header, which TrellisAspOptions has no access to.
        var overriden = TryConcurrentModificationOverride(httpContext, error);
        var effectiveStatus = overriden?.Status ?? statusCode;
        var wireKindOverride = overriden?.WireKind;

        // The only async companion-header path: synthesize WWW-Authenticate when a 401 error
        // reaches the writer and the response doesn't already carry the header.
        if (error is Error.AuthenticationRequired authRequired
            && effectiveStatus == 401
            && !httpContext.Response.Headers.ContainsKey("WWW-Authenticate"))
        {
            await SynthesizeWwwAuthenticateAsync(httpContext, authRequired.Scheme).ConfigureAwait(false);
        }

        // RFC 9457 §3.1: "instance" identifies the specific occurrence of the problem.
        // Emitting the server-relative path+query (rather than the absolute URL) avoids host
        // disclosure while still letting clients correlate the response with the request that
        // produced it.
        var instance = httpContext.Request.GetEncodedPathAndQuery();

        Microsoft.AspNetCore.Http.IResult inner;

        if (error is Error.Aggregate aggregate)
        {
            var options = httpContext.RequestServices?.GetService<TrellisAspOptions>() ?? TrellisAspOptions.SystemDefault;
            var detail = effectiveStatus >= 500 ? "An internal error occurred." : GetPublicDetail(error);
            var extensions = BuildExtensions(error, default, wireKindOverride);
            extensions["errors"] = aggregate.Errors.Items
                .Select(child =>
                {
                    var (childCode, childKind) = GetCodeAndKind(child);
                    return (object?)new Dictionary<string, object?>(StringComparer.Ordinal)
                    {
                        ["type"] = childKind,
                        ["status"] = options.GetStatusCode(child),
                        ["code"] = childCode,
                        ["kind"] = childKind,
                        ["detail"] = GetPublicDetail(child),
                    };
                })
                .ToArray();

            inner = Microsoft.AspNetCore.Http.Results.Problem(
                detail,
                instance,
                effectiveStatus,
                extensions: extensions);
        }
        else if (error is Error.InvalidInput unprocessable
            && (unprocessable.Fields.Items.Length > 0 || unprocessable.Rules.Items.Length > 0))
        {
            var errors = unprocessable.Fields.Items
                .GroupBy(fv => JsonPointerToMvc.Translate(fv.Field.Path))
                .ToDictionary(g => g.Key, g => g.Select(fv => fv.Detail ?? fv.ReasonCode).ToArray());

            var validationDetail = effectiveStatus >= 500 ? "An internal error occurred." : unprocessable.Detail;
            inner = Microsoft.AspNetCore.Http.Results.ValidationProblem(
                errors,
                validationDetail,
                instance,
                effectiveStatus,
                extensions: BuildExtensions(error, unprocessable.Rules, wireKindOverride));
        }
        else
        {
            var detail = effectiveStatus >= 500 ? "An internal error occurred." : GetPublicDetail(error);
            var rules = error is Error.InvalidInput uc ? uc.Rules : default;
            inner = Microsoft.AspNetCore.Http.Results.Problem(
                detail,
                instance,
                effectiveStatus,
                extensions: BuildExtensions(error, rules, wireKindOverride));
        }

        await inner.ExecuteAsync(httpContext).ConfigureAwait(false);
    }

    private static void EmitCompanionHeaders(HttpResponse response, Error error)
    {
        switch (error)
        {
            case Error.TransportFault { Fault: HttpError.MethodNotAllowed mae }:
                response.Headers["Allow"] = string.Join(", ", mae.Allow.Items);
                break;

            case Error.TransportFault { Fault: HttpError.RangeNotSatisfiable rnse }:
                response.Headers["Content-Range"] = $"{rnse.Unit} */{rnse.CompleteLength}";
                break;

            case Error.RateLimited { Retry: { } rl }:
                EmitRetryAfter(response, rl);
                break;

            case Error.Unavailable { Retry: { } uv }:
                EmitRetryAfter(response, uv);
                break;
        }
    }

    private static void EmitRetryAfter(HttpResponse response, RetryAdvice retry)
    {
        // RFC 9110 §10.2.3: prefer delta-seconds when both are set — simpler for clients and
        // not subject to clock skew.
        if (retry.After is { } delta)
        {
            var seconds = Math.Max(0L, (long)Math.Ceiling(delta.TotalSeconds));
            response.Headers["Retry-After"] = seconds.ToString(CultureInfo.InvariantCulture);
        }
        else if (retry.At is { } at)
        {
            // RFC 7231 §7.1.1.1 IMF-fixdate form, e.g. "Sun, 06 Nov 1994 08:49:37 GMT".
            response.Headers["Retry-After"] = at.UtcDateTime.ToString("r", CultureInfo.InvariantCulture);
        }
    }

    private static (int Status, string WireKind)? TryConcurrentModificationOverride(HttpContext httpContext, Error error)
    {
        if (error is Error.Conflict { ReasonCode: "concurrent_modification" }
            && httpContext.Request.Headers.ContainsKey("If-Match"))
        {
            return (StatusCodes.Status412PreconditionFailed, "precondition-failed");
        }

        return null;
    }

    /// <summary>
    /// Synthesizes a <c>WWW-Authenticate</c> challenge. When the domain error carries an explicit
    /// scheme string, it is emitted verbatim (RFC 9110 §11.6.1 challenge syntax is the caller's
    /// responsibility). Otherwise the registered default-challenge scheme is resolved via
    /// <see cref="IAuthenticationSchemeProvider"/>. If no auth scheme is configured, emits
    /// nothing — synthesizing "Bearer" for a service that does not use Bearer would mislead clients.
    /// </summary>
    private static async Task SynthesizeWwwAuthenticateAsync(HttpContext httpContext, string? explicitScheme)
    {
        if (!string.IsNullOrWhiteSpace(explicitScheme))
        {
            httpContext.Response.Headers.Append("WWW-Authenticate", explicitScheme);
            return;
        }

        var schemeProvider = httpContext.RequestServices?.GetService<IAuthenticationSchemeProvider>();
        if (schemeProvider is null)
            return;

        var scheme = await schemeProvider.GetDefaultChallengeSchemeAsync().ConfigureAwait(false)
            ?? await schemeProvider.GetDefaultAuthenticateSchemeAsync().ConfigureAwait(false);
        if (scheme is null)
            return;

        httpContext.Response.Headers.Append("WWW-Authenticate", scheme.Name);
    }

    private static string? GetPublicDetail(Error error) =>
        error.Detail
        ?? (error is Error.TransportFault { Fault: HttpError httpError } ? httpError.Detail : null);

    private static (string Code, string Kind) GetCodeAndKind(Error error) =>
        error is Error.TransportFault { Fault: HttpError httpError }
            ? (httpError.Code, httpError.Kind)
            : (error.Code, ToWireKind(error));

    private static Dictionary<string, object?> BuildExtensions(Error error, EquatableArray<RuleViolation> rules, string? wireKindOverride = null)
    {
        var (code, kind) = GetCodeAndKind(error);
        if (wireKindOverride is not null)
            kind = wireKindOverride;

        var ext = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["code"] = code,
            ["kind"] = kind,
        };

        if (error is Error.Unexpected ise && ise.FaultId is not null)
            ext["faultId"] = ise.FaultId;

        if (error is Error.TransportFault { Fault: HttpError fault })
        {
            ProjectHttpErrorPayload(ext, fault);
        }

        if (rules.Items.Length > 0)
        {
            ext["rules"] = rules.Items
                .Select(rv => new RuleViolationProblemDetail(
                    rv.ReasonCode,
                    rv.Detail,
                    rv.Fields.Items.Select(p => p.Path).ToArray()))
                .ToArray();
        }

        return ext;
    }

    private static void ProjectHttpErrorPayload(Dictionary<string, object?> ext, HttpError fault)
    {
        switch (fault)
        {
            case HttpError.MethodNotAllowed { Allow: var allow }:
                ext["allow"] = allow.Items.ToArray();
                break;
            case HttpError.NotAcceptable { Available: var avail }:
                ext["available"] = avail.Items.ToArray();
                break;
            case HttpError.UnsupportedMediaType { Supported: var sup }:
                ext["supported"] = sup.Items.ToArray();
                break;
            case HttpError.RangeNotSatisfiable rnse:
                ext["completeLength"] = rnse.CompleteLength;
                ext["unit"] = rnse.Unit;
                break;
            case HttpError.ContentTooLarge { MaxBytes: { } max }:
                ext["maxBytes"] = max;
                break;
            case HttpError.PreconditionFailed pf:
                ext["preconditionKind"] = pf.Condition.ToString();
                break;
            case HttpError.PreconditionRequired pr:
                ext["preconditionKind"] = pr.Condition.ToString();
                break;
        }
    }

    private static string ToWireKind(Error error) => error switch
    {
        Error.InvalidInput => "unprocessable-content",
        Error.InvariantViolation => "unprocessable-content",
        Error.AuthenticationRequired => "unauthorized",
        Error.RateLimited => "too-many-requests",
        Error.Unavailable => "service-unavailable",
        Error.Unexpected u when u.ReasonCode == "not_implemented" => "not-implemented",
        Error.Unexpected => "internal-server-error",
        Error.Aggregate => "multi",
        _ => error.Kind,
    };
}

/// <summary>JSON shape used for rule violations in ProblemDetails extensions.</summary>
public sealed record RuleViolationProblemDetail(string Code, string? Detail, string[] Fields);