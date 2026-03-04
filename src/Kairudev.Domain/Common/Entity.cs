namespace Kairudev.Domain.Common;

public abstract class Entity<TId>
    where TId : notnull
{
    protected Entity(TId id) => Id = id;

    // Parameterless constructor required by EF Core for materialization
    protected Entity() => Id = default!;

    public TId Id { get; private set; } = default!;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
        !(left == right);
}
