using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace BookMyCinema.Api.Api.Tickets;
internal static class TicketsGrouper
{
    internal static RouteGroupBuilder Get(IEndpointRouteBuilder endPointRouteBuilder)
    {
        return endPointRouteBuilder.MapGroup(TicketsRoutes.Base);
    }
}
