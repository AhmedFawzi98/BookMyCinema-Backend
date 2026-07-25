using BookMyCinema.Api;
using BookMyCinema.App;
using BookMyCinema.Application;
using BookMyCinema.Infrastructure;
using BookMyCinema.Persistance;
using BookMyCinema.WebApp;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddWeb()
    .AddPresentation()
    .AddApplication()
    .AddPersistence(builder.Configuration)
    .AddInfrastructure();

builder.Host.AddSerilog();

var app = builder.Build();

app.ConfigureWebApplication();

try
{
    app.Run();
}
catch
{
    await Log.CloseAndFlushAsync();
}
