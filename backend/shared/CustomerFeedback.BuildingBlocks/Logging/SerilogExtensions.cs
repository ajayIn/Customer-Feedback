using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace CustomerFeedback.BuildingBlocks.Logging;

public static class SerilogExtensions
{
    private const string DefaultLogDirectory = "logs";
    private const int DefaultRetainedFileCount = 14;

    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        var bootstrapPath = ResolveLogFilePath(builder.Environment, builder.Configuration, builder.Environment.ApplicationName);
        EnsureLogDirectory(bootstrapPath);

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(new CompactJsonFormatter())
            .WriteTo.File(new CompactJsonFormatter(), bootstrapPath, rollingInterval: RollingInterval.Day, shared: true)
            .CreateBootstrapLogger();

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            var logPath = ResolveLogFilePath(context.HostingEnvironment, context.Configuration, context.HostingEnvironment.ApplicationName);
            EnsureLogDirectory(logPath);
            var retained = context.Configuration.GetValue("Logging:File:RetainedFileCountLimit", DefaultRetainedFileCount);

            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", context.HostingEnvironment.ApplicationName)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .WriteTo.Console(new CompactJsonFormatter())
                .WriteTo.File(
                    new CompactJsonFormatter(),
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: retained,
                    shared: true);
        });

        return builder;
    }

    public static WebApplication UseSerilogRequestContext(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
            {
                var userId = context.User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    await next();
                    return;
                }

                using (LogContext.PushProperty("UserId", userId))
                {
                    await next();
                }
            }
        });

        return app;
    }

    public static WebApplication UseStructuredRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (httpContext, elapsed, exception) =>
            {
                if (exception is not null || httpContext.Response.StatusCode >= 500)
                {
                    return LogEventLevel.Error;
                }

                if (httpContext.Response.StatusCode >= 400)
                {
                    return LogEventLevel.Warning;
                }

                if (httpContext.Request.Path.StartsWithSegments("/health"))
                {
                    return LogEventLevel.Debug;
                }

                return LogEventLevel.Information;
            };
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
                var userId = httpContext.User.FindFirst("sub")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    diagnosticContext.Set("UserId", userId);
                }
            };
        });

        return app;
    }

    public static void RunWithSerilog(this WebApplication app)
    {
        try
        {
            Log.Information("Starting {Service} in {Environment}", app.Environment.ApplicationName, app.Environment.EnvironmentName);
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "{Service} terminated unexpectedly", app.Environment.ApplicationName);
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static string ResolveLogFilePath(IHostEnvironment environment, IConfiguration configuration, string applicationName)
    {
        var directory = configuration["Logging:File:Directory"] ?? DefaultLogDirectory;
        if (Path.IsPathRooted(directory))
        {
            return Path.Combine(directory, $"{applicationName}-.json");
        }

        return Path.Combine(environment.ContentRootPath, directory, $"{applicationName}-.json");
    }

    private static void EnsureLogDirectory(string logFilePath)
    {
        var directory = Path.GetDirectoryName(logFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
