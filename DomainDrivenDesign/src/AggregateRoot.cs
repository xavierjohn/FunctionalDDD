namespace FunctionalDDD.Domain;

/// <summary>
/// A DDD aggregate is a cluster of domain objects that can be treated as a single unit. 
/// An aggregate will have one of its component objects be the aggregate root.Any references from outside the aggregate should only go to the aggregate root.
/// The root can thus ensure the integrity of the aggregate as a whole.
/// <typeparam name="TId"></typeparam>
public abstract class AggregateRoot<TId> : Aggregate<TId>
    where TId : notnull
{
    protected AggregateRoot(TId id) : base(id)
    {
    }
}
