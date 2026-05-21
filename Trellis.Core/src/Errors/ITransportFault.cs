namespace Trellis;

/// <summary>
/// Marker interface for transport-layer failure payloads carried by <see cref="Error.TransportFault"/>.
/// Implemented by transport-specific error unions (e.g., HTTP, gRPC, message bus) defined in
/// non-Core packages. Domain code does not inspect implementations; it treats <see cref="Error.TransportFault"/>
/// as opaque lower-layer failure.
/// </summary>
public interface ITransportFault { }