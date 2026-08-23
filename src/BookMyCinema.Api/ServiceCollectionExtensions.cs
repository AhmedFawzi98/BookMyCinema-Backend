using BookMyCinema.Api.Api.Extensions;
using BookMyCinema.Api.Common.Errors;
using BookMyCinema.Api.Common.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace BookMyCinema.Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddEndpoints();

        services.AddOpenApi();

        services.AddScoped<HttpBodyCaptureMiddleware>();

        return services;
    }
}
