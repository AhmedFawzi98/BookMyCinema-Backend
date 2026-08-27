using BookMyCinema.Application.Common.Abstractions.Messaging;
using BookMyCinema.Domain.Common.Results;
using BookMyCinema.Domain.Ticket;

namespace BookMyCinema.Application.Features.Tickets.GetTicketById;

internal class GetTicketByIdQueryHandler : IQueryHandler<GetTickerByIdQuery, TicketDetails>
{
    public async Task<Result<TicketDetails>> HandleAsync(GetTickerByIdQuery query, CancellationToken cancellationToken)
    {
        //dummy implmentation
        if(query.Id > 5)
        {
            return TicketErrors.TicketDummyIdError;
        }

        return new TicketDetails()
        {
            Id = query.Id,
            Title = "dummy ticket title"
        };
    }
}
