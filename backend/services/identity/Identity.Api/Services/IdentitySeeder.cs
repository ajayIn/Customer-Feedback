using Identity.Api.Domain;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Services;

public static class IdentitySeeder
{
    public const string ManagerRole = "StoreManager";
    public const string CustomerRole = "Customer";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        foreach (var role in new[] { CustomerRole, ManagerRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        var email = configuration["Seed:ManagerEmail"];
        var password = configuration["Seed:ManagerPassword"];
        var displayName = configuration["Seed:ManagerDisplayName"] ?? "Store Manager";
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return;
        }

        var manager = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(manager, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(manager, ManagerRole);
    }
}
