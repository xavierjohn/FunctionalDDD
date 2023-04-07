namespace FunctionalDDD;
public abstract class Entity<TId>
    where TId : notnull
{
    public TId Id { get; init; }

    protected Entity(TId id) => Id = id;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Id is null || Id.Equals(default(TId)) || other.Id is null || other.Id.Equals(default(TId)))
            return false;

        return Id.Equals(other.Id);
    }

    public static bool operator ==(Entity<TId>? a, Entity<TId>? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(Entity<TId>? a, Entity<TId>? b) => !(a == b);

    public override int GetHashCode() => HashCode.Combine(this, Id);
}

