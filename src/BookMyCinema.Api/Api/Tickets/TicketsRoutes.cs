namespace BookMyCinema.Api.Api.Tickets;
internal static class TicketsRoutes
{
    internal const string Base = "tickets";
    internal static class GetTicket
    {
        internal const string Route = "{id:guid}";
        internal static string ForId(Guid id) => $"tickets/{id}";
    }
}
