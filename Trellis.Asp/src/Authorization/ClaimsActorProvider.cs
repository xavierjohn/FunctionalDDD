namespace Trellis.Asp.Authorization;

using System.Collections.Frozen;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Trellis.Authorization;

/// <summary>
/// Configuration options for <see cref="ClaimsActorProvider"/>.
/// Controls which claims are used for actor identity and permissions.
/// </summary>
/// <remarks>
/// <para>
/// Default behavior maps the OIDC standard <c>sub</c> claim to
/// <see cref="Actor.Id"/> and the <c>permissions</c> claim to
/// <see cref="Actor.Permissions"/>. Override the claim names
/// to match your identity provider's token format.
/// </para>
/// <para>
/// <b>Nested-claim mapping</b> (ga-15). Both <see cref="ActorIdClaim"/> and
/// <see cref="PermissionsClaim"/> are matched against the flat
/// <see cref="System.Security.Claims.Claim.Type"/> string only — no JSON-path
/// or dotted traversal is performed. Two practical consequences:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <b>Provider-prefixed claim names work as-is.</b> Set the option to the
///       full claim type the JWT handler emits, e.g.
///       <c>"http://schemas.microsoft.com/identity/claims/objectidentifier"</c>
///       (Entra <c>oid</c>) or <c>"extension_role"</c> (Entra External Identities
///       custom attribute). Whatever the issuer/handler exposes via
///       <c>ClaimsIdentity.FindFirst(name)</c> is what to put here.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>JSON-object claims are not auto-flattened.</b> When a JWT contains
///       a nested object (e.g. <c>{ "app_metadata": { "roles": [ ... ] } }</c>),
///       the JWT handler stores the value as the raw JSON string under
///       claim type <c>"app_metadata"</c>. To project nested values into
///       <see cref="Actor.Permissions"/>, subclass <see cref="ClaimsActorProvider"/>
///       and override <see cref="ClaimsActorProvider.GetCurrentActorAsync"/> to
///       parse the JSON yourself, or use <see cref="EntraActorProvider"/> with a
///       custom <see cref="EntraActorOptions.MapPermissions"/> delegate which
///       receives the full <see cref="System.Security.Claims.Claim"/> sequence.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Multi-valued JWT claims are flattened automatically</b> by
///       <c>JwtBearerHandler</c> — a JSON array such as
///       <c>"permissions": [ "orders:read", "orders:write" ]</c> arrives as
///       multiple <see cref="System.Security.Claims.Claim"/> instances of the
///       same <see cref="System.Security.Claims.Claim.Type"/>, which the default
///       provider already aggregates via <c>FindAll</c>.
///     </description>
///   </item>
/// </list>
/// </remarks>
public class ClaimsActorOptions
{
    /// <summary>
    /// The claim type used to resolve the actor's unique identifier.
    /// Defaults to <c>"sub"</c> (OIDC standard subject claim).
    /// </summary>
    /// <remarks>
    /// Matched against <see cref="System.Security.Claims.Claim.Type"/> verbatim;
    /// no dotted-path traversal. See the <see cref="ClaimsActorOptions"/> remarks
    /// for nested-claim guidance.
    /// </remarks>
    public string ActorIdClaim { get; set; } = "sub";

    /// <summary>
    /// The claim type used to resolve the actor's permissions.
    /// Defaults to <c>"permissions"</c> (common convention in OIDC/JWT tokens).
    /// </summary>
    /// <remarks>
    /// Matched against <see cref="System.Security.Claims.Claim.Type"/> verbatim;
    /// every matching claim contributes one entry. See the
    /// <see cref="ClaimsActorOptions"/> remarks for nested-claim guidance.
    /// </remarks>
    public string PermissionsClaim { get; set; } = "permissions";
}

/// <summary>
/// <see cref="IActorProvider"/> implementation that hydrates an <see cref="Actor"/>
/// from the current <see cref="HttpContext.User"/> using standard JWT/OIDC claims.
/// Works with any identity provider (Auth0, Keycloak, Okta, Entra, etc.).
/// </summary>
/// <remarks>
/// <para>
/// Register as scoped in DI via
/// <see cref="ServiceCollectionExtensions.AddClaimsActorProvider"/>.
/// Claim mapping is controlled by <see cref="ClaimsActorOptions"/>.
/// </para>
/// <para>
/// This provider assumes authentication has already occurred. It returns
/// <see cref="Maybe{T}.None"/> when the request has no usable authenticated actor —
/// no authenticated <see cref="System.Security.Claims.ClaimsIdentity"/>, or the configured
/// <see cref="ClaimsActorOptions.ActorIdClaim"/> is missing from the authenticated identity —
/// and the mediator authorization pipeline maps that to <see cref="Error.Unauthorized"/>
/// (HTTP 401). It throws <see cref="InvalidOperationException"/> only when invoked outside
/// an HTTP request scope (no <c>HttpContext</c>); that is a configuration bug, not
/// authentication state, and surfaces as HTTP 500.
/// </para>
/// <para>
/// <b>Extending for nested or computed claims.</b> The default mapping is flat
/// (see the <see cref="ClaimsActorOptions"/> remarks). For provider-specific
/// shapes — JSON-object claims, dotted paths, claims that need to be merged with
/// an external store, or permissions derived from multiple raw claims — subclass
/// this provider and override <see cref="GetCurrentActorAsync"/>. The
/// <see cref="HttpContextAccessor"/> and <see cref="Options"/> properties are
/// <c>protected</c> precisely to support this. <see cref="EntraActorProvider"/>
/// is a worked example.
/// </para>
/// </remarks>
public class ClaimsActorProvider(
    IHttpContextAccessor httpContextAccessor,
    IOptions<ClaimsActorOptions> options) : IActorProvider
{
    /// <summary>
    /// The HTTP context accessor used to retrieve the current user.
    /// </summary>
    protected IHttpContextAccessor HttpContextAccessor { get; } = httpContextAccessor;

    /// <summary>
    /// The configured claim mapping options.
    /// </summary>
    protected ClaimsActorOptions Options { get; } = options.Value;

    /// <inheritdoc />
    public virtual Task<Maybe<Actor>> GetCurrentActorAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // HttpContext missing is genuinely exceptional: the provider was invoked outside an
        // HTTP request scope (e.g., from a background worker or test bootstrap). That's a
        // configuration bug, not authentication state — throw rather than return None, which
        // would mask the bug as a 401.
        var httpContext = HttpContextAccessor.HttpContext
            ?? throw new InvalidOperationException(
                "No HttpContext available. Ensure this is called within an HTTP request scope.");

        // The request has no authenticated identity (no Authorization header, anonymous-tolerant
        // endpoint, expired/invalid token accepted as anonymous, etc.). Client-error state →
        // Maybe.None → the mediator pipeline emits 401.
        var identity = httpContext.User.Identities.FirstOrDefault(i => i.IsAuthenticated) as ClaimsIdentity;
        if (identity is null)
            return Task.FromResult(Maybe<Actor>.None);

        // Authenticated identity is present but the configured ActorIdClaim is missing. From
        // the framework's POV we can't identify the actor — same client-facing outcome as no
        // identity. (Whether this is a token-shape issue or a server-side option misconfig
        // is indistinguishable here; both want a 401 retry by the client.)
        var actorId = identity.FindFirst(Options.ActorIdClaim)?.Value;
        if (actorId is null)
            return Task.FromResult(Maybe<Actor>.None);

        var permissions = identity.FindAll(Options.PermissionsClaim)
            .Select(c => c.Value)
            .ToFrozenSet();

        var actor = Actor.Create(actorId, permissions);
        return Task.FromResult(Maybe.From(actor));
    }
}