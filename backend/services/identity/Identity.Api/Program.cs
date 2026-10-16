using CustomerFeedback.BuildingBlocks.Auth;
using CustomerFeedback.BuildingBlocks.Errors;
using CustomerFeedback.BuildingBlocks.Hosting;
using CustomerFeedback.BuildingBlocks.Logging;
using CustomerFeedback.BuildingBlocks.Validation;
using FluentValidation;
using Identity.Api.Contracts;
using Identity.Api.Data;
using Identity.Api.Domain;
using Identity.Api.Services;
using Identity.Api.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging();

var useInMemory = builder.Configuration.GetValue("UseInMemoryDatabase", false);
builder.Services.AddDbContext<IdentityDbContext>(options =>
{
    if (useInMemory)
    {
        options.UseInMemoryDatabase("IdentityTests");
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is required."));
    }
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddJwtBearerAuth(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
app.UseCustomerFeedbackPipeline();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await db.Database.EnsureCreatedAsync();
}

await IdentitySeeder.SeedAsync(app.Services);

var api = app.MapGroup("/api/identity");

api.MapPost("/register", async (
    RegisterRequest request,
    IValidator<RegisterRequest> validator,
    UserManager<ApplicationUser> users,
    CancellationToken cancellationToken) =>
{
    await RequestValidator.EnsureValidAsync(request, validator, cancellationToken);
    var existing = await users.FindByEmailAsync(request.Email);
    if (existing is not null)
    {
        throw AppException.Conflict("An account with that email already exists.");
    }

    var user = new ApplicationUser
    {
        Id = Guid.NewGuid(),
        Email = request.Email,
        UserName = request.Email,
        DisplayName = request.DisplayName.Trim()
    };
    var created = await users.CreateAsync(user, request.Password);
    if (!created.Succeeded)
    {
        var errors = created.Errors
            .GroupBy(_ => "password")
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
        throw AppException.Validation(errors);
    }

    await users.AddToRoleAsync(user, IdentitySeeder.CustomerRole);
    var roles = await users.GetRolesAsync(user);
    return Results.Created("/api/identity/me", new UserProfile(user.Id, user.Email!, user.DisplayName, roles.ToList()));
});

api.MapPost("/login", async (
    LoginRequest request,
    IValidator<LoginRequest> validator,
    UserManager<ApplicationUser> users,
    TokenService tokens,
    CancellationToken cancellationToken) =>
{
    await RequestValidator.EnsureValidAsync(request, validator, cancellationToken);
    var user = await users.FindByEmailAsync(request.Email);
    if (user is null || !await users.CheckPasswordAsync(user, request.Password))
    {
        throw AppException.Unauthorized("Invalid email or password.");
    }

    return Results.Ok(await tokens.IssueAsync(user, cancellationToken));
});

api.MapPost("/refresh", async (RefreshRequest request, TokenService tokens, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.RefreshToken))
    {
        throw AppException.Unauthorized("Refresh token is required.");
    }

    var user = await tokens.RotateAsync(request.RefreshToken, cancellationToken);
    return Results.Ok(await tokens.IssueAsync(user, cancellationToken));
});

api.MapGet("/me", async (HttpContext http, UserManager<ApplicationUser> users) =>
{
    var sub = http.User.FindFirst("sub")?.Value;
    if (sub is null || !Guid.TryParse(sub, out var userId))
    {
        throw AppException.Unauthorized();
    }

    var user = await users.FindByIdAsync(userId.ToString());
    if (user is null)
    {
        throw AppException.Unauthorized();
    }

    var roles = await users.GetRolesAsync(user);
    return Results.Ok(new UserProfile(user.Id, user.Email!, user.DisplayName, roles.ToList()));
}).RequireAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

public partial class Program;
