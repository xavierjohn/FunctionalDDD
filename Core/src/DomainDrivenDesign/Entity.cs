namespace FunctionalDDD.Core;

public abstract class Entity<TId>
{
    public virtual TId Id { get; protected set; }

    protected Entity()
    {
    }

    protected Entity(TId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (!(obj is Entity<TId> other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Id is null || Id.Equals(default(TId)) || other.Id is null || other.Id.Equals(default(TId)))
            return false;

        return Id.Equals(other.Id);
    }

    public static bool operator ==(Entity<TId> a, Entity<TId> b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(Entity<TId> a, Entity<TId> b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return (ToString() + Id).GetHashCode();
    }
}

