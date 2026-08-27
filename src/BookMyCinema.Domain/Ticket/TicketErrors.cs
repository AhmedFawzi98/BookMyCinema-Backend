using BookMyCinema.Domain.Common.Errors;

namespace BookMyCinema.Domain.Ticket;
public static class TicketErrors
{
    public static readonly Error TicketDummyIdError =
        new("Ticket.Id.DummyWrong", ErrorKind.RuleViolation, "Id is wrong dummy message");
}
