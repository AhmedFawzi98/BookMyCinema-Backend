using Serilog;

namespace BookMyCinema.WebApp;
public static class HostBuilderExtensions
{
    public static IHostBuilder AddSerilog(this IHostBuilder host)
    {
        return host.UseSerilog((context, services, config) =>
            config
                .ReadFrom.Configuration(context.Configuration));
    }
}
