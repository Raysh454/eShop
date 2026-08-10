namespace BuildingBlocks.Domain;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
{
    public TId Id { get; protected set; }

    protected Entity() { }

    protected Entity(TId id)
    {
        Id = id;
    }

    public static bool operator ==(Entity<TId> a, Entity<TId> b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }

    public static bool operator !=(Entity<TId> a, Entity<TId> b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (Entity<TId>)obj;
        return Id.Equals(other.Id);
    }

    public bool Equals(Entity<TId> other)
    {
        if (other == null) return false;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
