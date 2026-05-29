namespace Trellis.Asp.ApiVersioning;

using System;
using System.Linq;
using global::Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// API-versioning extensions on <see cref="HttpContext"/> that build paginated-list URLs in the
/// shape expected by <c>Trellis.Asp</c>'s paged <c>ToHttpResponse</c> overloads
/// (the <c>nextUrlBuilder</c> parameter, a <see cref="Func{T1, T2, TResult}"/> taking
/// (<see cref="Cursor"/>, <see cref="int"/>) and returning a URL string).
/// </summary>
/// <remarks>
/// <para>
/// Mirrors the resolution and skip rules of
/// <see cref="HttpResponseOptionsBuilderApiVersioningExtensions.WithVersionedRoute{TDomain}(HttpResponseOptionsBuilder{TDomain})"/>
/// for paginated <c>Link</c> / <c>next</c> URLs: query/header versioning round-trips
/// <c>api-version</c> automatically; URL-segment versioning fills the segment from ambient
/// route data and skips query injection; version-neutral endpoints never receive an
/// <c>api-version</c> parameter.
/// </para>
/// <para>
/// Builds absolute URLs via <c>LinkGenerator.GetUriByRouteValues(HttpContext, ...)</c> so the
/// emitted next-page URL preserves <see cref="HttpRequest.Scheme"/>, <see cref="HttpRequest.Host"/>,
/// and <see cref="HttpRequest.PathBase"/> from the current request.
/// </para>
/// <para>
/// The returned <see cref="Func{T1, T2, TResult}"/> captures the current <see cref="HttpContext"/>
/// and is therefore request-scoped: invoke it during the same request that produced it, before
/// the response completes. Don't hand it to a background task.
/// </para>
/// </remarks>
public static class HttpContextPageUrlExtensions
{
    /// <summary>
    /// Returns a <c>(Cursor, int) -&gt; string</c> builder suitable for the <c>nextUrlBuilder</c>
    /// parameter of <c>Trellis.Asp</c>'s paged <c>ToHttpResponse</c> overloads. Resolves the
    /// <c>api-version</c> route value from the request (per-request mode).
    /// </summary>
    /// <param name="httpContext">The current request context.</param>
    /// <param name="routeName">
    /// The name of the target route (typically the current paginated endpoint's route name).
    /// </param>
    /// <param name="routeValues">
    /// A callback that maps <c>(cursor, appliedLimit)</c> to the route-value dictionary for the
    /// next-page URL. The helper clones this dictionary before injecting <c>api-version</c>, so
    /// you may return a shared instance from the callback without it being mutated.
    /// </param>
    /// <returns>A request-scoped URL builder. Must be invoked during the same request.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="httpContext"/> or <paramref name="routeValues"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="routeName"/> is <c>null</c>, empty, or whitespace.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown by the returned builder when (a) the target route name resolves to no registered
    /// endpoint, (b) a multi-version endpoint has no client-requested version AND no
    /// <c>DefaultApiVersion</c> is configured, or (c) <c>LinkGenerator</c> returns <c>null</c>
    /// (route + ambient values produced no match).
    /// </exception>
    /// <remarks>
    /// Cross-route caveat: when <paramref name="routeName"/> differs from the current endpoint's
    /// route name, the skip rules and declared-version fallback are evaluated against the
    /// TARGET endpoint, not the current one. The per-request <c>RequestedApiVersion</c>
    /// (when the client supplied one) is still echoed first.
    /// </remarks>
    public static Func<Cursor, int, string> PageUrl(
        this HttpContext httpContext,
        string routeName,
        Func<Cursor, int, RouteValueDictionary> routeValues)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ThrowIfRouteNameInvalid(routeName);
        ArgumentNullException.ThrowIfNull(routeValues);

        return (cursor, appliedLimit) =>
        {
            var targetEndpoint = FindEndpointByRouteName(httpContext, routeName)
                ?? throw new InvalidOperationException(
                    $"PageUrl: no registered endpoint has route name '{routeName}'. " +
                    "Ensure the target action carries [HttpGet(Name = \"...\")] (or equivalent) " +
                    "and that its controller is registered with MapControllers().");

            var consumerValues = routeValues(cursor, appliedLimit)
                ?? throw new InvalidOperationException(
                    "PageUrl: the routeValues callback returned null. Return a RouteValueDictionary " +
                    "(possibly empty) instead.");

            // Clone before injecting api-version so the consumer's dict — which may be a shared
            // instance returned from a closure — is never mutated. Required by the contract
            // documented on the routeValues parameter.
            var values = new RouteValueDictionary(consumerValues);

            // Consumer-supplied api-version wins over framework injection — gives callers
            // an escape hatch for cross-version Location-like URLs without a separate overload.
            if (!values.ContainsKey(HttpResponseOptionsBuilderApiVersioningExtensions.DefaultRouteValueKey))
            {
                var resolved = HttpResponseOptionsBuilderApiVersioningExtensions.ResolveApiVersion(
                    httpContext,
                    targetEndpoint,
                    callerLabel: "the next-page URL",
                    explicitOverloadHint: "PageUrl(routeName, ApiVersion, ...)");
                if (resolved is not null)
                {
                    values[HttpResponseOptionsBuilderApiVersioningExtensions.DefaultRouteValueKey] = resolved;
                }
            }

            var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
            var url = linkGenerator.GetUriByRouteValues(httpContext, routeName, values);
            if (url is null)
            {
                throw new InvalidOperationException(
                    $"PageUrl: LinkGenerator.GetUriByRouteValues returned null for route '{routeName}'. " +
                    "The supplied route values, combined with the current request's ambient route values, " +
                    "did not match the target route template.");
            }

            return url;
        };
    }

    /// <summary>
    /// Returns a <c>(Cursor, int) -&gt; string</c> builder pinned to a specific
    /// <see cref="ApiVersion"/> regardless of what the client requested. Use for cross-version
    /// list-page URLs (e.g. a deprecated endpoint redirecting paginated traffic to its successor).
    /// </summary>
    /// <param name="httpContext">The current request context.</param>
    /// <param name="routeName">The name of the target route.</param>
    /// <param name="version">The version to inject, regardless of client request or endpoint metadata.</param>
    /// <param name="routeValues">
    /// A callback that maps <c>(cursor, appliedLimit)</c> to the route-value dictionary for the
    /// next-page URL. The helper clones this dictionary before injecting <c>api-version</c>.
    /// </param>
    /// <returns>A request-scoped URL builder. Must be invoked during the same request.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="httpContext"/>, <paramref name="version"/>, or <paramref name="routeValues"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="routeName"/> is <c>null</c>, empty, or whitespace.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown by the returned builder when (a) the target route name resolves to no registered
    /// endpoint, (b) the target route is URL-segment-versioned (route template contains
    /// <c>:apiVersion</c>) — the pin cannot be honoured as a query parameter because the version
    /// belongs in the path segment, and silently dropping the pin would let <c>LinkGenerator</c>
    /// fill the segment from ambient route data and produce a URL with the wrong version; switch
    /// to the per-request overload <see cref="PageUrl(HttpContext, string, Func{Cursor, int, RouteValueDictionary})"/>,
    /// or (c) <c>LinkGenerator</c> returns <c>null</c>.
    /// </exception>
    /// <remarks>
    /// The explicit version is still silently suppressed on version-neutral endpoints — emitting
    /// a query <c>api-version</c> on a neutral endpoint would mislead clients into resending the
    /// URL with a parameter the target rejects.
    /// </remarks>
    public static Func<Cursor, int, string> PageUrl(
        this HttpContext httpContext,
        string routeName,
        ApiVersion version,
        Func<Cursor, int, RouteValueDictionary> routeValues)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ThrowIfRouteNameInvalid(routeName);
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(routeValues);

        // Cache the stringified version outside the closure — matches the
        // WithVersionedRoute(ApiVersion) precedent and avoids re-allocating on every call.
        var pinnedValue = version.ToString();

        return (cursor, appliedLimit) =>
        {
            var targetEndpoint = FindEndpointByRouteName(httpContext, routeName)
                ?? throw new InvalidOperationException(
                    $"PageUrl: no registered endpoint has route name '{routeName}'. " +
                    "Ensure the target action carries [HttpGet(Name = \"...\")] (or equivalent) " +
                    "and that its controller is registered with MapControllers().");

            // URL-segment-versioned targets carry the version in the path. Silently skipping
            // the pin (the WithVersionedRoute precedent for same-route Location headers) would
            // let LinkGenerator fall back to ambient route data — emitting `/v1/...` even when
            // the caller explicitly pinned v2. That is a wrong URL, not just a missing query
            // parameter. Surface it so the caller switches to the implicit overload (which
            // resolves the segment from ambient route data correctly for same-version targets).
            if (HttpResponseOptionsBuilderApiVersioningExtensions.RouteTemplateContainsVersionToken(targetEndpoint))
            {
                throw new InvalidOperationException(
                    $"PageUrl: cannot pin api-version='{pinnedValue}' on route '{routeName}' because it " +
                    "uses URL-segment versioning (the route template contains a ':apiVersion' constraint). " +
                    "Silently skipping the pin would let LinkGenerator fill the version segment from the " +
                    "current request's ambient route values, producing a URL with the wrong version. " +
                    "Drop the explicit version and use the per-request overload " +
                    "PageUrl(routeName, routeValues) — it resolves the segment from ambient route data " +
                    "correctly for same-version targets and validates cross-route target-version support.");
            }

            var consumerValues = routeValues(cursor, appliedLimit)
                ?? throw new InvalidOperationException(
                    "PageUrl: the routeValues callback returned null. Return a RouteValueDictionary " +
                    "(possibly empty) instead.");

            var values = new RouteValueDictionary(consumerValues);

            // Skip injection silently for version-neutral targets (same precedent as
            // WithVersionedRoute(ApiVersion)): neutral endpoints reject api-version parameters
            // entirely, so emitting one would mislead clients. The pin is honoured for everything
            // else — query/header versioning, no metadata at all, etc.
            if (!values.ContainsKey(HttpResponseOptionsBuilderApiVersioningExtensions.DefaultRouteValueKey)
                && !HttpResponseOptionsBuilderApiVersioningExtensions.ShouldSkipInjection(targetEndpoint))
            {
                values[HttpResponseOptionsBuilderApiVersioningExtensions.DefaultRouteValueKey] = pinnedValue;
            }

            var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
            var url = linkGenerator.GetUriByRouteValues(httpContext, routeName, values);
            if (url is null)
            {
                throw new InvalidOperationException(
                    $"PageUrl: LinkGenerator.GetUriByRouteValues returned null for route '{routeName}'. " +
                    "The supplied route values, combined with the current request's ambient route values, " +
                    "did not match the target route template.");
            }

            return url;
        };
    }

    private static void ThrowIfRouteNameInvalid(string routeName)
    {
        ArgumentNullException.ThrowIfNull(routeName);
        if (string.IsNullOrWhiteSpace(routeName))
            throw new ArgumentException("Route name must be a non-empty, non-whitespace string.", nameof(routeName));
    }

    private static Endpoint? FindEndpointByRouteName(HttpContext httpContext, string routeName)
    {
        // EndpointDataSource is the supported public API for enumerating registered endpoints
        // (MVC registers a CompositeEndpointDataSource; the framework merges all sources).
        // Walking it is O(endpoints) per call. Paginated controllers are not the hot path of
        // a high-throughput API — typical pages serve tens of items at the cost of a database
        // round-trip, so this lookup is in the noise. Caching would require invalidation
        // semantics for dynamically-modified endpoint sources (development hot-reload, plug-ins),
        // and the corresponding bug class is harder to detect than this lookup is expensive.
        var dataSource = httpContext.RequestServices.GetService<EndpointDataSource>();
        if (dataSource is null)
            return null;

        foreach (var endpoint in dataSource.Endpoints)
        {
            var routeNameMetadata = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.RouteNameMetadata>();
            if (routeNameMetadata is not null
                && string.Equals(routeNameMetadata.RouteName, routeName, StringComparison.Ordinal))
            {
                return endpoint;
            }
        }

        return null;
    }
}
