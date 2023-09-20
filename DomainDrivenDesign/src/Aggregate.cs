namespace FunctionalDDD.Domain;
/// <summary>
/// A DDD aggregate is a cluster of domain objects that can be treated as a single unit. 
/// An aggregate will have one of its component objects be the aggregate root.Any references from outside the aggregate should only go to the aggregate root.
/// The root can thus ensure the integrity of the aggregate as a whole.
/// </summary>
/// <typeparam name="TId"></typeparam>
public abstract class Aggregate<TId>
    where TId : notnull
{
    public TId Id { get; init; }

    protected Aggregate(TId id) => Id = id;

    public override bool Equals(object? obj)
    {
        if (obj is not Aggregate<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Id is null || Id.Equals(default(TId)) || other.Id is null || other.Id.Equals(default(TId)))
            return false;

        return Id.Equals(other.Id);
    }

    public static bool operator ==(Aggregate<TId>? a, Aggregate<TId>? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(Aggregate<TId>? a, Aggregate<TId>? b) => !(a == b);

    public override int GetHashCode() => HashCode.Combine(this, Id);
}

