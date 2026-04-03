using BookMyCinema.Presentation.Endpoints.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookMyCinema.Presentation.Api.Tickets.Endpoints.GetTicket;
internal class GetTicketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = TicketsGrouper.Get(app);

        group.MapGet(TicketsRoutes.GetTicket.Route, GetTicketHandler)
            .WithTags(GetTicketsDocumentation.Tag)
            .WithSummary(GetTicketsDocumentation.Summary)
            .WithDescription(GetTicketsDocumentation.Description);
    }

    public async Task<IResult> GetTicketHandler()
    {
        var ticketDummy = new GetTicketResponse
        {
            Id = 1,
            Name = "Test",
        };

        return Results.Ok(ticketDummy);
    }
}
