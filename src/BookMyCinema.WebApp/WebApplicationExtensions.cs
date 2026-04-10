using BookMyCinema.Api.Api;
using BookMyCinema.Api.Api.Abstractions;

namespace BookMyCinema.App;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureWebApplication(this WebApplication app)
    {
        app.UseExceptionHandler(_ => { });
        app.MapOpenApi();
        app.UseHttpsRedirection();

        app.MapEndpoints();


        return app;
    }

    private static WebApplication MapEndpoints(this WebApplication app)
    {
        var baseGroupBuilder = app.MapGroup(ApiRoutes.ApiBase)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(baseGroupBuilder);
        }

        return app;
    }
}
