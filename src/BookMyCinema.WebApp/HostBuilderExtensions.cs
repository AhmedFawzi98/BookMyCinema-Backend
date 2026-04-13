using System.Data;
using BookMyCinema.Api.Common.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace BookMyCinema.WebApp;
public static class HostBuilderExtensions
{
    public static IHostBuilder AddSerilog(this IHostBuilder host)
    {
        host.UseSerilog((context, services, config) =>
        {
            config
               .ReadFrom.Configuration(context.Configuration)
                // Logs table (Application logs with Level Errors only)
                .WriteTo.Logger(loggerConfig =>
                {
                    loggerConfig
                        .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error &&
                            !e.Properties.ContainsKey(HttpLoggingConstants.IsHttpLog))
                        .WriteTo.MSSqlServer(
                            context.Configuration.GetConnectionString("DefaultConnection"),
                            sinkOptions: new MSSqlServerSinkOptions
                            {
                                TableName = "Logs",
                                SchemaName = "Logging",
                                AutoCreateSqlTable = true
                            }
                        );
                })
                // seperate HttpLogs table with custom schema (Request/Response logs for all levels)
                .WriteTo.Logger(loggerConfig =>
                {
                    loggerConfig
                        .Filter.ByIncludingOnly(e => e.Properties.ContainsKey(HttpLoggingConstants.IsHttpLog)) //request/response logs will go into HttpLogs table regardless of the level
                        .WriteTo.MSSqlServer(
                            context.Configuration.GetConnectionString("DefaultConnection"),
                            sinkOptions: new MSSqlServerSinkOptions
                            {
                                TableName = "HttpLogs",
                                SchemaName = "Logging",
                                AutoCreateSqlTable = true
                            },
                            columnOptions: CreateColumnsOptionsForHttpLogsTable()
                        );
                });
        });

        return host;
    }

    private static ColumnOptions CreateColumnsOptionsForHttpLogsTable()
    {
        var columnOptions = new ColumnOptions();

        // Remove XML properties column .. leave rest of columns (Id, Message, MessageTemplate, Level, TimeStamp, Exception)
        columnOptions.Store.Remove(StandardColumn.Properties);

        columnOptions.AdditionalColumns = new List<SqlColumn>
        {
            new SqlColumn(HttpLoggingConstants.Path, SqlDbType.NVarChar, dataLength: 256) { AllowNull = false },
            new SqlColumn(HttpLoggingConstants.Method, SqlDbType.NVarChar, dataLength: 10) { AllowNull = false },
            new SqlColumn(HttpLoggingConstants.StatusCode, SqlDbType.Int) { AllowNull = false },
            new SqlColumn(HttpLoggingConstants.ElapsedMs, SqlDbType.Int) { AllowNull = false },
            new SqlColumn(HttpLoggingConstants.TraceId, SqlDbType.NVarChar, dataLength: 64) { AllowNull = false },
            new SqlColumn(HttpLoggingConstants.UserId, SqlDbType.NVarChar, dataLength: 100) { AllowNull = true },
            new SqlColumn(HttpLoggingConstants.RequestBodyKey, SqlDbType.NVarChar, dataLength: -1) { AllowNull = true },
            new SqlColumn(HttpLoggingConstants.ResponseBodyKey, SqlDbType.NVarChar, dataLength: -1) { AllowNull = true }
        };

        return columnOptions;
    }
}
