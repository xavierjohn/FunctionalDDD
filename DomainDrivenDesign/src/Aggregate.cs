namespace FunctionalDdd;

/// <summary>
/// A DDD aggregate is a cluster of domain objects that can be treated as a single unit. 
/// Any references from outside the aggregate should only go to the aggregate root.
/// The root can thus ensure the integrity of the aggregate as a whole.
/// </summary>
/// <typeparam name="TId"></typeparam>
public abstract class Aggregate<TId> : Entity<TId>, IAggregate
    where TId : notnull
{
    protected List<IDomainEvent> DomainEvents { get; } = [];

    /// <summary>
    /// Gets a value indicating whether the aggregate has changed.
    /// </summary>
    /// <value>True if the aggregate has changed; otherwise, false.</value>
    public virtual bool IsChanged => DomainEvents.Count > 0;

    protected Aggregate(TId id) : base(id)
    {
    }

    /// <summary>
    /// Get all domain events that have been recorded.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<IDomainEvent> UncommittedEvents()
        => DomainEvents;

    public void AcceptChanges()
        => DomainEvents.Clear();
}
