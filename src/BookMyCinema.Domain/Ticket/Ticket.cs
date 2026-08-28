using BookMyCinema.Domain.Common.Audit;
using BookMyCinema.SharedKernel;

namespace BookMyCinema.Domain.Ticket;

public class Ticket : AggregateRoot<long>, ICreationAuditable
{
    // Temporary placeholder to use during infrastrucutre setup
    //assuming ticket is an aggregaate is just a temporary assumption (pre ddd modelling) to build repos infrastructure example
    public TicketReference TicketReference { get; private set; }

    public TicketStatus Status { get; private set; }

    public DateTime? UsedAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; set; }

    public int? CreatedByUserId { get; set; }
}
