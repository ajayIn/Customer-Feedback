using CustomerFeedback.BuildingBlocks.Auth;
using CustomerFeedback.BuildingBlocks.Errors;
using CustomerFeedback.BuildingBlocks.Hosting;
using CustomerFeedback.BuildingBlocks.Logging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Reporting.Api.Consumers;
using Reporting.Api.Contracts;
using Reporting.Api.Data;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging();

var useInMemory = builder.Configuration.GetValue("UseInMemoryDatabase", false);
builder.Services.AddDbContext<ReportingDbContext>(options =>
{
    if (useInMemory)
    {
        options.UseInMemoryDatabase("ReportingTests");
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is required."));
    }
});

builder.Services.AddJwtBearerAuth(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StoreManager", policy => policy.RequireRole("StoreManager"));
});

if (builder.Configuration.GetValue("MassTransit:UseInMemory", false) || useInMemory)
{
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<FeedbackSubmittedConsumer>();
        x.AddConsumer<FeedbackUpdatedConsumer>();
        x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
    });
}
else
{
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<FeedbackSubmittedConsumer>();
        x.AddConsumer<FeedbackUpdatedConsumer>();
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
            });
            cfg.ConfigureEndpoints(context);
        });
    });
}

var app = builder.Build();
app.UseCustomerFeedbackPipeline();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapGet("/api/reports/summary", async (
    Guid? storeId,
    DateOnly? from,
    DateOnly? to,
    ReportingDbContext db,
    CancellationToken cancellationToken) =>
{
    var query = db.Facts.AsNoTracking().AsQueryable();
    if (storeId is not null)
    {
        query = query.Where(x => x.StoreId == storeId);
    }

    if (from is not null)
    {
        query = query.Where(x => x.VisitDate >= from);
    }

    if (to is not null)
    {
        query = query.Where(x => x.VisitDate <= to);
    }

    var rows = await query.ToListAsync(cancellationToken);
    var total = rows.Count;
    var averageOverall = total == 0 ? 0 : rows.Average(x => x.Overall);
    var averageNps = total == 0 ? 0 : rows.Average(x => x.NpsScore);
    var byCategory = rows
        .GroupBy(x => x.Category)
        .Select(g => new CategoryBreakdown(g.Key, g.Count(), g.Average(x => x.Overall)))
        .OrderBy(x => x.Category)
        .ToList();
    var trend = rows
        .GroupBy(x => x.VisitDate)
        .OrderBy(g => g.Key)
        .Select(g => new TrendPoint(
            g.Key.ToString("yyyy-MM-dd"),
            g.Count(),
            g.Average(x => x.Overall),
            g.Average(x => x.NpsScore)))
        .ToList();
    var statusCounts = new StatusCounts(
        rows.Count(x => x.Status == "New"),
        rows.Count(x => x.Status == "Reviewed"),
        rows.Count(x => x.Status == "Actioned"));

    return Results.Ok(new ReportSummary(total, averageOverall, averageNps, byCategory, trend, statusCounts));
}).RequireAuthorization("StoreManager");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.Run();

public partial class Program;
