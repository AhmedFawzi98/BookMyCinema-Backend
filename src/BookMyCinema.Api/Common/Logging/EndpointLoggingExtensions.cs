using Microsoft.AspNetCore.Builder;

namespace BookMyCinema.Api.Common.Logging;
internal static class EndpointLoggingExtensions
{
    public static RouteHandlerBuilder WithHttpLogging(
        this RouteHandlerBuilder builder,
        HttpLoggingOptions options = HttpLoggingOptions.Request | HttpLoggingOptions.Response) //defaults to logging request and response without thier payloads
    {
        return builder.WithMetadata(new HttpLoggingAttribute(options));
    }
}
