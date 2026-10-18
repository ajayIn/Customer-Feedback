using Feedback.Application.Abstractions;
using Feedback.Infrastructure.Messaging;
using Feedback.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Feedback.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFeedbackInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue("UseInMemoryDatabase", false);
        services.AddDbContext<FeedbackDbContext>(options =>
        {
            if (useInMemory)
            {
                options.UseInMemoryDatabase("FeedbackTests");
            }
            else
            {
                options.UseSqlServer(configuration.GetConnectionString("Default")
                    ?? throw new InvalidOperationException("ConnectionStrings:Default is required."));
            }
        });

        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<IFeedbackEventPublisher, FeedbackEventPublisher>();

        if (configuration.GetValue("MassTransit:UseInMemory", false) || useInMemory)
        {
            services.AddMassTransit(x =>
            {
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            });
        }
        else
        {
            services.AddMassTransit(x =>
            {
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

    public static async Task EnsureFeedbackDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FeedbackDbContext>();
        await db.Database.EnsureCreatedAsync();
    }
}
