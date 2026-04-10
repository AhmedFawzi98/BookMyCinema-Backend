namespace BookMyCinema.Api.Api.Tickets;
internal static class TicketsRoutes
{
    public const string Base = "tickets";
    public static class GetTicket
    {
        public const string Route = "{id:guid}";
        public static string ForId(Guid id) => $"tickets/{id}";
    }
}
