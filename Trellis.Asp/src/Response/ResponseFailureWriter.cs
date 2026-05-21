namespace Trellis.Asp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Trellis;

/// <summary>
/// Internal helper that writes failure responses (ProblemDetails / ValidationProblem) and emits
/// RFC-required companion headers (<c>Allow</c>, <c>Content-Range</c>,
/// <c>WWW-Authenticate</c>) using the supplied per-call status code.
/// </summary>
internal static class ResponseFailureWriter
{
    public static async Task WriteAsync(HttpContext httpContext, Error error, int statusCode)
    {
        EmitCompanionHeaders(httpContext.Response, error);

        // The only async companion-header path: synthesize WWW-Authenticate when a 401 error
        // reaches the writer and the response doesn't already carry the header.
        if (error is Error.AuthenticationRequired
            && statusCode == 401
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
        if (error is Error.InvalidInput unprocessable
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
            var detail = statusCode >= 500 ? "An internal error occurred." : GetPublicDetail(error);
            var rules = error is Error.InvalidInput uc ? uc.Rules : default;
            inner = Microsoft.AspNetCore.Http.Results.Problem(
                detail,
                instance,
                statusCode,
                extensions: BuildExtensions(error, rules));
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
        }
    }

    /// <summary>
    /// Synthesizes a scheme-only <c>WWW-Authenticate</c> challenge from the registered
    /// default-challenge scheme via <see cref="IAuthenticationSchemeProvider"/>. This is the
    /// RFC 9110 §11.6.1 compliance path for mediator-emitted <see cref="Error.AuthenticationRequired"/>
    /// results on anonymous-tolerant routes where the ASP.NET Core auth handler is never
    /// invoked. If no auth scheme is configured (<see cref="IAuthenticationSchemeProvider"/>
    /// unavailable or no default challenge), emits nothing — synthesizing "Bearer" for a
    /// service that does not use Bearer would mislead clients.
    /// </summary>
    /// <remarks>
    /// Caller (<see cref="WriteAsync"/>) is responsible for the precondition checks
    /// (no existing <c>WWW-Authenticate</c> header, status code 401). This method only
    /// does the async scheme lookup and header append.
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

    private static string? GetPublicDetail(Error error) =>
        error.Detail
        ?? (error is Error.TransportFault { Fault: HttpError httpError } ? httpError.Detail : null);

    private static Dictionary<string, object?> BuildExtensions(Error error, EquatableArray<RuleViolation> rules)
    {
        var (code, kind) = error is Error.TransportFault { Fault: HttpError httpError }
            ? (httpError.Code, httpError.Kind)
            : (error.Code, ToWireKind(error));

        var ext = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["code"] = code,
            ["kind"] = kind,
        };

        if (error is Error.Unexpected ise && ise.FaultId is not null)
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

    private static string ToWireKind(Error error) => error switch
    {
        Error.InvalidInput => "unprocessable-content",
        Error.InvariantViolation => "unprocessable-content",
        Error.AuthenticationRequired => "unauthorized",
        Error.RateLimited => "too-many-requests",
        Error.Unavailable => "service-unavailable",
        Error.Unexpected u when u.ReasonCode == "not_implemented" => "not-implemented",
        Error.Unexpected => "internal-server-error",
        _ => error.Kind,
    };
}

/// <summary>JSON shape used for rule violations in ProblemDetails extensions.</summary>
public sealed record RuleViolationProblemDetail(string Code, string? Detail, string[] Fields);