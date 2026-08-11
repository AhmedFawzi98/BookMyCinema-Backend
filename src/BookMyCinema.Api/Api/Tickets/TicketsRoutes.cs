namespace BookMyCinema.Api.Api.Tickets;
internal static class TicketsRoutes
{
    internal const string Base = "tickets";
    internal static class GetTicket
    {
        public const string Route = "{id:guid}";
        public static string ForId(Guid id) => $"tickets/{id}"; //not used, example for now
    }
}
