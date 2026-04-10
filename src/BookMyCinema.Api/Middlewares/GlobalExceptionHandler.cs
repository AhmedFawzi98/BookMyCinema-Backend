using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BookMyCinema.Api.Middlewares;
internal class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var response = context.Response;
        response.ContentType = "application/problem+json";

        logger.LogError(exception, "Unhandled exception occurred");

        var problemDetails = new ProblemDetails
        {
            Title = "Server error",
            Detail = "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError,
        };

        response.StatusCode = StatusCodes.Status500InternalServerError;
        await WriteResponseAsync(response, problemDetails, cancellationToken);

        return true;
    }


    private static async Task WriteResponseAsync(HttpResponse response, ProblemDetails problemDetails, CancellationToken cancellationToken)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(problemDetails, options);
        await response.WriteAsync(json, cancellationToken);
    }
}
