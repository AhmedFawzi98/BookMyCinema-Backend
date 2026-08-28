using BookMyCinema.Domain.Ticket;
using BookMyCinema.Persistance.Common;

namespace BookMyCinema.Persistance.Tickets;

internal class TicketRepository(ApplicationDbContext dbContext) : Repository<Ticket>(dbContext), ITicketRepository
{
    //aggregate specific methods
    public async Task<Ticket> GetTicketByTicketReference(TicketReference ticketReference, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
