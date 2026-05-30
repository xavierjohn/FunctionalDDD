namespace Trellis.Mediator;

/// <summary>
/// Shared constants used by the domain-event dispatch behaviors and the manual
/// <see cref="DomainEventPublisherExtensions.DispatchAggregateEventsAsync"/> helper.
/// </summary>
/// <remarks>
/// Centralising the cap removes the risk of the response-shape dispatcher, the tracked
/// dispatcher, and the manual helper drifting apart.
/// </remarks>
internal static class DomainEventDispatchDefaults
{
    /// <summary>
    /// Maximum number of dispatch waves across all aggregate-dispatch call sites.
    /// </summary>
    /// <remarks>
    /// v1 expects single-wave dispatch; this cap exists to surface accidental re-entry
    /// or runaway event-on-event chains without hanging the pipeline.
    /// </remarks>
    public const int MaxDispatchWaves = 8;
}
