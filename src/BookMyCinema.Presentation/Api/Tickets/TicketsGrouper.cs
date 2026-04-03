using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace BookMyCinema.Presentation.Api.Tickets;
internal static class TicketsGrouper
{
    public static RouteGroupBuilder Get(IEndpointRouteBuilder endPointRouteBuilder)
    {
        return endPointRouteBuilder.MapGroup(TicketsRoutes.Base);
    }
}
