namespace Trellis.Asp.Authorization.Tests;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trellis.Authorization;

/// <summary>
/// Tests for <see cref="ServiceCollectionExtensions.AddEntraActorProvider"/>.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddEntraActorProvider_RegistersIActorProvider()
    {
        var services = new ServiceCollection();

        services.AddEntraActorProvider();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IActorProvider));
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be<EntraActorProvider>();
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddEntraActorProvider_RegistersHttpContextAccessor()
    {
        var services = new ServiceCollection();

        services.AddEntraActorProvider();

        services.Should().Contain(d =>
            d.ServiceType == typeof(Microsoft.AspNetCore.Http.IHttpContextAccessor));
    }

    [Fact]
    public void AddEntraActorProvider_RegistersDefaultOptions()
    {
        var services = new ServiceCollection();
        services.AddEntraActorProvider();
        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<EntraActorOptions>>();

        options.Value.Should().NotBeNull();
        options.Value.IdClaimType.Should().Contain("objectidentifier");
    }

    [Fact]
    public void AddEntraActorProvider_WithConfigure_AppliesOptions()
    {
        var services = new ServiceCollection();
        services.AddEntraActorProvider(opts => opts.IdClaimType = "sub");
        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<EntraActorOptions>>();

        options.Value.IdClaimType.Should().Be("sub");
    }
}