namespace Trellis.Mediator.Tests;

using Microsoft.Extensions.DependencyInjection;
using Trellis.Authorization;

/// <summary>
/// Tests for <see cref="ServiceCollectionExtensions.AddResourceLoaders"/>.
/// </summary>
public class AddResourceLoadersTests
{
    #region Scan registers concrete loaders

    [Fact]
    public void AddResourceLoaders_RegistersConcreteLoaderFromAssembly()
    {
        var services = new ServiceCollection();

        services.AddResourceLoaders(typeof(TestLoader).Assembly);

        var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(IResourceLoader<TestMessage, TestEntity>));

        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be<TestLoader>();
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    #endregion

    #region Scan skips abstract and interface types

    [Fact]
    public void AddResourceLoaders_SkipsAbstractTypes()
    {
        var services = new ServiceCollection();

        services.AddResourceLoaders(typeof(AbstractLoader).Assembly);

        services.Should().NotContain(
            d => d.ImplementationType == typeof(AbstractLoader));
    }

    #endregion

    #region Resolved loader is functional

    [Fact]
    public async Task AddResourceLoaders_ResolvedLoaderIsUsable()
    {
        var services = new ServiceCollection();
        services.AddResourceLoaders(typeof(TestLoader).Assembly);
        await using var sp = services.BuildServiceProvider();

        using var scope = sp.CreateScope();
        var loader = scope.ServiceProvider.GetRequiredService<IResourceLoader<TestMessage, TestEntity>>();
        var result = await loader.LoadAsync(new TestMessage("id-1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be("id-1");
    }

    #endregion

    #region Returns service collection for chaining

    [Fact]
    public void AddResourceLoaders_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();

        var returned = services.AddResourceLoaders(typeof(TestLoader).Assembly);

        returned.Should().BeSameAs(services);
    }

    #endregion

    #region Test helpers

    public sealed record TestMessage(string Id);
    public sealed record TestEntity(string Id);

    public sealed class TestLoader : IResourceLoader<TestMessage, TestEntity>
    {
        public Task<Result<TestEntity>> LoadAsync(TestMessage message, CancellationToken ct)
            => Task.FromResult(Result.Success(new TestEntity(message.Id)));
    }

    public abstract class AbstractLoader : IResourceLoader<TestMessage, TestEntity>
    {
        public abstract Task<Result<TestEntity>> LoadAsync(TestMessage message, CancellationToken ct);
    }

    #endregion
}
