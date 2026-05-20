namespace Trellis.Asp.ApiVersioning;

using System;
using global::Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trellis.Asp;

/// <summary>
/// API-versioning extensions on <see cref="HttpResponseOptionsBuilder{TDomain}"/> that auto-inject
/// the <c>api-version</c> route value into <c>Location</c> headers so responses round-trip the
/// requested version under query/header API versioning. Chain after any builder method that
/// generates a <c>Location</c> header — <c>CreatedAtRoute(...)</c>, <c>CreatedAtAction(...)</c>,
/// <c>WithLocation(...)</c>, etc.
/// </summary>
/// <remarks>
/// <para>
/// Resolution order (per request, inside the <c>LinkGenerator</c> callback):
/// </para>
/// <list type="number">
///   <item><description><c>HttpContext.RequestedApiVersion</c> — primary signal; reflects whatever the configured <c>IApiVersionReader</c> parsed (query, header, media-type, URL segment, composite).</description></item>
///   <item><description>Endpoint metadata <c>ApiVersionMetadata.Map(ApiVersionMapping.Implicit).DeclaredApiVersions</c> — fallback when (1) is null and exactly one declared version exists.</description></item>
///   <item><description><c>ApiVersioningOptions.DefaultApiVersion</c> — final fallback, configured via <c>services.AddApiVersioning(o =&gt; o.DefaultApiVersion = ...)</c>.</description></item>
/// </list>
/// <para>
/// The route-value key is fixed at <c>"api-version"</c>, matching the default for
/// <c>QueryStringApiVersionReader</c> and the conventional header name. Hosts using a
/// non-default reader parameter name should register a custom resolver via
/// <see cref="HttpResponseOptionsBuilder{TDomain}.WithRouteValueResolver"/> directly.
/// </para>
/// <para>
/// Cases that skip injection (resolver returns <c>null</c>, applies to both the per-request
/// and explicit-version overloads): version-neutral endpoints
/// (<c>ApiVersionMetadata.IsApiVersionNeutral</c> or <c>[ApiVersionNeutral]</c>) and
/// URL-segment-versioned routes (route template contains <c>:apiVersion</c>; ambient
/// routing handles substitution). Multi-version actions with no client-requested version
/// AND no <c>DefaultApiVersion</c> throw <see cref="InvalidOperationException"/> rather
/// than silently picking — silent picking would resurrect the original 404 bug.
/// </para>
/// </remarks>
public static class HttpResponseOptionsBuilderApiVersioningExtensions
{
    /// <summary>
    /// Injects the configured <c>api-version</c> route value into the <c>Location</c> header
    /// emitted by a preceding <see cref="HttpResponseOptionsBuilder{TDomain}.CreatedAtRoute(string, Func{TDomain, RouteValueDictionary})"/>,
    /// <see cref="HttpResponseOptionsBuilder{TDomain}.CreatedAtAction(string, Func{TDomain, RouteValueDictionary}, string?)"/>
    /// or <see cref="HttpResponseOptionsBuilder{TDomain}.WithLocation(string, Func{TDomain, RouteValueDictionary})"/>
    /// call. The version is resolved per-request from <see cref="HttpContext"/>.
    /// </summary>
    /// <typeparam name="TDomain">The domain value type from <c>Result&lt;TDomain&gt;</c>.</typeparam>
    /// <param name="builder">The builder to configure.</param>
    /// <remarks>
    /// See the type-level remarks on <see cref="HttpResponseOptionsBuilderApiVersioningExtensions"/>
    /// for the version-resolution order and skip rules. The route-value key is fixed at
    /// <c>"api-version"</c> — matches Asp.Versioning's defaults.
    /// </remarks>
    public static HttpResponseOptionsBuilder<TDomain> WithVersionedRoute<TDomain>(
        this HttpResponseOptionsBuilder<TDomain> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Resolution happens per-request from HttpContext (inside the resolver delegate);
        // only the route-value key is fixed at registration time. The key is the constant
        // `DefaultRouteValueKey` ("api-version") — no IOptions lookup is involved for the key.
        return builder.WithRouteValueResolver(
            DefaultRouteValueKey,
            ResolveApiVersion);
    }

    /// <summary>
    /// Escape hatch: pin the <c>Location</c> header to a specific <see cref="ApiVersion"/>
    /// regardless of what the client requested. Use for cross-version <c>Location</c> redirects
    /// on deprecated endpoints.
    /// </summary>
    /// <typeparam name="TDomain">The domain value type from <c>Result&lt;TDomain&gt;</c>.</typeparam>
    /// <param name="builder">The builder to configure.</param>
    /// <param name="explicitVersion">The version to inject, regardless of client request or endpoint metadata.</param>
    /// <remarks>
    /// Explicit pinning overrides the per-request resolution order (requested / declared /
    /// default), but the skip rules still apply: on a version-neutral endpoint
    /// (<c>ApiVersionMetadata.IsApiVersionNeutral</c> or <c>[ApiVersionNeutral]</c>) and on
    /// URL-segment-versioned routes (route template contains <c>:apiVersion</c>) the resolver
    /// returns <c>null</c> and no <c>api-version</c> route value is injected. Injecting a
    /// pinned version into a Location that targets a neutral endpoint would mislead clients;
    /// injecting it as a query parameter alongside a path segment would create a redundant /
    /// conflicting value. Both bugs are silent without this guard.
    /// </remarks>
    public static HttpResponseOptionsBuilder<TDomain> WithVersionedRoute<TDomain>(
        this HttpResponseOptionsBuilder<TDomain> builder,
        ApiVersion explicitVersion)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(explicitVersion);

        var pinnedValue = explicitVersion.ToString();
        return builder.WithRouteValueResolver(
            DefaultRouteValueKey,
            httpContext => ShouldSkipInjection(httpContext) ? null : pinnedValue);
    }

    /// <summary>
    /// Default route-value key used to inject the resolved API version into the
    /// <see cref="RouteValueDictionary"/>. Matches <c>QueryStringApiVersionReader.DefaultParameterName</c>
    /// and the conventional <c>api-version</c> header name. Hosts using a non-default reader
    /// parameter name should bypass this helper and register a resolver via
    /// <see cref="HttpResponseOptionsBuilder{TDomain}.WithRouteValueResolver"/> directly.
    /// </summary>
    private const string DefaultRouteValueKey = "api-version";

    private static string? ResolveApiVersion(HttpContext httpContext)
    {
        // Skip injection on version-neutral endpoints (the response Location shouldn't carry
        // a parameter the target endpoint doesn't accept) and on URL-segment versioning (the
        // ambient route values already carry the version token; adding a query copy creates
        // a redundant/conflicting parameter).
        if (ShouldSkipInjection(httpContext))
            return null;

        var endpoint = httpContext.GetEndpoint();
        var metadata = endpoint?.Metadata.GetMetadata<ApiVersionMetadata>();

        // 1. Echo the version the client requested — primary signal.
        var requested = httpContext.RequestedApiVersion;
        if (requested is not null)
            return requested.ToString();

        // 2. Single declared version on the endpoint metadata — unambiguous fallback.
        if (metadata is not null)
        {
            var implicitVersions = metadata.Map(ApiVersionMapping.Implicit).DeclaredApiVersions;
            if (implicitVersions.Count == 1)
                return implicitVersions[0].ToString();
        }

        // 3. ApiVersioningOptions.DefaultApiVersion — host-level fallback.
        var apiVersioningOptions = httpContext.RequestServices.GetService<IOptions<ApiVersioningOptions>>();
        if (apiVersioningOptions?.Value.DefaultApiVersion is { } defaultVersion)
            return defaultVersion.ToString();

        // 4. No way to resolve — fail loudly. Silent picking would resurrect the original 404 bug.
        throw new InvalidOperationException(
            "Cannot determine the api-version for the Location header: the request did not specify a " +
            "version, the endpoint declares more than one (or none), and no DefaultApiVersion is configured. " +
            "Either configure ApiVersioningOptions.DefaultApiVersion or use the explicit-version " +
            "WithVersionedRoute(explicitVersion) overload.");
    }

    /// <summary>
    /// Returns <c>true</c> when the configured api-version resolver must NOT inject a route
    /// value, regardless of which resolution path produced the candidate. Centralised so the
    /// per-request resolver and the explicit-pinning overload share identical skip semantics.
    /// </summary>
    private static bool ShouldSkipInjection(HttpContext httpContext)
    {
        var endpoint = httpContext.GetEndpoint();

        // Version-neutral endpoints reject any api-version parameter; emitting one would
        // mislead clients into resending the same Location with an unsupported value.
        var metadata = endpoint?.Metadata.GetMetadata<ApiVersionMetadata>();
        if (metadata is { IsApiVersionNeutral: true })
            return true;

        // URL-segment versioning embeds the version in the path; ambient routing fills the
        // segment from RouteData, and a query-string copy would create a redundant /
        // conflicting parameter on the Location URI.
        if (RouteTemplateContainsVersionToken(endpoint))
            return true;

        return false;
    }

    private static bool RouteTemplateContainsVersionToken(Microsoft.AspNetCore.Http.Endpoint? endpoint)
    {
        if (endpoint is not Microsoft.AspNetCore.Routing.RouteEndpoint routeEndpoint)
            return false;

        var template = routeEndpoint.RoutePattern.RawText;
        if (string.IsNullOrEmpty(template))
            return false;

        // Recognises the standard URL-segment versioning shape: api/v{version:apiVersion}/...
        // The `:apiVersion` route constraint is the canonical signal — query/header readers don't
        // produce a route token. Match either the parameter name "version" with the apiVersion
        // constraint, or the constraint anywhere in the template.
        return template.Contains(":apiVersion", StringComparison.OrdinalIgnoreCase);
    }
}
