using Microsoft.AspNetCore.Routing;

namespace BookMyCinema.Api.Api.Abstractions;

public interface IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app);
}
