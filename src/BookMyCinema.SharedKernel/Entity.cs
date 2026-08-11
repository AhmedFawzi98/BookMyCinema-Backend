namespace BookMyCinema.SharedKernel;

public abstract class Entity<TId>
    where TId : notnull, IEquatable<TId>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity()
    {

    }

    protected Entity(TId id)
    {
        Id = id;
    }

    public TId Id { get; private set; } = default!;


    public IReadOnlyList<IDomainEvent> DomainEvents =>
        _domainEvents.AsReadOnly();

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is not Entity<TId> other)
        {
            return false;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        EqualityComparer<TId> comparer = EqualityComparer<TId>.Default;

        if (comparer.Equals(Id, default!) ||
            comparer.Equals(other.Id, default!))
        {
            return false;
        }

        return comparer.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        if (EqualityComparer<TId>.Default.Equals(Id, default!))
        {
            return base.GetHashCode();
        }

        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null && right is null)
        {
            return true;
        }
        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !(left == right);
}
