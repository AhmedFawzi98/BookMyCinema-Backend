using BookMyCinema.Domain.Common.Results;

namespace BookMyCinema.Application.Common.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken);

}
