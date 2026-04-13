using System.Diagnostics;
using BookMyCinema.Api.Api;
using BookMyCinema.Api.Api.Abstractions;
using BookMyCinema.Api.Common.Logging;
using Serilog;
using Serilog.Events;
using HttpLoggingAttribute = BookMyCinema.Api.Common.Logging.HttpLoggingAttribute;
using HttpLoggingOptions = BookMyCinema.Api.Common.Logging.HttpLoggingOptions;
namespace BookMyCinema.App;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureWebApplication(this WebApplication app)
    {
        app.UseExceptionHandler(_ => { });
        app.MapOpenApi();
        app.UseHttpsRedirection();

        app.MapEndpoints();

        app.ConfigureLogging();

        return app;
    }

    private static WebApplication MapEndpoints(this WebApplication app)
    {
        var baseGroupBuilder = app.MapGroup(ApiRoutes.ApiBase)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(baseGroupBuilder);
        }

        return app;
    }

    private static WebApplication ConfigureLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                var endpoint = httpContext.GetEndpoint();
                var attr = endpoint?.Metadata.GetMetadata<HttpLoggingAttribute>();

                if (attr is null || attr.Options == HttpLoggingOptions.None)
                {
                    return LogEventLevel.Verbose;
                }

                httpContext.Items[HttpLoggingConstants.ElapsedMs] = elapsed;

                if (ex is not null)
                {
                    return LogEventLevel.Error;
                }

                return httpContext.Response.StatusCode switch
                {
                    >= 500 => LogEventLevel.Error,
                    >= 400 => LogEventLevel.Warning,
                    _ => LogEventLevel.Information
                };
            };

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var endpoint = httpContext.GetEndpoint();
                var attr = endpoint?.Metadata.GetMetadata<HttpLoggingAttribute>();

                if (attr is null || attr.Options == HttpLoggingOptions.None)
                {
                    return;
                }

                diagnosticContext.Set(HttpLoggingConstants.IsHttpLog, true);

                if (attr.LogsRequest)
                {
                    diagnosticContext.Set(HttpLoggingConstants.Method, httpContext.Request.Method);
                    diagnosticContext.Set(HttpLoggingConstants.Path, httpContext.Request.Path);
                    diagnosticContext.Set(HttpLoggingConstants.TraceId, Activity.Current?.TraceId.ToString());

                    var userId = httpContext.User?.Identity?.Name;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        diagnosticContext.Set(HttpLoggingConstants.UserId, userId);
                    }
                }

                if (attr.LogsResponse)
                {
                    diagnosticContext.Set(HttpLoggingConstants.StatusCode, httpContext.Response.StatusCode);

                    if (httpContext.Items.TryGetValue(HttpLoggingConstants.ElapsedMs, out var elapsed))
                    {
                        diagnosticContext.Set(HttpLoggingConstants.ElapsedMs, Convert.ToInt32(elapsed));
                    }
                }

                if (httpContext.Items.TryGetValue(HttpLoggingConstants.RequestBodyKey, out var reqBody))
                {
                    diagnosticContext.Set(HttpLoggingConstants.RequestBodyKey, reqBody?.ToString());
                }

                if (httpContext.Items.TryGetValue(HttpLoggingConstants.ResponseBodyKey, out var resBody))
                {
                    diagnosticContext.Set(HttpLoggingConstants.ResponseBodyKey, resBody?.ToString());
                }
            };
        });

        app.UseMiddleware<HttpRequestResponseBodyLoggingHelperMiddleware>();

        return app;
    }
}
