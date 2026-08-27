using BookMyCinema.Application.Common.Abstractions.Messaging;

namespace BookMyCinema.Application.Features.Tickets.GetTicketById;

public sealed record GetTickerByIdQuery(int Id) : IQuery<TicketDetails>;
