using BookMyCinema.Api;
using BookMyCinema.App;
using BookMyCinema.Application;
using BookMyCinema.Infrastructure.Extensions;
using BookMyCinema.Persistance;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddWeb()
    .AddPresentation()
    .AddApplication()
    .AddPersistance()
    .AddInfrastructure();

var app = builder.Build();

app.ConfigureWebApplication();
app.Run();
