using BookMyCinema.Application.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BookMyCinema.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        AddInfrastructureService(services);

        return services;
    }

    private static void AddInfrastructureService(IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DatetimeProvider>();
    }
}
