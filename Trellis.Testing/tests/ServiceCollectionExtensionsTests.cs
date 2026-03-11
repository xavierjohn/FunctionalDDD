namespace Trellis.Testing.Tests;

using Microsoft.Extensions.DependencyInjection;
using Trellis.Authorization;

/// <summary>
/// Tests for <see cref="ServiceCollectionExtensions.ReplaceResourceLoader{TMessage, TResource}"/>.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    #region ReplaceResourceLoader

    [Fact]
    public void ReplaceResourceLoader_NoExistingRegistration_RegistersLoader()
    {
        var services = new ServiceCollection();
        var loader = new FakeResourceLoader();

        services.ReplaceResourceLoader<TestCommand, TestResource>(loader);

        using var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<IResourceLoader<TestCommand, TestResource>>();
        resolved.Should().BeSameAs(loader);
    }

    [Fact]
    public void ReplaceResourceLoader_WithExistingRegistration_ReplacesIt()
    {
        var services = new ServiceCollection();
        var original = new FakeResourceLoader();
        var replacement = new FakeResourceLoader();
        services.AddScoped<IResourceLoader<TestCommand, TestResource>>(_ => original);

        services.ReplaceResourceLoader<TestCommand, TestResource>(replacement);

        using var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<IResourceLoader<TestCommand, TestResource>>();
        resolved.Should().BeSameAs(replacement);
    }

    [Fact]
    public void ReplaceResourceLoader_WithMultipleExistingRegistrations_ReplacesAll()
    {
        var services = new ServiceCollection();
        services.AddScoped<IResourceLoader<TestCommand, TestResource>>(_ => new FakeResourceLoader());
        services.AddScoped<IResourceLoader<TestCommand, TestResource>>(_ => new FakeResourceLoader());
        var replacement = new FakeResourceLoader();

        services.ReplaceResourceLoader<TestCommand, TestResource>(replacement);

        using var provider = services.BuildServiceProvider();
        var all = provider.GetServices<IResourceLoader<TestCommand, TestResource>>().ToList();
        all.Should().ContainSingle().Which.Should().BeSameAs(replacement);
    }

    [Fact]
    public void ReplaceResourceLoader_ReturnsServiceCollection_ForChaining()
    {
        var services = new ServiceCollection();
        var loader = new FakeResourceLoader();

        var returned = services.ReplaceResourceLoader<TestCommand, TestResource>(loader);

        returned.Should().BeSameAs(services);
    }

    [Fact]
    public void ReplaceResourceLoader_DoesNotAffectOtherResourceLoaders()
    {
        var services = new ServiceCollection();
        var otherLoader = new OtherFakeResourceLoader();
        services.AddScoped<IResourceLoader<OtherCommand, OtherResource>>(_ => otherLoader);
        var replacement = new FakeResourceLoader();

        services.ReplaceResourceLoader<TestCommand, TestResource>(replacement);

        using var provider = services.BuildServiceProvider();
        var other = provider.GetRequiredService<IResourceLoader<OtherCommand, OtherResource>>();
        other.Should().BeSameAs(otherLoader);
    }

    #endregion

    #region Scoped Lifetime Tests

    [Fact]
    public void ReplaceResourceLoader_ResolvesFromScopedProvider_ReturnsSameInstance()
    {
        var services = new ServiceCollection();
        var loader = new FakeResourceLoader();
        services.ReplaceResourceLoader<TestCommand, TestResource>(loader);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var resolved = scope.ServiceProvider.GetRequiredService<IResourceLoader<TestCommand, TestResource>>();
        resolved.Should().BeSameAs(loader);
    }

    [Fact]
    public void ReplaceResourceLoader_TwoScopes_ResolveSameInstance()
    {
        var services = new ServiceCollection();
        var loader = new FakeResourceLoader();
        services.ReplaceResourceLoader<TestCommand, TestResource>(loader);

        using var provider = services.BuildServiceProvider();
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var resolved1 = scope1.ServiceProvider.GetRequiredService<IResourceLoader<TestCommand, TestResource>>();
        var resolved2 = scope2.ServiceProvider.GetRequiredService<IResourceLoader<TestCommand, TestResource>>();

        resolved1.Should().BeSameAs(loader);
        resolved2.Should().BeSameAs(loader);
        resolved1.Should().BeSameAs(resolved2);
    }

    [Fact]
    public void ReplaceResourceLoader_DisposableLoader_NotDisposedWhenScopeEnds()
    {
        var services = new ServiceCollection();
        var loader = new DisposableFakeResourceLoader();
        services.ReplaceResourceLoader<TestCommand, TestResource>(loader);

        using var provider = services.BuildServiceProvider();

        var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IResourceLoader<TestCommand, TestResource>>();
        scope.Dispose();

        loader.Disposed.Should().BeFalse("singleton instances are not disposed when a scope ends");
    }

    [Fact]
    public void ReplaceResourceLoader_DisposableLoader_SecondScopeStillUsable()
    {
        var services = new ServiceCollection();
        var loader = new DisposableFakeResourceLoader();
        services.ReplaceResourceLoader<TestCommand, TestResource>(loader);

        using var provider = services.BuildServiceProvider();

        var scope1 = provider.CreateScope();
        scope1.ServiceProvider.GetRequiredService<IResourceLoader<TestCommand, TestResource>>();
        scope1.Dispose();

        using var scope2 = provider.CreateScope();
        var resolved = scope2.ServiceProvider.GetRequiredService<IResourceLoader<TestCommand, TestResource>>();

        resolved.Should().BeSameAs(loader);
        loader.Disposed.Should().BeFalse("the instance must survive across scopes");
    }

    #endregion

    #region Test Types

    private sealed record TestCommand(string Id);
    private sealed record TestResource(string Name);
    private sealed record OtherCommand(string Id);
    private sealed record OtherResource(string Name);

    private sealed class FakeResourceLoader : IResourceLoader<TestCommand, TestResource>
    {
        public Task<Result<TestResource>> LoadAsync(TestCommand message, CancellationToken ct) =>
            Task.FromResult(Result.Success(new TestResource("test")));
    }

    private sealed class OtherFakeResourceLoader : IResourceLoader<OtherCommand, OtherResource>
    {
        public Task<Result<OtherResource>> LoadAsync(OtherCommand message, CancellationToken ct) =>
            Task.FromResult(Result.Success(new OtherResource("other")));
    }

    private sealed class DisposableFakeResourceLoader : IResourceLoader<TestCommand, TestResource>, IDisposable
    {
        public bool Disposed { get; private set; }

        public Task<Result<TestResource>> LoadAsync(TestCommand message, CancellationToken ct) =>
            Task.FromResult(Result.Success(new TestResource("disposable")));

        public void Dispose() => Disposed = true;
    }

    #endregion
}
