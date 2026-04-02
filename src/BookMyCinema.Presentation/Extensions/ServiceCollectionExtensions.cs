using Microsoft.Extensions.DependencyInjection;

namespace BookMyCinema.Presentation.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers();
              
        services.AddOpenApi();

        return services;
    }
}
