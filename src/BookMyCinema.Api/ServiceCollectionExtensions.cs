using BookMyCinema.Presentation.Api.Extensions;
using BookMyCinema.Presentation.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace BookMyCinema.Presentation;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddEndpoints();

        services.AddOpenApi();

        return services;
    }
}
