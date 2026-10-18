using Microsoft.Extensions.DependencyInjection;
using Reporting.Application.UseCases;

namespace Reporting.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingApplication(this IServiceCollection services)
    {
        services.AddScoped<GetReportSummaryHandler>();
        services.AddScoped<ProjectSubmittedFeedbackHandler>();
        services.AddScoped<ProjectUpdatedFeedbackHandler>();
        return services;
    }
}
