using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Identity.Api.Contracts;
using Identity.Api.Data;
using Identity.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Services;

public sealed class TokenService(
    IConfiguration configuration,
    UserManager<ApplicationUser> userManager,
    IdentityDbContext db)
{
    public async Task<TokenResponse> IssueAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var expiresIn = configuration.GetValue("Jwt:AccessTokenMinutes", 15);
        var signingKey = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is required.");
        var issuer = configuration["Jwt:Issuer"] ?? "customer-feedback";
        var audience = configuration["Jwt:Audience"] ?? "customer-feedback";

        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("email", user.Email ?? string.Empty),
            new("name", user.DisplayName)
        };
        claims.AddRange(roles.Select(role => new Claim("role", role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expiresIn),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshRaw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshHash = Hash(refreshRaw);
        var refreshDays = configuration.GetValue("Jwt:RefreshTokenDays", 7);

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshHash,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshDays)
        });
        await db.SaveChangesAsync(cancellationToken);

        var profile = new UserProfile(user.Id, user.Email ?? string.Empty, user.DisplayName, roles.ToList());
        return new TokenResponse(accessToken, refreshRaw, expiresIn * 60, profile);
    }

    public async Task<ApplicationUser> RotateAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = Hash(refreshToken);
        var stored = await db.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);

        if (stored is null || stored.RevokedAt is not null || stored.ExpiresAt < DateTimeOffset.UtcNow || stored.User is null)
        {
            throw CustomerFeedback.BuildingBlocks.Errors.AppException.Unauthorized("Refresh token is invalid or expired.");
        }

        stored.RevokedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return stored.User;
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
