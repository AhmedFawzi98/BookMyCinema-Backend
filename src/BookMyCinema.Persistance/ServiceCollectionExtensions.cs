using Microsoft.Extensions.DependencyInjection;

namespace BookMyCinema.Persistance;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistance(this IServiceCollection services)
    {

        return services;
    }
}
