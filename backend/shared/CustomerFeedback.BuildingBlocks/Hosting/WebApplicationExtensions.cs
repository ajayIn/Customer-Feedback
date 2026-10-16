using CustomerFeedback.BuildingBlocks.Errors;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace CustomerFeedback.BuildingBlocks.Hosting;

public static class WebApplicationExtensions
{
    public static WebApplication UseCustomerFeedbackPipeline(this WebApplication app)
    {
        app.UseProblemDetailsExceptionHandler();
        app.UseSerilogRequestLogging();
        return app;
    }
}
