using BookMyCinema.SharedKernel.Events;

namespace BookMyCinema.SharedKernel;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull, IEquatable<TId>
{
    //future possible TODO: A Version property for optimistic concurrency (whether in separate interface or on IAggregateRoot) 
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot()
    {
    }
    protected AggregateRoot(TId id) : base(id)
    {
    }

    public IReadOnlyList<IDomainEvent> DomainEvents =>
      _domainEvents.AsReadOnly();

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

}
