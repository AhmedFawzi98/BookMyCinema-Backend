using BookMyCinema.Api.Api.Extensions;
using BookMyCinema.Api.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace BookMyCinema.Api;

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
