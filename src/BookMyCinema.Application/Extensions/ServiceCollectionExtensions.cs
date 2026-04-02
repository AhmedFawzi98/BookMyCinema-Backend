using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BookMyCinema.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        AddFluentValidatation(services);

        return services;
    }

    private static void AddFluentValidatation(IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
    }
}