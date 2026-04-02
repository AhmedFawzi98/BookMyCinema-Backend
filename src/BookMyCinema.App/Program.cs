using BookMyCinema.App.Extensions;
using BookMyCinema.Infrastructure.Extensions;
using BookMyCinema.Presentation.Extensions;

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
