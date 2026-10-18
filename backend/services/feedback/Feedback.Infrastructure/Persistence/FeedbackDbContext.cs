using Feedback.Domain;
using Microsoft.EntityFrameworkCore;

namespace Feedback.Infrastructure.Persistence;

public sealed class FeedbackDbContext(DbContextOptions<FeedbackDbContext> options) : DbContext(options)
{
    public DbSet<FeedbackEntry> Feedback => Set<FeedbackEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<FeedbackEntry>();
        entity.ToTable("Feedback");
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.StoreId);
        entity.HasIndex(x => x.VisitDate);
        entity.HasIndex(x => x.Category);
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => x.CustomerId);
        entity.Property(x => x.Category).HasMaxLength(32);
        entity.Property(x => x.Status).HasMaxLength(32);
        entity.Property(x => x.Comments).HasMaxLength(2000);
        entity.Property(x => x.ContactEmail).HasMaxLength(256);
    }
}
