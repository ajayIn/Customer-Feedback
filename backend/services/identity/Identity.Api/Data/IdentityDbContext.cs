using Identity.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Data;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TokenHash);
            entity.HasIndex(x => x.UserId);
            entity.Property(x => x.TokenHash).HasMaxLength(128);
        });
    }
}
