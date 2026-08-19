using BookMyCinema.SharedKernel.Events;

namespace BookMyCinema.SharedKernel;

public interface IAggregateRoot
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
