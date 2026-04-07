using Microsoft.AspNetCore.Routing;

namespace BookMyCinema.Presentation.Endpoints.Abstractions;

public interface IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app);
}
