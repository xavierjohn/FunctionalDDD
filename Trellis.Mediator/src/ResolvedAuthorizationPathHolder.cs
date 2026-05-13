namespace Trellis.Mediator;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Closed-generic carrier for a <see cref="ResolvedAuthorizationPath"/> registered per
/// via-authorized command. Used so DI naturally disambiguates the path per command (each
/// closed generic is a distinct service type) and so
/// <see cref="ResourceAuthorizationViaBehavior{TMessage, TLeaf, TOwner, TResponse}"/> can be
/// registered as a TYPED descriptor (not a factory descriptor), letting
/// <see cref="ServiceCollectionExtensions"/>'s relocator recognize it by
/// <see cref="ServiceDescriptor.ImplementationType"/> alone.
/// </summary>
/// <typeparam name="TMessage">The command type.</typeparam>
/// <typeparam name="TLeaf">The leaf resource type.</typeparam>
/// <typeparam name="TOwner">The owner resource type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class ResolvedAuthorizationPathHolder<TMessage, TLeaf, TOwner, TResponse>
{
    /// <summary>Creates a new holder for the resolved path.</summary>
    /// <param name="path">The pre-built path. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null.</exception>
    public ResolvedAuthorizationPathHolder(ResolvedAuthorizationPath path)
    {
        ArgumentNullException.ThrowIfNull(path);
        Path = path;
    }

    /// <summary>Gets the resolved authorization path.</summary>
    public ResolvedAuthorizationPath Path { get; }
}
