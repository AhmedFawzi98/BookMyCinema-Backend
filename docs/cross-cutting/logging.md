# Logging

BookMyCinema uses Serilog as the main logging provider.

Serilog is configured through `UseSerilog()` on `IHostBuilder`, so it becomes the logging implementation behind Microsoft.Extensions.Logging and replaces the default logging providers.

The project does not keep the default `Logging` section in `appsettings.json`. Serilog does not use that section for its own configuration, so logging settings live under the `Serilog` section instead.

Application code depends on `ILogger<T>` from Microsoft.Extensions.Logging across layers such as Domain, Application, Infrastructure, and API. Serilog stays in the host layer as the concrete provider, so lower layers do not need a custom logger wrapper and do not depend directly on Serilog.

The generic type parameter in `ILogger<T>` provides the `SourceContext` category. That category is used for source identification in log output and supports per-namespace filtering through `Serilog:MinimumLevel:Override`.

Logs should be written as structured events using message templates and named properties, not string-concatenated messages. That keeps log output queryable across console, file, and database destinations.

The logging setup separates normal application logs from structured HTTP request logs:

- `Logging.Logs` stores application error logs.
- `Logging.HttpLogs` stores opted-in HTTP request/response logs.
- console and file sinks are controlled from `appsettings`.

This keeps operational failures and HTTP traffic trace data queryable separately.

## Logging Destinations

The application currently writes logs to three destination types:

- console logging
- file logging
- SQL Server database logging

Console and file logging are configured from `appsettings.json` under `Serilog:WriteTo`.

Both are wrapped with Serilog's `Async` sink. `Async` means log events are placed into an in-memory queue and written by a background worker. It keeps the request/application thread from doing console or file I/O directly.

This is different from database batching. `WriteTo.Async(...)` is about queued background dispatch; batching behavior belongs to the destination sink itself.

Console logging uses a custom output template that includes important diagnostic properties such as level, timestamp, machine name, environment name, trace id, source context, message, and exception.

Log events are also enriched with machine and environment information through Serilog enrichers.

File logging is currently enabled from the base `appsettings.json`, so it applies to every environment unless an environment-specific configuration changes that behavior.

The file sink is configured with:

- daily rolling interval
- `10 MB` file size limit
- rolling when the file size limit is reached
- `60` retained log files
- compact JSON formatting

This makes file logging useful during development and simple hosted environments.

NOTE: In a future production or containerized setup, file logging may be reduced or removed if logs are collected through stdout, or a dedicated observability system instead.

Database logging is configured in code through the SQL Server sink. The SQL Server sink writes using its own batching behavior, so database inserts are not performed as one direct insert per log event.

The SQL Server sink currently uses its default batching behavior:

- batch period: `5 seconds`
- batch posting limit: `50` events

## Application Log Levels

Application log levels are controlled through the `Serilog:MinimumLevel` section in `appsettings.json` and environment-specific overrides such as `appsettings.Development.json`.

The default configuration uses:

- `Serilog:MinimumLevel:Default` for the application-wide minimum level
- `Serilog:MinimumLevel:Override` for noisy framework namespaces such as `Microsoft`, `System`, ASP.NET Core, and Entity Framework
- sink-specific settings such as the file sink `restrictedToMinimumLevel`

For example, the base configuration keeps the default level at `Information`, while development raises application detail to `Debug` and allows more Entity Framework logs.

These settings decide which log events enter the Serilog pipeline and which events reach normal sinks such as console and files.

## HTTP Log Controls

HTTP logging has an additional endpoint-level control.

An endpoint must opt in by attaching `HttpLoggingAttribute`, usually through `WithHttpLogging(...)`.

The supported options are:

- `Request`
- `Response`
- `RequestBody`
- `ResponseBody`
- `None`

The default `WithHttpLogging()` behavior logs request and response metadata without payloads.

Payload capture is explicit. Request and response bodies are only captured when the endpoint includes `RequestBody` or `ResponseBody`.

This keeps HTTP logging intentional per endpoint instead of globally logging every request and every payload.

## Body Capture

`HttpBodyCaptureMiddleware` does not write logs directly.

Its responsibility is to read the configured request and response bodies and place them in `HttpContext.Items`.

Serilog request logging later reads those values through `EnrichDiagnosticContext` and stores them as structured properties on the HTTP log event.

That means body logging requires both parts:

- endpoint metadata must request body capture through `HttpLoggingOptions`
- the body capture middleware must run before the Serilog request logging event is completed

## HTTP Event Levels

For endpoints with HTTP logging enabled, the request log level is derived from the result:

- exceptions are logged as `Error`
- responses with status code `500` and above are logged as `Error`
- responses with status code `400` to `499` are logged as `Warning`
- successful responses are logged as `Information`

Endpoints without `HttpLoggingAttribute`, or with `HttpLoggingOptions.None`, are treated as low-noise HTTP requests and assigned `Verbose`.

With the normal `Information` default minimum level, those unmarked HTTP requests do not reach the Serilog sinks.

## What Reaches The Database

The database sinks are configured in `BookMyCinema.WebApp.HostBuilderExtensions.AddSerilog()`.

`Logging.Logs` receives:

- log events with level `Error` or higher
- only events that are not marked as HTTP logs

This table is intended for application failures and operational errors, not normal request tracing.

`Logging.HttpLogs` receives:

- events marked with the `IsHttpLog` property
- request/response properties selected by the endpoint's `HttpLoggingOptions` using the `HttpLoggingAttribute`
- only events that pass the configured Serilog minimum level

The `IsHttpLog` property is only added for endpoints that opt in through HTTP logging metadata. If an endpoint does not opt in, it does not reach `Logging.HttpLogs`.

HTTP logs are emitted by Serilog request logging middleware. The middleware creates the request log event; persistence batching is handled by the database sink that receives the event.

Because HTTP logs are persisted through the SQL Server sink, they use the same database batching behavior as other database-bound log events.

## What Does Not Reach The Database

The following do not reach `Logging.HttpLogs`:

- endpoints without `HttpLoggingAttribute`
- endpoints using `HttpLoggingOptions.None`
- request bodies unless `RequestBody` is enabled
- response bodies unless `ResponseBody` is enabled
- HTTP events filtered out by the configured Serilog minimum level

The following do not reach `Logging.Logs`:

- application logs with level below `Error`
- HTTP logs marked with `IsHttpLog`

Console and file logging may still receive some of these events depending on `appsettings`, but database persistence is intentionally narrower.

## Stored HTTP Fields

`Logging.HttpLogs` is configured with typed columns (instead of serilog XML column) for the important request data:

- request path
- request method
- request body
- response status code
- response body
- elapsed time
- trace id
- user id when available

The default Serilog `Properties` XML column is removed from `HttpLogs` because the important HTTP data is stored in typed columns.

The SQL logging sink currently provides persistent storage and supports direct investigation when needed. It is not intended to become the long-term primary platform for log querying, aggregation, or dashboards.
As the observability setup evolves, a dedicated logging/observability platform such as Seq or Azure Application Insights may be introduced. At that point, SQL logging can be reduced or removed where it no longer provides sufficient value.

## Shutdown Flushing

The bootstrap code calls `Log.CloseAndFlushAsync()` when startup/runtime execution enters the exception path around `app.Run()`.

This disposes the Serilog logger and gives buffered sinks a chance to flush queued events before the process exits from that path.

If the project needs the same guarantee for normal shutdown paths as well, the flush should be placed in a `finally` block so it runs whether `app.Run()` completes normally or throws.

## Why This Design

The logging setup is designed to keep separate concerns separate:

- appsettings control general logging levels and normal sinks
- endpoint attributes control HTTP logging intent and payload capture
- application logs (failures only) stay in `Logging.Logs`
- request logs stays in `Logging.HttpLogs` controlled by each endpoint metadata. with payload logging by explicit opt in as bodies can be large or sensitive.
