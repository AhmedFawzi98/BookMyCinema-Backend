using Microsoft.AspNetCore.Routing;

namespace BookMyCinema.Api.Api.Abstractions;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
