using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Formatting.Compact;

namespace CustomerFeedback.BuildingBlocks.Logging;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", context.HostingEnvironment.ApplicationName)
                .WriteTo.Console(new CompactJsonFormatter());
        });

        return builder;
    }
}
