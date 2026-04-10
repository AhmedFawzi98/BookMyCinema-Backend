using BookMyCinema.Api.Api.Abstractions;
using BookMyCinema.Api.Common.Results;
using BookMyCinema.Application.Common.Results;
using BookMyCinema.Application.Features.Tickets.Dtos;
using BookMyCinema.Domain.User;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

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

    public static async Task<IResult> GetTicketHandler(ILogger<GetTicketEndpoint> logger)
    {
        Result<string> result = UserErrors.EmailTaken;
        Console.WriteLine($"value: {result.Value}"); //null
        var ticket = new TicketDto() { Id = 1, Title = "some title" };

        logger.LogInformation("this is an info dummy msg for the ticket object: {@Ticket}", ticket);
        logger.LogInformation("this is string property {City}", "London");
        var str = $"{3} ok";

        return result.Match(
            value => Results.Ok(value)); // won't hit
    }
}
