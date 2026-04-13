namespace BookMyCinema.Api.Common.Logging;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
public sealed class HttpLoggingAttribute : Attribute
{
    public HttpLoggingOptions Options { get; }

    public HttpLoggingAttribute(HttpLoggingOptions options)
    {
        Options = options;
    }

    public bool LogsRequest => Options.HasFlag(HttpLoggingOptions.Request);
    public bool LogsResponse => Options.HasFlag(HttpLoggingOptions.Response);
    public bool LogsRequestBody => Options.HasFlag(HttpLoggingOptions.RequestBody);
    public bool LogsResponseBody => Options.HasFlag(HttpLoggingOptions.ResponseBody);
}
