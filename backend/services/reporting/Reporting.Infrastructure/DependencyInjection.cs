using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reporting.Application.Abstractions;
using Reporting.Infrastructure.Messaging;
using Reporting.Infrastructure.Persistence;

namespace Reporting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue("UseInMemoryDatabase", false);
        services.AddDbContext<ReportingDbContext>(options =>
        {
            if (useInMemory)
            {
                options.UseInMemoryDatabase("ReportingTests");
            }
            else
            {
                options.UseSqlServer(configuration.GetConnectionString("Default")
                    ?? throw new InvalidOperationException("ConnectionStrings:Default is required."));
            }
        });

        services.AddScoped<IFeedbackFactStore, FeedbackFactStore>();

        if (configuration.GetValue("MassTransit:UseInMemory", false) || useInMemory)
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<FeedbackSubmittedConsumer>();
                x.AddConsumer<FeedbackUpdatedConsumer>();
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            });
        }
        else
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<FeedbackSubmittedConsumer>();
                x.AddConsumer<FeedbackUpdatedConsumer>();
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", h =>
                    {
                        h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                        h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                    });
                    cfg.ConfigureEndpoints(context);
                });
            });
        }

        return services;
    }

    public static async Task EnsureReportingDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
        await db.Database.EnsureCreatedAsync();
    }
}
