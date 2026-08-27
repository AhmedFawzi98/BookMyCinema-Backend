using BookMyCinema.Application.Common.Abstractions;

namespace BookMyCinema.Application.Features.Tickets.GetTicketById;
public class TicketDetails : IDto
{
    public string Title { get; set; } = string.Empty;
    public int Id { get; set; }
}
