using BookMyCinema.Application;

namespace BookMyCinema.Api.Common.Logging;

[PubliclyVisible]
[Flags]
public enum HttpLoggingOptions : byte
{
    None = 0,
    Request = 1,
    Response = 2,
    RequestBody = 4,
    ResponseBody = 8
}
