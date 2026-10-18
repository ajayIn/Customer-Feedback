using CustomerFeedback.BuildingBlocks.Auth;
using CustomerFeedback.BuildingBlocks.Errors;
using CustomerFeedback.BuildingBlocks.Hosting;
using CustomerFeedback.BuildingBlocks.Logging;
using Identity.Application;
using Identity.Application.Contracts;
using Identity.Application.UseCases;
using Identity.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging();
builder.Services.AddIdentityApplication();
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddJwtBearerAuth(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
app.UseCustomerFeedbackPipeline();
app.UseAuthentication();
app.UseAuthorization();
await app.Services.EnsureIdentityDatabaseAsync();
await IdentitySeeder.SeedAsync(app.Services);

var api = app.MapGroup("/api/identity");

api.MapPost("/register", async (
    RegisterRequest request,
    RegisterUserHandler handler,
    CancellationToken cancellationToken) =>
{
    var profile = await handler.Handle(request, cancellationToken);
    return Results.Created("/api/identity/me", profile);
});

api.MapPost("/login", async (
    LoginRequest request,
    LoginHandler handler,
    CancellationToken cancellationToken) =>
    Results.Ok(await handler.Handle(request, cancellationToken)));

api.MapPost("/refresh", async (
    RefreshRequest request,
    RefreshTokenHandler handler,
    CancellationToken cancellationToken) =>
    Results.Ok(await handler.Handle(request, cancellationToken)));

api.MapGet("/me", async (
    HttpContext http,
    GetCurrentUserHandler handler,
    CancellationToken cancellationToken) =>
{
    var sub = http.User.FindFirst("sub")?.Value;
    if (sub is null || !Guid.TryParse(sub, out var userId))
    {
        throw AppException.Unauthorized();
    }

    return Results.Ok(await handler.Handle(userId, cancellationToken));
}).RequireAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.Run();

public partial class Program;
