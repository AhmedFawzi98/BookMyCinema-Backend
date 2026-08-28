namespace BookMyCinema.SharedKernel;

public interface IRepository<TAggregate>
    where TAggregate : IAggregateRoot
{
    void Add(TAggregate aggregate);

    void AddRange(IEnumerable<TAggregate> aggregates);
}
