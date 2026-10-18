using Feedback.Application.UseCases;
using Feedback.Application.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Feedback.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFeedbackApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateFeedbackRequestValidator>();
        services.AddScoped<CreateFeedbackHandler>();
        services.AddScoped<ListMyFeedbackHandler>();
        services.AddScoped<ListFeedbackHandler>();
        services.AddScoped<UpdateFeedbackCategoryHandler>();
        services.AddScoped<UpdateFeedbackStatusHandler>();
        return services;
    }
}
