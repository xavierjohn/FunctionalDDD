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
        EmitCompanionHeaders(httpContext.Response, error, statusCode);

        // The only async companion-header path: synthesize WWW-Authenticate when the mediator
        // emitted Error.Unauthorized with no Challenges and the response doesn't already carry
        // the header. EmitCompanionHeaders has already written explicit-Challenges challenges
        // synchronously, so reaching here means the synthesis branch is the only one left.
        if (error is Error.Unauthorized unauth
            && statusCode == 401
            && unauth.Challenges.Items.Length == 0
            && !httpContext.Response.Headers.ContainsKey("WWW-Authenticate"))
        {
            await SynthesizeWwwAuthenticateAsync(httpContext).ConfigureAwait(false);
        }

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

    private static void EmitCompanionHeaders(HttpResponse response, Error error, int statusCode)
    {
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

            // Explicit challenges are emitted verbatim, synchronously. Synthesis (when
            // Challenges is empty) needs IAuthenticationSchemeProvider and runs in
            // SynthesizeWwwAuthenticateAsync from WriteAsync.
            //
            // Gated on the resolved status code: WWW-Authenticate is RFC 9110 §11.6.1
            // tied specifically to 401. If WithErrorMapping promotes Error.Unauthorized
            // to a non-401 status, suppress the header rather than mislead clients into
            // attempting re-authentication. Mirrors the m-13 status-aware design used
            // by ValidationProblem detail scrubbing.
            case Error.Unauthorized unauth when statusCode == 401 && unauth.Challenges.Items.Length > 0:
                foreach (var challenge in unauth.Challenges.Items)
                    response.Headers.Append("WWW-Authenticate", FormatChallenge(challenge));
                break;
        }
    }

    /// <summary>
    /// Synthesizes a scheme-only <c>WWW-Authenticate</c> challenge from the registered
    /// default-challenge scheme via <see cref="IAuthenticationSchemeProvider"/>. This is the
    /// RFC 9110 §11.6.1 compliance path for the mediator-emitted <c>Error.Unauthorized</c>
    /// with empty <see cref="Error.Unauthorized.Challenges"/> on anonymous-tolerant routes
    /// where the ASP.NET Core auth handler is never invoked. If no auth scheme is configured
    /// (<see cref="IAuthenticationSchemeProvider"/> unavailable or no default challenge),
    /// emits nothing — synthesizing "Bearer" for a service that does not use Bearer would
    /// mislead clients.
    /// </summary>
    /// <remarks>
    /// Caller (<see cref="WriteAsync"/>) is responsible for the precondition checks
    /// (<see cref="Error.Unauthorized.Challenges"/> empty, no existing
    /// <c>WWW-Authenticate</c> header, status code 401). This method only does the async
    /// scheme lookup and header append.
    /// </remarks>
    private static async Task SynthesizeWwwAuthenticateAsync(HttpContext httpContext)
    {
        var schemeProvider = httpContext.RequestServices?.GetService<IAuthenticationSchemeProvider>();
        if (schemeProvider is null)
            return;

        var scheme = await schemeProvider.GetDefaultChallengeSchemeAsync().ConfigureAwait(false)
            ?? await schemeProvider.GetDefaultAuthenticateSchemeAsync().ConfigureAwait(false);
        if (scheme is null)
            return;

        httpContext.Response.Headers.Append("WWW-Authenticate", scheme.Name);
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