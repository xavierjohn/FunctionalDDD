namespace FunctionalDDD.Core;


public abstract class Aggregate<TId> : Entity<TId>
    where TId : notnull
{
    protected Aggregate(TId id) : base(id)
    {
    }
}
