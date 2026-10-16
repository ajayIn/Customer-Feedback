using CustomerFeedback.BuildingBlocks.Auth;
using CustomerFeedback.BuildingBlocks.Errors;
using CustomerFeedback.BuildingBlocks.Hosting;
using CustomerFeedback.BuildingBlocks.Logging;
using CustomerFeedback.BuildingBlocks.Messaging;
using CustomerFeedback.BuildingBlocks.Pagination;
using CustomerFeedback.BuildingBlocks.Text;
using CustomerFeedback.BuildingBlocks.Validation;
using Feedback.Api.Contracts;
using Feedback.Api.Data;
using Feedback.Api.Domain;
using Feedback.Api.Mapping;
using Feedback.Api.Validation;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging();

var useInMemory = builder.Configuration.GetValue("UseInMemoryDatabase", false);
builder.Services.AddDbContext<FeedbackDbContext>(options =>
{
    if (useInMemory)
    {
        options.UseInMemoryDatabase("FeedbackTests");
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is required."));
    }
});

builder.Services.AddJwtBearerAuth(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<CreateFeedbackRequestValidator>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Customer", policy => policy.RequireRole("Customer"));
    options.AddPolicy("StoreManager", policy => policy.RequireRole("StoreManager"));
});

if (builder.Configuration.GetValue("MassTransit:UseInMemory", false) || useInMemory)
{
    builder.Services.AddMassTransit(x =>
    {
        x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
    });
}
else
{
    builder.Services.AddMassTransit(x =>
    {
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
    var db = scope.ServiceProvider.GetRequiredService<FeedbackDbContext>();
    await db.Database.EnsureCreatedAsync();
}

var api = app.MapGroup("/api/feedback");

api.MapPost("/", async (
    CreateFeedbackRequest request,
    IValidator<CreateFeedbackRequest> validator,
    HttpContext http,
    FeedbackDbContext db,
    IPublishEndpoint publish,
    CancellationToken cancellationToken) =>
{
    await RequestValidator.EnsureValidAsync(request, validator, cancellationToken);
    var sub = http.User.FindFirst("sub")?.Value;
    if (sub is null || !Guid.TryParse(sub, out var customerId))
    {
        throw AppException.Unauthorized();
    }

    var now = DateTimeOffset.UtcNow;
    var entry = new FeedbackEntry
    {
        Id = Guid.NewGuid(),
        StoreId = request.StoreId,
        CustomerId = request.IsAnonymous ? null : customerId,
        VisitDate = request.VisitDate,
        Overall = request.Overall,
        Cleanliness = request.Cleanliness,
        Staff = request.Staff,
        WaitTime = request.WaitTime,
        ProductAvailability = request.ProductAvailability,
        NpsScore = request.NpsScore,
        Category = request.Category,
        Comments = CommentSanitizer.Sanitize(request.Comments),
        ContactEmail = request.IsAnonymous ? null : request.ContactEmail,
        IsAnonymous = request.IsAnonymous,
        Status = "New",
        CreatedAt = now,
        UpdatedAt = now
    };
    db.Feedback.Add(entry);
    await db.SaveChangesAsync(cancellationToken);

    await publish.Publish(new FeedbackSubmitted(
        entry.Id,
        entry.StoreId,
        entry.CustomerId,
        entry.VisitDate,
        entry.Overall,
        entry.Cleanliness,
        entry.Staff,
        entry.WaitTime,
        entry.ProductAvailability,
        entry.NpsScore,
        entry.Category,
        entry.Status,
        entry.CreatedAt), cancellationToken);

    return Results.Created($"/api/feedback/{entry.Id}", entry.ToDto());
}).RequireAuthorization("Customer");

api.MapGet("/mine", async (
    int? page,
    int? pageSize,
    HttpContext http,
    FeedbackDbContext db,
    CancellationToken cancellationToken) =>
{
    var sub = http.User.FindFirst("sub")?.Value;
    if (sub is null || !Guid.TryParse(sub, out var customerId))
    {
        throw AppException.Unauthorized();
    }

    var (p, size) = Paging.Normalize(page, pageSize);
    var query = db.Feedback.AsNoTracking().Where(x => x.CustomerId == customerId);
    var total = await query.CountAsync(cancellationToken);
    var items = await query.OrderByDescending(x => x.CreatedAt)
        .Skip((p - 1) * size)
        .Take(size)
        .ToListAsync(cancellationToken);
    return Results.Ok(new PagedResult<FeedbackDto>(items.Select(x => x.ToDto()).ToList(), p, size, total));
}).RequireAuthorization("Customer");

api.MapGet("/", async (
    int? page,
    int? pageSize,
    Guid? storeId,
    string? category,
    string? status,
    DateOnly? from,
    DateOnly? to,
    FeedbackDbContext db,
    CancellationToken cancellationToken) =>
{
    var (p, size) = Paging.Normalize(page, pageSize);
    var query = db.Feedback.AsNoTracking().AsQueryable();
    if (storeId is not null)
    {
        query = query.Where(x => x.StoreId == storeId);
    }

    if (!string.IsNullOrWhiteSpace(category))
    {
        query = query.Where(x => x.Category == category);
    }

    if (!string.IsNullOrWhiteSpace(status))
    {
        query = query.Where(x => x.Status == status);
    }

    if (from is not null)
    {
        query = query.Where(x => x.VisitDate >= from);
    }

    if (to is not null)
    {
        query = query.Where(x => x.VisitDate <= to);
    }

    var total = await query.CountAsync(cancellationToken);
    var items = await query.OrderByDescending(x => x.CreatedAt)
        .Skip((p - 1) * size)
        .Take(size)
        .ToListAsync(cancellationToken);
    return Results.Ok(new PagedResult<FeedbackDto>(items.Select(x => x.ToDto()).ToList(), p, size, total));
}).RequireAuthorization("StoreManager");

api.MapPatch("/{id:guid}/category", async (
    Guid id,
    PatchCategoryRequest request,
    FeedbackDbContext db,
    IPublishEndpoint publish,
    CancellationToken cancellationToken) =>
{
    var allowed = new[] { "Service", "Product", "Facilities", "Other" };
    if (!allowed.Contains(request.Category))
    {
        throw AppException.Validation(new Dictionary<string, string[]>
        {
            ["category"] = ["Category must be Service, Product, Facilities, or Other."]
        });
    }

    var entry = await db.Feedback.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
        ?? throw AppException.NotFound("Feedback was not found.");
    entry.Category = request.Category;
    entry.UpdatedAt = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync(cancellationToken);
    await publish.Publish(new FeedbackUpdated(entry.Id, entry.Category, entry.Status, entry.UpdatedAt), cancellationToken);
    return Results.Ok(entry.ToDto());
}).RequireAuthorization("StoreManager");

api.MapPatch("/{id:guid}/status", async (
    Guid id,
    PatchStatusRequest request,
    FeedbackDbContext db,
    IPublishEndpoint publish,
    CancellationToken cancellationToken) =>
{
    var allowed = new[] { "New", "Reviewed", "Actioned" };
    if (!allowed.Contains(request.Status))
    {
        throw AppException.Validation(new Dictionary<string, string[]>
        {
            ["status"] = ["Status must be New, Reviewed, or Actioned."]
        });
    }

    var entry = await db.Feedback.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
        ?? throw AppException.NotFound("Feedback was not found.");
    entry.Status = request.Status;
    entry.UpdatedAt = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync(cancellationToken);
    await publish.Publish(new FeedbackUpdated(entry.Id, entry.Category, entry.Status, entry.UpdatedAt), cancellationToken);
    return Results.Ok(entry.ToDto());
}).RequireAuthorization("StoreManager");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.Run();

public partial class Program;
