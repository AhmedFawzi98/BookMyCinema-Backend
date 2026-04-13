using BookMyCinema.Api.Api.Abstractions;
using BookMyCinema.Api.Common.Logging;
using BookMyCinema.Api.Common.Results;
using BookMyCinema.Application.Common.Results;
using BookMyCinema.Application.Features.Tickets.Dtos;
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
            .WithDescription(GetTicketsDocumentation.Description)
            .WithHttpLogging(HttpLoggingOptions.Request | HttpLoggingOptions.Response | HttpLoggingOptions.ResponseBody);
    }

    public static async Task<IResult> GetTicketHandler(ILogger<GetTicketEndpoint> logger)
    {
        Result<TicketDto> result = new TicketDto() { Id = 4, Title = "nice ticket" };

        return result.Match(
            value => Results.Ok(result.Value));
    }
}
