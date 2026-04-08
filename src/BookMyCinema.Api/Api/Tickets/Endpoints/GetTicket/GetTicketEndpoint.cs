using BookMyCinema.Api.Api.Abstractions;
using BookMyCinema.Api.Common.Results;
using BookMyCinema.Application.Common.Results;
using BookMyCinema.Domain.User;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookMyCinema.Api.Api.Tickets.Endpoints.GetTicket;
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
        Result<string> result = UserErrors.EmailTaken;
        Console.WriteLine($"value: {result.Value}"); //null

        return result.Match(
            value => Results.Ok(value)); // won't hit
    }
}
