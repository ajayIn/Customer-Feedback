using CustomerFeedback.BuildingBlocks.Auth;
using CustomerFeedback.BuildingBlocks.Errors;
using CustomerFeedback.BuildingBlocks.Hosting;
using CustomerFeedback.BuildingBlocks.Logging;
using Feedback.Application;
using Feedback.Application.Contracts;
using Feedback.Application.UseCases;
using Feedback.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging();
builder.Services.AddFeedbackApplication();
builder.Services.AddFeedbackInfrastructure(builder.Configuration);
builder.Services.AddJwtBearerAuth(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Customer", policy => policy.RequireRole("Customer"));
    options.AddPolicy("StoreManager", policy => policy.RequireRole("StoreManager"));
});

var app = builder.Build();
app.UseCustomerFeedbackPipeline();
app.UseAuthentication();
app.UseAuthorization();
await app.Services.EnsureFeedbackDatabaseAsync();

var api = app.MapGroup("/api/feedback");

api.MapPost("/", async (
    CreateFeedbackRequest request,
    CreateFeedbackHandler handler,
    HttpContext http,
    CancellationToken cancellationToken) =>
{
    var customerId = RequireUserId(http);
    var dto = await handler.Handle(request, customerId, cancellationToken);
    return Results.Created($"/api/feedback/{dto.Id}", dto);
}).RequireAuthorization("Customer");

api.MapGet("/mine", async (
    int? page,
    int? pageSize,
    ListMyFeedbackHandler handler,
    HttpContext http,
    CancellationToken cancellationToken) =>
{
    var customerId = RequireUserId(http);
    return Results.Ok(await handler.Handle(customerId, page, pageSize, cancellationToken));
}).RequireAuthorization("Customer");

api.MapGet("/", async (
    int? page,
    int? pageSize,
    Guid? storeId,
    string? category,
    string? status,
    DateOnly? from,
    DateOnly? to,
    ListFeedbackHandler handler,
    CancellationToken cancellationToken) =>
    Results.Ok(await handler.Handle(page, pageSize, storeId, category, status, from, to, cancellationToken)))
    .RequireAuthorization("StoreManager");

api.MapPatch("/{id:guid}/category", async (
    Guid id,
    PatchCategoryRequest request,
    UpdateFeedbackCategoryHandler handler,
    CancellationToken cancellationToken) =>
    Results.Ok(await handler.Handle(id, request, cancellationToken)))
    .RequireAuthorization("StoreManager");

api.MapPatch("/{id:guid}/status", async (
    Guid id,
    PatchStatusRequest request,
    UpdateFeedbackStatusHandler handler,
    CancellationToken cancellationToken) =>
    Results.Ok(await handler.Handle(id, request, cancellationToken)))
    .RequireAuthorization("StoreManager");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.Run();

static Guid RequireUserId(HttpContext http)
{
    var sub = http.User.FindFirst("sub")?.Value;
    if (sub is null || !Guid.TryParse(sub, out var userId))
    {
        throw AppException.Unauthorized();
    }

    return userId;
}

public partial class Program;
