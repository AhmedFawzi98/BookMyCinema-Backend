using BookMyCinema.Persistance.Constants;
using BookMyCinema.Persistance.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BookMyCinema.Persistance;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence
        (this IServiceCollection services, IConfiguration configuration)
    {
        AddPersistenceOptions(services);
        AddEfCore(services, configuration);

        return services;
    }


    private static void AddPersistenceOptions(IServiceCollection services)
    {
        services
            .AddOptions<EntityFrameworkOptions>()
            .BindConfiguration(EntityFrameworkOptions.SectionName);
    }

    private static void AddEfCore(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
          configuration.GetConnectionString(DatabaseConstants.DefaultConnection)
          ?? throw new InvalidOperationException(
              $"Connection string '{DatabaseConstants.DefaultConnection}' was not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var efOptions = configuration
               .GetRequiredSection(EntityFrameworkOptions.SectionName)
               .Get<EntityFrameworkOptions>()
                    ?? throw new InvalidOperationException(
                        $"Configuration section '{EntityFrameworkOptions.SectionName}' is missing.");

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);

                //todo: handle retry logic for transient failures
            });

            if (efOptions.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }

            if (efOptions.EnableDetailedErrors)
            {
                options.EnableDetailedErrors();
            }
        });
    }
}
