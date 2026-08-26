using BookMyCinema.Api;
using BookMyCinema.WebApp;
using BookMyCinema.Application;
using BookMyCinema.Infrastructure;
using BookMyCinema.Persistance;
using BookMyCinema.Domain;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDomain()
    .AddApplication()
    .AddApi()
    .AddPersistence(builder.Configuration)
    .AddInfrastructure()
    .AddWeb();

builder.Host.AddSerilog();

WebApplication app = builder.Build();

app.ConfigureWebApplication();

try
{
    app.Run();
}
catch
{
    await Log.CloseAndFlushAsync();
}
