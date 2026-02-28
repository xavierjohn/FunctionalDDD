namespace Trellis.Mediator.Tests;

using Trellis.Mediator.Tests.Helpers;

/// <summary>
/// Tests for <see cref="ResourceAuthorizationBehavior{TMessage, TResponse}"/>.
/// </summary>
public class ResourceAuthorizationBehaviorTests
{
    #region Actor is resource owner

    [Fact]
    public async Task Handle_ActorIsOwner_CallsNextAndReturnsHandlerResult()
    {
        var actorProvider = FakeActorProvider.NoPermissions("owner-1");
        var behavior = new ResourceAuthorizationBehavior<OwnerOnlyCommand, Result<string>>(actorProvider);
        var command = new OwnerOnlyCommand("owner-1", "data");
        var (next, tracker) = NextDelegate.TrackingAsync<OwnerOnlyCommand, Result<string>>(
            Result.Success("Done"));

        var result = await behavior.Handle(command, next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Done");
        tracker.WasInvoked.Should().BeTrue();
    }

    #endregion

    #region Actor is NOT resource owner

    [Fact]
    public async Task Handle_ActorIsNotOwner_DoesNotCallNextAndReturnsForbidden()
    {
        var actorProvider = FakeActorProvider.NoPermissions("other-user");
        var behavior = new ResourceAuthorizationBehavior<OwnerOnlyCommand, Result<string>>(actorProvider);
        var command = new OwnerOnlyCommand("owner-1", "data");
        var (next, tracker) = NextDelegate.TrackingAsync<OwnerOnlyCommand, Result<string>>(
            Result.Success("should not reach"));

        var result = await behavior.Handle(command, next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ForbiddenError>();
        result.Error.Detail.Should().Contain("Not the resource owner");
        tracker.WasInvoked.Should().BeFalse("handler should not be invoked when authorization fails");
    }

    #endregion

    #region Actor with elevated permissions via DualAuthCommand

    [Fact]
    public async Task Handle_ActorIsNotOwnerButHasElevatedPermission_AllowsAccess()
    {
        var actorProvider = FakeActorProvider.WithPermissions("admin-user", "Orders.WriteAny");
        var behavior = new ResourceAuthorizationBehavior<DualAuthCommand, Result<string>>(actorProvider);
        var command = new DualAuthCommand("owner-1");
        var (next, tracker) = NextDelegate.TrackingAsync<DualAuthCommand, Result<string>>(
            Result.Success("Done"));

        var result = await behavior.Handle(command, next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tracker.WasInvoked.Should().BeTrue();
    }

    #endregion

    #region Custom error detail preserved

    [Fact]
    public async Task Handle_AuthorizationFails_PreservesCustomErrorDetail()
    {
        var actorProvider = FakeActorProvider.NoPermissions("other-user");
        var behavior = new ResourceAuthorizationBehavior<DualAuthCommand, Result<string>>(actorProvider);
        var command = new DualAuthCommand("owner-1");
        var (next, tracker) = NextDelegate.TrackingAsync<DualAuthCommand, Result<string>>(
            Result.Success("should not reach"));

        var result = await behavior.Handle(command, next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Detail.Should().Contain("Cannot modify another user's resource");
        tracker.WasInvoked.Should().BeFalse();
    }

    #endregion
}
