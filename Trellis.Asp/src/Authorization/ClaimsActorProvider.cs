namespace Trellis.Asp.Authorization;

using System.Collections.Frozen;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
    /// Defaults to <c>"sub"</c> (the RFC 7519 / OpenID Connect subject claim).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Matched against <see cref="System.Security.Claims.Claim.Type"/> verbatim;
    /// no dotted-path traversal. See the <see cref="ClaimsActorOptions"/> remarks
    /// for nested-claim guidance.
    /// </para>
    /// <para>
    /// <b>JwtBearer <c>MapInboundClaims</c> robustness.</b> ASP.NET Core's
    /// <c>JwtBearerOptions.MapInboundClaims</c> defaults to <see langword="true"/>,
    /// which remaps RFC 7519 short claim names (e.g. <c>"sub"</c>) onto WS-* long-form
    /// URNs (e.g. <see cref="System.Security.Claims.ClaimTypes.NameIdentifier"/>)
    /// before they reach <see cref="System.Security.Claims.ClaimsIdentity"/>. If the
    /// configured <see cref="ActorIdClaim"/> is not found literally,
    /// <see cref="ClaimsActorProvider"/> automatically falls back to the well-known
    /// short↔long counterpart from the JWT inbound claim-name map (<c>sub</c> ↔
    /// <c>NameIdentifier</c>, <c>email</c> ↔ <c>Email</c>, <c>role</c>/<c>roles</c> ↔
    /// <c>Role</c>, <c>name</c>/<c>unique_name</c> ↔ <c>Name</c>, etc.) so the default
    /// configuration just-works with either <c>MapInboundClaims = true</c> or
    /// <c>MapInboundClaims = false</c>. The fallback emits a debug-level log entry
    /// when it fires. Recommended for new services: set <c>MapInboundClaims = false</c>
    /// on <c>AddJwtBearer(...)</c> so claim names round-trip with their RFC names.
    /// </para>
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
public class ClaimsActorProvider : IActorProvider
{
    /// <summary>
    /// Well-known short → long claim name pairs that match
    /// <c>JwtSecurityTokenHandler.DefaultInboundClaimTypeMap</c> /
    /// <c>JsonWebTokenHandler.DefaultInboundClaimTypeMap</c>. When
    /// <c>JwtBearerOptions.MapInboundClaims = true</c> (the ASP.NET Core default),
    /// the handler emits the long-form WS-* URN onto the <see cref="ClaimsIdentity"/>
    /// instead of the short RFC 7519 / OIDC claim name. The provider treats these
    /// pairs as equivalent: if the configured claim name is not found, the
    /// counterpart from this map is tried as a fallback (Postel's-law robustness
    /// against ASP.NET Core's legacy MapInboundClaims default — same shape that
    /// <see cref="EntraActorProvider"/> uses for its <c>oid</c> lookup).
    /// </summary>
    /// <remarks>
    /// Bidirectional lookup is performed in <see cref="ResolveClaimWithFallback"/>:
    /// configured short → mapped long, configured long → mapped short.
    /// Hardcoded to avoid taking a dependency on
    /// <c>System.IdentityModel.Tokens.Jwt</c> from this package; the mapping table is
    /// effectively frozen — no new entries have been added to the JWT inbound map
    /// in years.
    /// </remarks>
    private static readonly FrozenDictionary<string, string> KnownShortToLongClaimNames =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["sub"] = ClaimTypes.NameIdentifier,
            ["nameid"] = ClaimTypes.NameIdentifier,
            ["name"] = ClaimTypes.Name,
            ["unique_name"] = ClaimTypes.Name,
            ["email"] = ClaimTypes.Email,
            ["role"] = ClaimTypes.Role,
            ["roles"] = ClaimTypes.Role,
            ["family_name"] = ClaimTypes.Surname,
            ["given_name"] = ClaimTypes.GivenName,
            ["gender"] = ClaimTypes.Gender,
            ["birthdate"] = ClaimTypes.DateOfBirth,
            ["website"] = ClaimTypes.Webpage,
            ["actort"] = ClaimTypes.Actor,
        }.ToFrozenDictionary(StringComparer.Ordinal);

    /// <summary>
    /// Inverse lookup table built from <see cref="KnownShortToLongClaimNames"/>: each long-form
    /// URN maps to the <em>set</em> of short forms that alias to it. Multi-valued because
    /// several short forms can collapse onto the same long form (e.g. <c>"sub"</c> and
    /// <c>"nameid"</c> both alias to <see cref="ClaimTypes.NameIdentifier"/>; <c>"name"</c> and
    /// <c>"unique_name"</c> both alias to <see cref="ClaimTypes.Name"/>; <c>"role"</c> and
    /// <c>"roles"</c> both alias to <see cref="ClaimTypes.Role"/>). The reverse fallback must
    /// iterate every candidate short form — picking only one canonical short form would
    /// silently fail to resolve tokens carrying the alternate short variant and reproduce
    /// the same silent-401 footgun this fallback exists to prevent.
    /// </summary>
    private static readonly FrozenDictionary<string, FrozenSet<string>> KnownLongToShortClaimNames =
        KnownShortToLongClaimNames
            .GroupBy(kvp => kvp.Value, StringComparer.Ordinal)
            .ToFrozenDictionary(
                g => g.Key,
                g => g.Select(kvp => kvp.Key).ToFrozenSet(StringComparer.Ordinal),
                StringComparer.Ordinal);

    private readonly ILogger<ClaimsActorProvider>? _logger;

    /// <summary>
    /// Initializes a new <see cref="ClaimsActorProvider"/>.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for the current request's <see cref="HttpContext"/>.</param>
    /// <param name="options">Claim-name mapping options.</param>
    /// <param name="logger">
    /// Optional logger; when supplied, emits a debug-level message the first time the
    /// short↔long claim-name fallback fires for a given request. Helpful for diagnosing
    /// "always 401" issues caused by <c>JwtBearerOptions.MapInboundClaims = true</c>.
    /// </param>
    public ClaimsActorProvider(
        IHttpContextAccessor httpContextAccessor,
        IOptions<ClaimsActorOptions> options,
        ILogger<ClaimsActorProvider>? logger = null)
    {
        HttpContextAccessor = httpContextAccessor;
        Options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// The HTTP context accessor used to retrieve the current user.
    /// </summary>
    protected IHttpContextAccessor HttpContextAccessor { get; }

    /// <summary>
    /// The configured claim mapping options.
    /// </summary>
    protected ClaimsActorOptions Options { get; }

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
        //
        // Fallback: if the configured claim is one of the well-known JWT short↔long pairs
        // (e.g., "sub" ↔ ClaimTypes.NameIdentifier), also try its counterpart. This is the
        // robustness measure for ASP.NET Core's JwtBearerOptions.MapInboundClaims = true
        // default, which silently remaps RFC 7519 short names onto WS-* long-form URNs.
        var actorId = ResolveClaimWithFallback(identity, Options.ActorIdClaim);
        if (actorId is null)
            return Task.FromResult(Maybe<Actor>.None);

        var permissions = identity.FindAll(Options.PermissionsClaim)
            .Select(c => c.Value)
            .ToFrozenSet();

        var actor = Actor.Create(actorId, permissions);
        return Task.FromResult(Maybe.From(actor));
    }

    /// <summary>
    /// Looks up <paramref name="configuredClaim"/> on the identity; if not found, tries the
    /// well-known short↔long counterpart(s) from <see cref="KnownShortToLongClaimNames"/>.
    /// Returns the resolved value, or <see langword="null"/> when neither form is present.
    /// </summary>
    /// <remarks>
    /// The forward direction (configured short → long) has a single counterpart per short
    /// form. The reverse direction (configured long → short) iterates every short form that
    /// aliases to the long form (e.g., <c>NameIdentifier</c> ← {<c>sub</c>, <c>nameid</c>};
    /// <c>Name</c> ← {<c>name</c>, <c>unique_name</c>}; <c>Role</c> ← {<c>role</c>,
    /// <c>roles</c>}) — picking only one short form would silently fail to resolve tokens
    /// that carry the alternate variant.
    /// </remarks>
    private string? ResolveClaimWithFallback(ClaimsIdentity identity, string configuredClaim)
    {
        var literal = identity.FindFirst(configuredClaim)?.Value;
        if (literal is not null)
            return literal;

        // Forward direction: configured short form → its single long-form counterpart.
        if (KnownShortToLongClaimNames.TryGetValue(configuredClaim, out var longForm))
        {
            var forward = identity.FindFirst(longForm)?.Value;
            if (forward is not null)
            {
                LogClaimNameFallback(_logger, configuredClaim, longForm);
                return forward;
            }
        }

        // Reverse direction: configured long form → iterate every short form that maps to it.
        // Picking only one canonical short would miss tokens carrying the alternate variant
        // (e.g., "nameid" instead of "sub" for ClaimTypes.NameIdentifier).
        if (KnownLongToShortClaimNames.TryGetValue(configuredClaim, out var shortForms))
        {
            foreach (var shortForm in shortForms)
            {
                var reverse = identity.FindFirst(shortForm)?.Value;
                if (reverse is not null)
                {
                    LogClaimNameFallback(_logger, configuredClaim, shortForm);
                    return reverse;
                }
            }
        }

        return null;
    }

    private static readonly Action<ILogger, string, string, Exception?> _logClaimNameFallback =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(1, nameof(ClaimsActorProvider)),
            "ClaimsActorProvider resolved actor id via short<->long claim-name fallback. " +
            "Configured ActorIdClaim '{ConfiguredClaim}' was not present on the authenticated " +
            "identity; resolved via the well-known counterpart '{ResolvedClaim}'. This typically " +
            "indicates JwtBearerOptions.MapInboundClaims = true (the ASP.NET Core default); " +
            "set MapInboundClaims = false to keep claim names round-tripping with their RFC 7519 / " +
            "OIDC names. The fallback continues to accept both forms.");

    private static void LogClaimNameFallback(ILogger<ClaimsActorProvider>? logger, string configured, string resolved)
    {
        if (logger is not null)
            _logClaimNameFallback(logger, configured, resolved, null);
    }
}