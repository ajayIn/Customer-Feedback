using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Domain;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
}
