using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CustomerFeedback.BuildingBlocks.Auth;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtBearerAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var issuer = configuration["Jwt:Issuer"] ?? "customer-feedback";
        var audience = configuration["Jwt:Audience"] ?? "customer-feedback";
        var signingKey = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is required.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    RoleClaimType = "role",
                    NameClaimType = "sub"
                };
            });

        services.AddAuthorization();
        return services;
    }
}
