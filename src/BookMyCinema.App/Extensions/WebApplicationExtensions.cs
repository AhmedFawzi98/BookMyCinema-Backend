namespace BookMyCinema.App.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureWebApplication(this WebApplication app)
    {
        app.MapOpenApi();
        app.UseHttpsRedirection();
        app.MapControllers();

        return app;
    }
}
