namespace Trellis.ServiceDefaults.Tests;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using global::Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trellis.Asp;
using Trellis.Asp.Authorization;
using Trellis.Authorization;
using Trellis.EntityFrameworkCore;
using Trellis.Mediator;
using DefaultHttpContext = Microsoft.AspNetCore.Http.DefaultHttpContext;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;
using ProblemDetailsContext = Microsoft.AspNetCore.Http.ProblemDetailsContext;
using ProblemDetailsOptions = Microsoft.AspNetCore.Http.ProblemDetailsOptions;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

/// <summary>
/// Tests for <see cref="TrellisServiceBuilder"/>.
/// </summary>
public class TrellisServiceBuilderTests
{
    [Fact]
    public void UseEntityFrameworkUnitOfWork_AppliesTransactionalBehaviorLast()
    {
        var services = new ServiceCollection();

        services.AddTrellis(options => options
            .UseMediator()
            .UseEntityFrameworkUnitOfWork<TestDbContext>());

        var behaviorTypes = services
            .Where(d => d.ServiceType == typeof(IPipelineBehavior<,>))
            .Select(d => d.ImplementationType)
            .ToList();

        behaviorTypes.Should().EndWith(typeof(TransactionalCommandBehavior<,>));
        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IUnitOfWork) &&
            d.ImplementationType == typeof(EfUnitOfWork<TestDbContext>));
    }

    [Fact]
    public void UseFluentValidation_ImpliedMediatorAndRegistersAdapter()
    {
        var services = new ServiceCollection();

        services.AddTrellis(options => options
            .UseFluentValidation(typeof(TrellisServiceBuilderTests).Assembly));

        services.Should().Contain(d =>
            d.ServiceType == typeof(IPipelineBehavior<,>) &&
            d.ImplementationType == typeof(ValidationBehavior<,>));
        services.Count(d =>
            d.ServiceType == typeof(IMessageValidator<>) &&
            d.ImplementationType?.Name == "FluentValidationMessageValidatorAdapter`1").Should().Be(1);
    }

    [Fact]
    public void UseFluentValidation_WithoutAssemblies_RegistersAdapterOnly()
    {
        var services = new ServiceCollection();

        services.AddTrellis(options => options.UseFluentValidation());

        services.Count(d =>
            d.ServiceType == typeof(IMessageValidator<>) &&
            d.ImplementationType?.Name == "FluentValidationMessageValidatorAdapter`1").Should().Be(1);
    }

    [Fact]
    public void UseResourceAuthorization_WithoutAssemblies_RegistersMediatorOnly()
    {
        var services = new ServiceCollection();

        services.AddTrellis(options => options.UseResourceAuthorization());

        services.Should().Contain(d =>
            d.ServiceType == typeof(IPipelineBehavior<,>) &&
            d.ImplementationType == typeof(AuthorizationBehavior<,>));
        services.Should().NotContain(d =>
            d.ServiceType == typeof(IPipelineBehavior<UpdateProtectedOrderCommand, Result<string>>));
        services.Should().NotContain(d =>
            d.ServiceType == typeof(IResourceLoader<UpdateProtectedOrderCommand, ProtectedOrder>));
    }

    [Fact]
    public void UseResourceAuthorization_WithAssembly_RegistersResourceAuthorizationForDiscoveredMessages()
    {
        var services = new ServiceCollection();

        services.AddTrellis(options => options.UseResourceAuthorization(typeof(UpdateProtectedOrderCommand).Assembly));

        services.Should().Contain(d =>
            d.ServiceType == typeof(IPipelineBehavior<,>) &&
            d.ImplementationType == typeof(AuthorizationBehavior<,>));
        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IPipelineBehavior<UpdateProtectedOrderCommand, Result<string>>));
        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IResourceLoader<UpdateProtectedOrderCommand, ProtectedOrder>) &&
            d.ImplementationType == typeof(UpdateProtectedOrderLoader));
    }

    [Fact]
    public void UseResourceAuthorization_NullAssemblyArray_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        var act = () => services.AddTrellis(options => options.UseResourceAuthorization(null!));

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("assemblies");
    }

    [Fact]
    public void UseResourceAuthorization_NullAssemblyElement_ThrowsArgumentException()
    {
        var services = new ServiceCollection();

        var act = () => services.AddTrellis(options => options.UseResourceAuthorization(
            typeof(UpdateProtectedOrderCommand).Assembly,
            null!));

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "assemblies")
            .And.Message.Should().Contain("[1]");
    }

    [Fact]
    public void UseAsp_RegistersTrellisAspOptionsAndScalarValidationInfrastructure()
    {
        var services = new ServiceCollection();

        services.AddTrellis(options => options.UseAsp());

        services.Should().ContainSingle(d => d.ServiceType == typeof(TrellisAspOptions));
    }

    [Fact]
    public void MultipleActorProviders_Throws()
    {
        var services = new ServiceCollection();

        var act = () => services.AddTrellis(options => options
            .UseClaimsActorProvider()
            .UseEntraActorProvider());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only one actor provider*");
    }

    [Fact]
    public void SameActorProviderConfiguredTwice_Throws()
    {
        var services = new ServiceCollection();

        var act = () => services.AddTrellis(options => options
            .UseClaimsActorProvider()
            .UseClaimsActorProvider());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only one actor provider*");
    }

    [Fact]
    public void UseClaimsActorProvider_RegistersActorProvider()
    {
        var services = new ServiceCollection();

        services.AddTrellis(options => options.UseClaimsActorProvider());

        services.Count(d =>
            d.ServiceType == typeof(IActorProvider) &&
            d.ImplementationType?.Name == "ClaimsActorProvider").Should().Be(1);
    }

    [Fact]
    public void UseProblemDetails_RegistersTrellisProblemDetailsCustomization()
    {
        // Run the registered CustomizeProblemDetails delegate and assert it carries the
        // Trellis defaults (traceId, 405 Allow projection). Resolving through
        // IOptions<ProblemDetailsOptions> proves the full PostConfigure chain is wired,
        // not just that the boolean was set.
        var services = new ServiceCollection();
        services.AddTrellis(options => options.UseProblemDetails());

        using var sp = services.BuildServiceProvider();
        var customize = sp.GetRequiredService<IOptions<ProblemDetailsOptions>>().Value.CustomizeProblemDetails;
        customize.Should().NotBeNull();

        var http = new DefaultHttpContext();
        http.Response.Headers["Allow"] = "GET, POST";
        var ctx = new ProblemDetailsContext
        {
            HttpContext = http,
            ProblemDetails = new ProblemDetails { Status = StatusCodes.Status405MethodNotAllowed },
        };
        customize!.Invoke(ctx);

        ctx.ProblemDetails.Extensions["traceId"].Should().NotBeNull();
        string[] expected = ["GET", "POST"];
        ctx.ProblemDetails.Extensions["allow"].Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void UseProblemDetails_DoesNotImplyUseAsp()
    {
        // ProblemDetails customization is orthogonal to Trellis.Asp's MVC/result-mapping
        // infrastructure. Consumers should be able to opt into ProblemDetails without
        // pulling in TrellisAspOptions / scalar validation / response mapping.
        var services = new ServiceCollection();
        services.AddTrellis(options => options.UseProblemDetails());

        services.Should().NotContain(d => d.ServiceType == typeof(TrellisAspOptions));
    }

    [Fact]
    public void UseProblemDetails_MixedWithDirectAddCallStaysSingleLayer()
    {
        // The direct AddTrellisProblemDetails() and the builder slot share the same
        // sentinel-based idempotency. A consumer that calls both (shared library +
        // application composition root) must end up with exactly one Trellis
        // post-configure layer wrapping CustomizeProblemDetails — not two layers
        // doubling traceId/allow extensions.
        var services = new ServiceCollection();
        services.AddTrellisProblemDetails();
        services.AddTrellis(options => options.UseProblemDetails());

        // The marker sentinel registered by AddTrellisProblemDetails IS the
        // idempotency contract — its presence short-circuits the second call. Count
        // marker registrations directly rather than IPostConfigureOptions descriptors,
        // because the descriptor count is coupled to ASP.NET Core internals (a future
        // AddProblemDetails() release adding its own PostConfigure would break the
        // assertion even though Trellis idempotency is still correct). The marker is
        // private to Trellis.Asp, so match by type name across the assembly boundary.
        var markerRegistrationCount = services.Count(d =>
            string.Equals(d.ServiceType.Name, "TrellisProblemDetailsMarker", StringComparison.Ordinal));

        markerRegistrationCount.Should().Be(1,
            "the marker-sentinel idempotency must apply across builder + direct composition");
    }

    private sealed class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }
    }

    private sealed class SecondaryDbContext : DbContext
    {
        public SecondaryDbContext(DbContextOptions<SecondaryDbContext> options)
            : base(options)
        {
        }
    }

    public sealed record ProtectedOrder(string Id, string OwnerId);

    public sealed record UpdateProtectedOrderCommand(string ResourceId)
        : ICommand<Result<string>>, IAuthorizeResource<ProtectedOrder>
    {
        public IResult Authorize(Actor actor, ProtectedOrder resource) =>
            actor.Id == resource.OwnerId
                ? Result.Ok()
                : Result.Fail(new Error.Forbidden("protected-order.owner") { Detail = "Only the owner can update the order." });
    }

    public sealed class UpdateProtectedOrderLoader : IResourceLoader<UpdateProtectedOrderCommand, ProtectedOrder>
    {
        public Task<Result<ProtectedOrder>> LoadAsync(UpdateProtectedOrderCommand message, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Ok(new ProtectedOrder(message.ResourceId, "owner-1")));
    }

    public sealed record SampleEvent(DateTimeOffset OccurredAt) : IDomainEvent;

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Naming", "CA1711:Identifiers should not have incorrect suffix",
        Justification = "Domain event handler is a DDD term of art and is unrelated to System.EventHandler.")]
    public sealed class SampleEventHandler : IDomainEventHandler<SampleEvent>
    {
        public ValueTask HandleAsync(SampleEvent domainEvent, CancellationToken cancellationToken) => ValueTask.CompletedTask;
    }

    [Fact]
    public void UseDomainEvents_WithoutAssemblies_RegistersDispatchBehaviorAndPublisher()
    {
        var services = new ServiceCollection();

        services.AddTrellis(options => options.UseDomainEvents());

        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IDomainEventPublisher));
        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IPipelineBehavior<,>) &&
            d.ImplementationType == typeof(DomainEventDispatchBehavior<,>));
    }

    [Fact]
    public void UseDomainEvents_WithAssembly_RegistersDiscoveredHandlers()
    {
        var services = new ServiceCollection();

        services.AddTrellis(options => options.UseDomainEvents(typeof(SampleEventHandler).Assembly));

        services.Should().Contain(d =>
            d.ServiceType == typeof(IDomainEventHandler<SampleEvent>) &&
            d.ImplementationType == typeof(SampleEventHandler));
    }

    [Fact]
    public void UseDomainEvents_WithUnitOfWork_PlacesDispatchBeforeTransactional()
    {
        var services = new ServiceCollection();

        services.AddTrellis(options => options
            .UseDomainEvents()
            .UseEntityFrameworkUnitOfWork<TestDbContext>());

        var pipeline = services
            .Where(d => d.ServiceType == typeof(IPipelineBehavior<,>))
            .Select(d => d.ImplementationType)
            .ToList();

        var dispatchIndex = pipeline.IndexOf(typeof(DomainEventDispatchBehavior<,>));
        var txIndex = pipeline.IndexOf(typeof(TransactionalCommandBehavior<,>));

        dispatchIndex.Should().BeGreaterOrEqualTo(0);
        txIndex.Should().BeGreaterOrEqualTo(0);
        dispatchIndex.Should().BeLessThan(txIndex,
            "domain events must dispatch after the transaction commits");
        pipeline.Should().EndWith(typeof(TransactionalCommandBehavior<,>));
    }

    [Fact]
    public void UseTrackedAggregateDomainEvents_WithoutAssemblies_RegistersTrackedBehaviorAndPublisher()
    {
        var services = new ServiceCollection();

        services.AddTrellis(options => options.UseTrackedAggregateDomainEvents());

        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IDomainEventPublisher));
        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IPipelineBehavior<,>) &&
            d.ImplementationType == typeof(TrackedAggregateDomainEventDispatchBehavior<,>));
        services.Should().NotContain(d =>
            d.ServiceType == typeof(IPipelineBehavior<,>) &&
            d.ImplementationType == typeof(DomainEventDispatchBehavior<,>));
    }

    [Fact]
    public void UseTrackedAggregateDomainEvents_WithAssembly_RegistersDiscoveredHandlers()
    {
        var services = new ServiceCollection();

        services.AddTrellis(options => options.UseTrackedAggregateDomainEvents(typeof(SampleEventHandler).Assembly));

        services.Should().Contain(d =>
            d.ServiceType == typeof(IDomainEventHandler<SampleEvent>) &&
            d.ImplementationType == typeof(SampleEventHandler));
        // Handler-scan path uses AddDomainEventDispatch internally; the registration helper
        // detects the tracked behavior is already present and skips the response-shape append.
        services.Should().NotContain(d =>
            d.ServiceType == typeof(IPipelineBehavior<,>) &&
            d.ImplementationType == typeof(DomainEventDispatchBehavior<,>));
    }

    [Fact]
    public void UseTrackedAggregateDomainEvents_WithUnitOfWork_PlacesTrackedDispatchBeforeTransactional()
    {
        var services = new ServiceCollection();

        services.AddTrellis(options => options
            .UseTrackedAggregateDomainEvents()
            .UseEntityFrameworkUnitOfWork<TestDbContext>());

        var pipeline = services
            .Where(d => d.ServiceType == typeof(IPipelineBehavior<,>))
            .Select(d => d.ImplementationType)
            .ToList();

        var trackedIndex = pipeline.IndexOf(typeof(TrackedAggregateDomainEventDispatchBehavior<,>));
        var txIndex = pipeline.IndexOf(typeof(TransactionalCommandBehavior<,>));

        trackedIndex.Should().BeGreaterOrEqualTo(0);
        txIndex.Should().BeGreaterOrEqualTo(0);
        trackedIndex.Should().BeLessThan(txIndex,
            "tracked-aggregate dispatch must run after the transaction commits");
        pipeline.Should().EndWith(typeof(TransactionalCommandBehavior<,>));
    }

    [Fact]
    public void UseTrackedAggregateDomainEvents_WithUnitOfWorkRegisteredBefore_PlacesTrackedDispatchBeforeTransactional()
    {
        // Inverse ordering: TX registered before tracked dispatch on the builder. The opt-in
        // must yank TX, append always-on behaviors, append tracked dispatch, and re-append
        // TX so the final order is canonical regardless of call order.
        var services = new ServiceCollection();

        services.AddTrellis(options => options
            .UseEntityFrameworkUnitOfWork<TestDbContext>()
            .UseTrackedAggregateDomainEvents());

        var pipeline = services
            .Where(d => d.ServiceType == typeof(IPipelineBehavior<,>))
            .Select(d => d.ImplementationType)
            .ToList();

        var trackedIndex = pipeline.IndexOf(typeof(TrackedAggregateDomainEventDispatchBehavior<,>));
        var txIndex = pipeline.IndexOf(typeof(TransactionalCommandBehavior<,>));

        trackedIndex.Should().BeGreaterOrEqualTo(0);
        txIndex.Should().BeGreaterOrEqualTo(0);
        trackedIndex.Should().BeLessThan(txIndex);
        pipeline.Should().EndWith(typeof(TransactionalCommandBehavior<,>));
    }

    [Fact]
    public void UseDomainEvents_AfterUseTrackedAggregateDomainEvents_Throws()
    {
        // Mutex: picking both dispatchers would double-dispatch for Result<TAggregate>
        // handlers. The builder enforces fail-fast misconfiguration just like the actor-provider
        // and unit-of-work slots do.
        var services = new ServiceCollection();

        var act = () => services.AddTrellis(options => options
            .UseTrackedAggregateDomainEvents()
            .UseDomainEvents());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*mutually exclusive*");
    }

    [Fact]
    public void UseTrackedAggregateDomainEvents_AfterUseDomainEvents_Throws()
    {
        var services = new ServiceCollection();

        var act = () => services.AddTrellis(options => options
            .UseDomainEvents()
            .UseTrackedAggregateDomainEvents());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*mutually exclusive*");
    }

    // -------- Round-N inspection findings (M-S1, N-S1, N-S4) --------

    [Fact]
    public void UseEntityFrameworkUnitOfWork_TwiceWithSameContext_Throws()
    {
        // Inspection finding M-S1: the actor-provider slot throws on duplicate
        // configuration to prevent silent misconfiguration. The UoW slot must
        // follow the same fail-fast policy: a user mistakenly chaining two
        // UseEntityFrameworkUnitOfWork calls is always misconfigured.
        var services = new ServiceCollection();

        var act = () => services.AddTrellis(options => options
            .UseEntityFrameworkUnitOfWork<TestDbContext>()
            .UseEntityFrameworkUnitOfWork<TestDbContext>());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unit of work*");
    }

    [Fact]
    public void UseEntityFrameworkUnitOfWork_TwiceWithDifferentContext_Throws()
    {
        // Inspection finding M-S1: chaining UseEntityFrameworkUnitOfWork<DbContextA>
        // then UseEntityFrameworkUnitOfWork<DbContextB> previously silently
        // overwrote the first registration so only DbContextB's UoW was wired.
        // That class of mistake (read/write split, multi-tenant) must fail fast.
        var services = new ServiceCollection();

        var act = () => services.AddTrellis(options => options
            .UseEntityFrameworkUnitOfWork<TestDbContext>()
            .UseEntityFrameworkUnitOfWork<SecondaryDbContext>());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unit of work*");
    }

    [Fact]
    public void ExplicitResourceAuthorization_BeforeAddTrellis_PositionsBehaviorBeforeValidation()
    {
        // Inspection finding N-S1: the documented "explicit resource-authorization
        // registrations without scanning" use case (UseResourceAuthorization() with
        // no assemblies) requires the user to call AddResourceAuthorization<T,R,Resp>()
        // explicitly. If they do so BEFORE AddTrellis(...), the closed-generic
        // ResourceAuthorizationBehavior<,,> previously ended up at descriptor slot 0,
        // before exception/tracing/logging/static-auth/validation — outside the
        // canonical Trellis behavior envelope. AddTrellisBehaviors now re-positions
        // any pre-existing closed-generic resource-auth behaviors to sit just before
        // ValidationBehavior, mirroring the AddTrellisUnitOfWork ↔ AddDomainEventDispatch
        // symmetry.
        var services = new ServiceCollection();

        services.AddResourceAuthorization<UpdateProtectedOrderCommand, ProtectedOrder, Result<string>>();
        services.AddScoped<IResourceLoader<UpdateProtectedOrderCommand, ProtectedOrder>, UpdateProtectedOrderLoader>();
        services.AddTrellis(options => options.UseResourceAuthorization());

        var descriptors = services
            .Where(d => d.ServiceType == typeof(IPipelineBehavior<,>)
                     || d.ServiceType == typeof(IPipelineBehavior<UpdateProtectedOrderCommand, Result<string>>))
            .ToList();

        var validationIndex = descriptors.FindIndex(d =>
            d.ServiceType == typeof(IPipelineBehavior<,>) &&
            d.ImplementationType == typeof(ValidationBehavior<,>));
        var resAuthIndex = descriptors.FindIndex(d =>
            d.ServiceType == typeof(IPipelineBehavior<UpdateProtectedOrderCommand, Result<string>>));

        validationIndex.Should().BeGreaterOrEqualTo(0, "ValidationBehavior must be registered by AddTrellisBehaviors");
        resAuthIndex.Should().BeGreaterOrEqualTo(0, "explicit AddResourceAuthorization must remain registered");
        resAuthIndex.Should().Be(validationIndex - 1,
            "ResourceAuthorizationBehavior<,,> must sit immediately before ValidationBehavior in the canonical pipeline");
    }

    [Fact]
    public void UseCachingActorProvider_AfterUseClaimsActorProvider_WrapsInnerProvider()
    {
        // Inspection finding N-S4: Trellis.Asp exposes AddCachingActorProvider<T>()
        // for per-request caching of an inner IActorProvider, but the builder didn't
        // expose a slot for it. Calling AddCachingActorProvider<ClaimsActorProvider>()
        // after AddTrellis(...UseClaimsActorProvider()) works but is awkward; making
        // it a builder slot makes the composition explicit and prevents the user
        // from forgetting the order constraint.
        var services = new ServiceCollection();

        services.AddTrellis(options => options
            .UseClaimsActorProvider()
            .UseCachingActorProvider<ClaimsActorProvider>());

        // The IActorProvider resolves to a delegate registration (factory-based
        // CachingActorProvider) — assert by descriptor shape that the slot is
        // factory-based rather than the bare ClaimsActorProvider implementation
        // type registered by UseClaimsActorProvider alone.
        services.Should().Contain(d =>
            d.ServiceType == typeof(IActorProvider) &&
            d.ImplementationFactory != null,
            "UseCachingActorProvider must replace the IActorProvider slot with a CachingActorProvider factory");
        services.Should().Contain(d =>
            d.ServiceType == typeof(ClaimsActorProvider),
            "the inner provider type must be registered as scoped so the caching wrapper can resolve it");
    }

    [Fact]
    public void UseCachingActorProvider_TwiceWithDifferentInner_Throws()
    {
        // Inspection finding N-S4: the caching slot must follow the same fail-fast
        // duplicate-detection pattern as the actor-provider slot itself.
        var services = new ServiceCollection();

        var act = () => services.AddTrellis(options => options
            .UseClaimsActorProvider()
            .UseCachingActorProvider<ClaimsActorProvider>()
            .UseCachingActorProvider<EntraActorProvider>());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*caching actor provider*");
    }
}