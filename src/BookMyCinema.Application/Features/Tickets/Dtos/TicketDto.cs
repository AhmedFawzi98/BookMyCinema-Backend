using BookMyCinema.Application.Common.Abstractions;

namespace BookMyCinema.Application.Features.Tickets.Dtos;
public class TicketDto : IDto
{
    public string Title { get; set; } = "ticket title blabla";
    public int Id { get; set; }
}
