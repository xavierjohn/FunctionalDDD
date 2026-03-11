namespace Trellis.Testing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Trellis.Authorization;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> that simplify
/// replacing service registrations in integration tests.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Replaces any existing <see cref="IResourceLoader{TMessage, TResource}"/> registration
    /// with the specified <paramref name="loader"/> instance, registered as scoped.
    /// </summary>
    /// <typeparam name="TMessage">The command or query type that identifies the resource.</typeparam>
    /// <typeparam name="TResource">The resource type returned by the loader.</typeparam>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="loader">The replacement resource loader instance.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureServices(services =>
    /// {
    ///     services.ReplaceResourceLoader&lt;CancelOrderCommand, Order&gt;(
    ///         new FakeOrderResourceLoader(fakeRepo));
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection ReplaceResourceLoader<TMessage, TResource>(
        this IServiceCollection services,
        IResourceLoader<TMessage, TResource> loader)
    {
        services.RemoveAll<IResourceLoader<TMessage, TResource>>();
        services.AddScoped(_ => loader);
        return services;
    }
}
