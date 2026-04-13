using BookMyCinema.Api.Api.Extensions;
using BookMyCinema.Api.Common.Errors;
using BookMyCinema.Api.Common.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace BookMyCinema.Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddEndpoints();

        services.AddOpenApi();

        services.AddScoped<HttpRequestResponseBodyLoggingHelperMiddleware>();

        return services;
    }
}
