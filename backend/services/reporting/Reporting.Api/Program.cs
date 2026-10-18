using CustomerFeedback.BuildingBlocks.Auth;
using CustomerFeedback.BuildingBlocks.Hosting;
using CustomerFeedback.BuildingBlocks.Logging;
using Reporting.Application;
using Reporting.Application.UseCases;
using Reporting.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging();
builder.Services.AddReportingApplication();
builder.Services.AddReportingInfrastructure(builder.Configuration);
builder.Services.AddJwtBearerAuth(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StoreManager", policy => policy.RequireRole("StoreManager"));
});

var app = builder.Build();
app.UseCustomerFeedbackPipeline();
app.UseAuthentication();
app.UseAuthorization();
await app.Services.EnsureReportingDatabaseAsync();

app.MapGet("/api/reports/summary", async (
    Guid? storeId,
    DateOnly? from,
    DateOnly? to,
    GetReportSummaryHandler handler,
    CancellationToken cancellationToken) =>
    Results.Ok(await handler.Handle(storeId, from, to, cancellationToken)))
    .RequireAuthorization("StoreManager");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.Run();

public partial class Program;
