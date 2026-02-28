namespace Trellis.Asp.Authorization;

using Microsoft.Extensions.DependencyInjection;
using Trellis.Authorization;

/// <summary>
/// Extension methods for registering <see cref="EntraActorProvider"/> in ASP.NET Core DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="EntraActorProvider"/> as the scoped <see cref="IActorProvider"/>
    /// and configures <see cref="EntraActorOptions"/> with default Entra v2.0 claim mappings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">
    /// Optional delegate to customize <see cref="EntraActorOptions"/>.
    /// Override <see cref="EntraActorOptions.MapPermissions"/> to flatten roles into granular permissions,
    /// <see cref="EntraActorOptions.MapForbiddenPermissions"/> to populate deny lists, or
    /// <see cref="EntraActorOptions.MapAttributes"/> to add custom ABAC attributes.
    /// </param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddEntraActorProvider(options =>
    /// {
    ///     options.MapPermissions = claims => claims
    ///         .Where(c => c.Type == "roles")
    ///         .SelectMany(role => RolePermissionMap[role.Value])
    ///         .ToHashSet();
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddEntraActorProvider(
        this IServiceCollection services,
        Action<EntraActorOptions>? configure = null)
    {
        services.AddHttpContextAccessor();

        if (configure is not null)
            services.Configure(configure);
        else
            services.Configure<EntraActorOptions>(_ => { });

        services.AddScoped<IActorProvider, EntraActorProvider>();

        return services;
    }
}