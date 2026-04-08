using Microsoft.AspNetCore.Routing;

namespace BookMyCinema.Api.Api.Abstractions;

internal interface IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app);
}
