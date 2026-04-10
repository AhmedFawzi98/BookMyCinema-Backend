using BookMyCinema.Api;
using BookMyCinema.App;
using BookMyCinema.Application;
using BookMyCinema.Infrastructure;
using BookMyCinema.Persistance;
using BookMyCinema.WebApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddWeb()
    .AddPresentation()
    .AddApplication()
    .AddPersistance()
    .AddInfrastructure();

builder.Host.AddSerilog();

var app = builder.Build();

app.ConfigureWebApplication();
app.Run();
