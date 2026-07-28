using BookMyCinema.Application;

namespace BookMyCinema.Api.Common.Logging;

[PubliclyVisible]
public static class HttpLogProperties
{
    public const string IsHttpLog = nameof(IsHttpLog);

    [PubliclyVisible]
    public static class Request
    {
        public const string Path = nameof(Path);
        public const string Method = nameof(Method);
        public const string Body = "RequestBody";
    }

    [PubliclyVisible]
    public static class Response
    {
        public const string StatusCode = nameof(StatusCode);
        public const string Body = "ResponseBody";
    }

    [PubliclyVisible]
    public static class Diagnostics
    {
        public const string ElapsedMs = nameof(ElapsedMs);
        public const string TraceId = nameof(TraceId);
        public const string UserId = nameof(UserId);
    }
}
