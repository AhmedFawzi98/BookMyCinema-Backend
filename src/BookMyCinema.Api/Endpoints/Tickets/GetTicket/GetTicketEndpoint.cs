using BookMyCinema.Api.Api.Abstractions;
using BookMyCinema.Api.Common.Logging;
using BookMyCinema.Api.Common.Results;
using BookMyCinema.Application.Common.Abstractions.Messaging;
using BookMyCinema.Application.Features.Tickets.GetTicketById;
using BookMyCinema.Domain.Common.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace BookMyCinema.Api.Api.Tickets.GetTicket;
public class GetTicketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = TicketsGrouper.Get(app);

        group.MapGet(TicketsRoutes.GetTicket.Route, GetTicketHandler)
            .WithTags(GetTicketsDocumentation.Tag)
            .WithSummary(GetTicketsDocumentation.Summary)
            .WithDescription(GetTicketsDocumentation.Description)
            .WithHttpLogging(HttpLoggingOptions.Request | HttpLoggingOptions.Response | HttpLoggingOptions.ResponseBody);
    }

    public static async Task<IResult> GetTicketHandler(
        int id,
        IQueryHandler<GetTickerByIdQuery, TicketDetails> handler,
        CancellationToken cancellationToken)
    {
        //map to query (manual for now)
        var query = new GetTickerByIdQuery(id);

        Result<TicketDetails> result = await handler.HandleAsync(query, cancellationToken);

        return result.Match(
            value => Results.Ok(value));
    }
}
