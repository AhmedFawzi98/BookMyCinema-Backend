namespace BookMyCinema.Api.Common.Logging;

[Flags]
public enum HttpLoggingOptions
{
    None = 0,
    Request = 1,
    Response = 2,
    RequestBody = 4,
    ResponseBody = 8
}
