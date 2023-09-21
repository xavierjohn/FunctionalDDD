namespace FunctionalDDD.Domain;

/// <summary>
/// A DDD aggregate is a cluster of domain objects that can be treated as a single unit. 
/// Any references from outside the aggregate should only go to the aggregate root.
/// The root can thus ensure the integrity of the aggregate as a whole.
/// </summary>
/// <typeparam name="TId"></typeparam>
public abstract class Aggregate<TId> : Entity<TId>
    where TId : notnull
{
    protected Aggregate(TId id) : base(id)
    {
    }
}
