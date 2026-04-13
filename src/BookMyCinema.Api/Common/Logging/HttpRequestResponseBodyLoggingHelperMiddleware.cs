using System.Text;
using Microsoft.AspNetCore.Http;

namespace BookMyCinema.Api.Common.Logging;

public sealed class HttpRequestResponseBodyLoggingHelperMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();
        var attr = endpoint?.Metadata.GetMetadata<HttpLoggingAttribute>();

        if (attr is null || attr.Options == HttpLoggingOptions.None)
        {
            await next(context);
            return;
        }

        await HandleAsync(context, attr, next);
    }

    private async Task HandleAsync(HttpContext context, HttpLoggingAttribute attr, RequestDelegate next)
    {
        if (attr.LogsRequestBody)
        {
            await CaptureRequestBodyAsync(context);
        }

        if (attr.LogsResponseBody)
        {
            await CaptureWithResponseBufferingAsync(context, attr, next);
            return;
        }

        await next(context);
    }

    private static async Task CaptureRequestBodyAsync(HttpContext context)
    {
        var request = context.Request;

        if (!request.Body.CanRead || request.ContentLength == 0)
        {
            return;
        }

        request.EnableBuffering();

        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        context.Items[HttpLoggingConstants.RequestBodyKey] = body;
    }

    private async Task CaptureWithResponseBufferingAsync(
        HttpContext context,
        HttpLoggingAttribute attr,
        RequestDelegate next)
    {
        var originalResponseBody = context.Response.Body;
        try
        {
            using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            await next(context);

            if (buffer.Length == 0)
            {
                context.Response.Body = originalResponseBody;
                return;
            }

            // Read response after endpoint writes it
            buffer.Position = 0;
            using var reader = new StreamReader(buffer, Encoding.UTF8, false, leaveOpen: true);
            var responseBody = await reader.ReadToEndAsync();

            context.Items[HttpLoggingConstants.ResponseBodyKey] = responseBody;

            // Copy buffered response back to original stream
            buffer.Position = 0;
            await buffer.CopyToAsync(originalResponseBody);

        }
        finally
        {
            context.Response.Body = originalResponseBody;
        }
    }
}
