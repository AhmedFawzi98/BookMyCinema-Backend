namespace BookMyCinema.Domain.Ticket;

public interface ITicketRepository
{
    Task<Ticket> GetTicketByTicketReference(TicketReference ticketReference, CancellationToken cancellationToken = default);
}
