using BookMyCinema.Presentation.Endpoints;
using BookMyCinema.Presentation.Endpoints.Abstractions;

namespace BookMyCinema.App.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureWebApplication(this WebApplication app)
    {
        app.MapOpenApi();
        app.UseHttpsRedirection();

        app.MapEndpoints();

        return app;
    }

    private static WebApplication MapEndpoints(this WebApplication app)
    {
        var baseGroupBuilder = app.MapGroup(ApiRoutes.ApiBase);

        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(baseGroupBuilder);
        }

        return app;
    }
}
