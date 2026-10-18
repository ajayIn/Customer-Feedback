using CustomerFeedback.BuildingBlocks.Errors;
using CustomerFeedback.BuildingBlocks.Logging;
using Microsoft.AspNetCore.Builder;

namespace CustomerFeedback.BuildingBlocks.Hosting;

public static class WebApplicationExtensions
{
    public static WebApplication UseCustomerFeedbackPipeline(this WebApplication app)
    {
        app.UseSerilogRequestContext();
        app.UseProblemDetailsExceptionHandler();
        app.UseStructuredRequestLogging();
        return app;
    }
}
