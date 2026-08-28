using BookMyCinema.Domain.Common.Audit;
using BookMyCinema.SharedKernel;

namespace BookMyCinema.Domain.Ticket;

public class Ticket : Entity<long>, ICreationAuditable
{
    // Temporary placeholder to use during infrastrucutre setup
    public TicketReference TicketReference { get; private set; }

    public TicketStatus Status { get; private set; }

    public DateTime? UsedAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; set; }

    public int? CreatedByUserId { get; set; }
}
