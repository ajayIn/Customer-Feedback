using Identity.Application.Abstractions;
using Identity.Domain;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace Identity.Infrastructure.Auth;

public sealed class UserAccountStore(UserManager<ApplicationUser> users) : IUserAccounts
{
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
        await users.FindByEmailAsync(email) is not null;

    public async Task<CreateCustomerResult> CreateCustomerAsync(string email, string password, string displayName, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            DisplayName = displayName
        };
        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            var errors = created.Errors
                .GroupBy(_ => "password")
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            return new CreateCustomerResult.Invalid(errors);
        }

        await users.AddToRoleAsync(user, Roles.Customer);
        return new CreateCustomerResult.Created(await ToAccountAsync(user));
    }

    public async Task<UserAccount?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null || !await users.CheckPasswordAsync(user, password))
        {
            return null;
        }

        return await ToAccountAsync(user);
    }

    public async Task<UserAccount?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(id.ToString());
        return user is null ? null : await ToAccountAsync(user);
    }

    private async Task<UserAccount> ToAccountAsync(ApplicationUser user)
    {
        var roles = await users.GetRolesAsync(user);
        return new UserAccount(user.Id, user.Email ?? string.Empty, user.DisplayName, roles.ToList());
    }
}
