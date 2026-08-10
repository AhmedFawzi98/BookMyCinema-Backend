using BookMyCinema.Api;
using BookMyCinema.WebApp;
using BookMyCinema.Application;
using BookMyCinema.Infrastructure;
using BookMyCinema.Persistance;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddWeb()
    .AddApi()
    .AddApplication()
    .AddPersistence(builder.Configuration)
    .AddInfrastructure();

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
