namespace Trellis.Asp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Trellis;

/// <summary>
/// Internal helper that writes failure responses (ProblemDetails / ValidationProblem) and emits
/// RFC-required companion headers (<c>Allow</c>, <c>Retry-After</c>, <c>Content-Range</c>,
/// <c>WWW-Authenticate</c>) using the supplied per-call status code.
/// </summary>
internal static class ResponseFailureWriter
{
    public static async Task WriteAsync(HttpContext httpContext, Error error, int statusCode)
    {
        await EmitCompanionHeadersAsync(httpContext, error, statusCode).ConfigureAwait(false);

        // RFC 9457 §3.1: "instance" identifies the specific occurrence of the problem.
        // Emitting the server-relative path+query (rather than the absolute URL) avoids host
        // disclosure while still letting clients correlate the response with the request that
        // produced it. Matches what public APIs and the System.Net.Http.Json ProblemDetails
        // round-trip convention expect.
        var instance = httpContext.Request.GetEncodedPathAndQuery();

        Microsoft.AspNetCore.Http.IResult inner;
        if (error is Error.UnprocessableContent unprocessable
            && (unprocessable.Fields.Items.Length > 0 || unprocessable.Rules.Items.Length > 0))
        {
            var errors = unprocessable.Fields.Items
                .GroupBy(fv => JsonPointerToMvc.Translate(fv.Field.Path))
                .ToDictionary(g => g.Key, g => g.Select(fv => fv.Detail ?? fv.ReasonCode).ToArray());

            var validationDetail = statusCode >= 500 ? "An internal error occurred." : unprocessable.Detail;
            inner = Microsoft.AspNetCore.Http.Results.ValidationProblem(
                errors,
                validationDetail,
                instance,
                statusCode,
                extensions: BuildExtensions(error, unprocessable.Rules));
        }
        else
        {
            var detail = statusCode >= 500 ? "An internal error occurred." : error.Detail;
            var rules = error is Error.UnprocessableContent uc ? uc.Rules : default;
            inner = Microsoft.AspNetCore.Http.Results.Problem(
                detail,
                instance,
                statusCode,
                extensions: BuildExtensions(error, rules));
        }

        await inner.ExecuteAsync(httpContext).ConfigureAwait(false);
    }

    private static Task EmitCompanionHeadersAsync(HttpContext httpContext, Error error, int statusCode)
    {
        var response = httpContext.Response;
        switch (error)
        {
            case Error.MethodNotAllowed mae:
                response.Headers["Allow"] = string.Join(", ", mae.Allow.Items);
                break;

            case Error.TooManyRequests { RetryAfter: not null } tmr:
                response.Headers["Retry-After"] = tmr.RetryAfter.ToHeaderValue();
                break;

            case Error.ServiceUnavailable { RetryAfter: not null } sue:
                response.Headers["Retry-After"] = sue.RetryAfter.ToHeaderValue();
                break;

            case Error.RangeNotSatisfiable rnse:
                response.Headers["Content-Range"] = $"{rnse.Unit} */{rnse.CompleteLength}";
                break;

            // Gated on the resolved status code: WWW-Authenticate is RFC 9110 §11.6.1
            // tied specifically to 401. If WithErrorMapping promotes Error.Unauthorized
            // to a non-401 status, suppress the header rather than mislead clients into
            // attempting re-authentication. Mirrors the m-13 status-aware design used
            // by ValidationProblem detail scrubbing.
            case Error.Unauthorized unauth when statusCode == 401:
                return EmitWwwAuthenticateAsync(httpContext, unauth);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Emits <c>WWW-Authenticate</c> on a 401. Three branches:
    /// <list type="number">
    ///   <item><description>
    ///     Caller supplied explicit <see cref="Error.Unauthorized.Challenges"/>: emit each
    ///     verbatim (authoritative; never synthesized over).
    ///   </description></item>
    ///   <item><description>
    ///     Response already carries a <c>WWW-Authenticate</c> header (e.g. upstream auth
    ///     middleware wrote one): preserve it. Prevents double-emission when a custom auth
    ///     pipeline has already produced its challenge.
    ///   </description></item>
    ///   <item><description>
    ///     Otherwise (the mediator-emitted "no actor → 401" path): synthesize a scheme-only
    ///     challenge from the registered default-challenge scheme via
    ///     <see cref="IAuthenticationSchemeProvider"/>. This is the RFC 9110 §11.6.1
    ///     compliance fix for anonymous-tolerant routes where the auth handler is never
    ///     invoked, so the ASP.NET Core auth subsystem cannot write the challenge itself.
    ///     If no auth scheme is configured (<see cref="IAuthenticationSchemeProvider"/>
    ///     unavailable or no default challenge), emit nothing — synthesizing "Bearer" for a
    ///     service that does not use Bearer would mislead clients.
    ///   </description></item>
    /// </list>
    /// </summary>
    private static async Task EmitWwwAuthenticateAsync(HttpContext httpContext, Error.Unauthorized unauth)
    {
        var response = httpContext.Response;

        if (unauth.Challenges.Items.Length > 0)
        {
            foreach (var challenge in unauth.Challenges.Items)
                response.Headers.Append("WWW-Authenticate", FormatChallenge(challenge));
            return;
        }

        if (response.Headers.ContainsKey("WWW-Authenticate"))
            return;

        var schemeProvider = httpContext.RequestServices?.GetService<IAuthenticationSchemeProvider>();
        if (schemeProvider is null)
            return;

        // The IAuthenticationSchemeProvider contract is async; sync-over-async (.Result /
        // .GetAwaiter().GetResult()) would deadlock under some SynchronizationContexts and
        // stall the request thread if a custom implementation does real I/O. Await both
        // before the ProblemDetails body is composed.
        var scheme = await schemeProvider.GetDefaultChallengeSchemeAsync().ConfigureAwait(false)
            ?? await schemeProvider.GetDefaultAuthenticateSchemeAsync().ConfigureAwait(false);
        if (scheme is null)
            return;

        response.Headers.Append("WWW-Authenticate", scheme.Name);
    }

    /// <summary>
    /// Formats a single <see cref="AuthChallenge"/> as a <c>WWW-Authenticate</c> header value
    /// per RFC 9110 §11.6.1. Parameter values are always emitted as quoted-strings (RFC 9110
    /// §5.6.4); embedded <c>"</c> and <c>\</c> are backslash-escaped.
    /// </summary>
    private static string FormatChallenge(AuthChallenge challenge)
    {
        if (challenge.Params is null || challenge.Params.Count == 0)
            return challenge.Scheme;

        var sb = new StringBuilder(challenge.Scheme);
        sb.Append(' ');
        var first = true;
        foreach (var kv in challenge.Params)
        {
            if (!first) sb.Append(", ");
            first = false;
            sb.Append(kv.Key).Append('=').Append('"');
            AppendQuotedString(sb, kv.Value);
            sb.Append('"');
        }

        return sb.ToString();
    }

    private static void AppendQuotedString(StringBuilder sb, string value)
    {
        foreach (var ch in value)
        {
            if (ch is '"' or '\\') sb.Append('\\');
            sb.Append(ch);
        }
    }

    private static Dictionary<string, object?> BuildExtensions(Error error, EquatableArray<RuleViolation> rules)
    {
        var ext = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["code"] = error.Code,
            ["kind"] = error.Kind,
        };

        if (error is Error.InternalServerError ise)
            ext["faultId"] = ise.FaultId;

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
}

/// <summary>JSON shape used for rule violations in ProblemDetails extensions.</summary>
public sealed record RuleViolationProblemDetail(string Code, string? Detail, string[] Fields);