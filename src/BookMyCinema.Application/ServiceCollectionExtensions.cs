using System.Reflection;
using BookMyCinema.Application.Common.Abstractions.Messaging;
using BookMyCinema.Application.Common.Logging;
using BookMyCinema.Application.Common.Validations;
using BookMyCinema.Application.User;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BookMyCinema.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        AddFluentValidatation(services);
        AddApplicationServices(services);
        AddApplicationHandlers(services);
        AddApplicationHandlersDecorators(services);

        return services;
    }

    private static void AddApplicationHandlers(IServiceCollection services)
    {
        services.Scan(scan => scan.FromAssembliesOf(typeof(ServiceCollectionExtensions))
           .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
               .AsImplementedInterfaces()
               .WithScopedLifetime()
           .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
               .AsImplementedInterfaces()
               .WithScopedLifetime()
           .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
               .AsImplementedInterfaces()
               .WithScopedLifetime());
    }

    private static void AddApplicationHandlersDecorators(IServiceCollection services)
    {
        //scrutor order of registeration is last registered -> outermost.
        //logging decorated handler -> validation decorated handler -> actual handler
        services.TryDecorate(typeof(ICommandHandler<>), typeof(ValidationDecorator.CommandHandler<>));
        services.TryDecorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator.CommandHandler<,>));

        services.TryDecorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandler<,>));
        services.TryDecorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));
        services.TryDecorate(typeof(ICommandHandler<>), typeof(LoggingDecorator.CommandHandler<>));
    }

    private static void AddFluentValidatation(IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(ServiceCollectionExtensions), includeInternalTypes: true);
        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();
    }

}
