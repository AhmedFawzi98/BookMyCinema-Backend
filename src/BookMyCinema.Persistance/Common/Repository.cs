using BookMyCinema.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace BookMyCinema.Persistance.Common;

internal abstract class Repository<TAggregate>(ApplicationDbContext dbContext)
    : IRepository<TAggregate>
    where TAggregate : class, IAggregateRoot
{
    public void Add(TAggregate aggregate)
        => dbContext.Set<TAggregate>().Add(aggregate);

    public void AddRange(IEnumerable<TAggregate> aggregates)
        => dbContext.Set<TAggregate>().AddRange(aggregates);
}
