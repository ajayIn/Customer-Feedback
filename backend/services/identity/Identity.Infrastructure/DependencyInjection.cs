using Identity.Application.Abstractions;
using Identity.Infrastructure.Auth;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue("UseInMemoryDatabase", false);
        services.AddDbContext<IdentityDbContext>(options =>
        {
            if (useInMemory)
            {
                options.UseInMemoryDatabase("IdentityTests");
            }
            else
            {
                options.UseSqlServer(configuration.GetConnectionString("Default")
                    ?? throw new InvalidOperationException("ConnectionStrings:Default is required."));
            }
        });

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUserAccounts, UserAccountStore>();
        services.AddScoped<ITokenIssuer, TokenIssuer>();
        return services;
    }

    public static async Task EnsureIdentityDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await db.Database.EnsureCreatedAsync();
    }
}
