using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CustomerFeedback.BuildingBlocks.Errors;

public static class ExceptionHandlingExtensions
{
    public static WebApplication UseProblemDetailsExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var log = context.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("ExceptionHandler");
                var feature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = feature?.Error;
                var traceId = context.TraceIdentifier;

                object body;
                int status;

                switch (exception)
                {
                    case AppException appEx:
                        status = appEx.Status;
                        if (status >= 500)
                        {
                            log?.LogError(appEx, "Application error {Status} {TraceId}", status, traceId);
                        }
                        else
                        {
                            log?.LogWarning("Application error {Status} {TraceId}: {Detail}", status, traceId, appEx.Message);
                        }

                        body = new
                        {
                            type = $"https://httpstatuses.com/{appEx.Status}",
                            title = appEx.Title,
                            status = appEx.Status,
                            detail = appEx.Message,
                            traceId,
                            errors = appEx.Errors
                        };
                        break;
                    case ValidationException fv:
                        log?.LogWarning("Validation failed {TraceId}", traceId);
                        status = 400;
                        var errors = fv.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(g => ToCamel(g.Key), g => g.Select(e => e.ErrorMessage).ToArray());
                        body = new
                        {
                            type = "https://httpstatuses.com/400",
                            title = "Validation failed",
                            status = 400,
                            detail = "One or more fields are invalid.",
                            traceId,
                            errors
                        };
                        break;
                    default:
                        status = 500;
                        log?.LogError(exception, "Unhandled exception {TraceId}", traceId);
                        var detail = app.Environment.IsDevelopment()
                            ? exception?.Message ?? "An error occurred."
                            : "An error occurred.";
                        body = new
                        {
                            type = "https://httpstatuses.com/500",
                            title = "An error occurred",
                            status = 500,
                            detail,
                            traceId
                        };
                        break;
                }

                context.Response.StatusCode = status;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(body);
            });
        });

        return app;
    }

    private static string ToCamel(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
