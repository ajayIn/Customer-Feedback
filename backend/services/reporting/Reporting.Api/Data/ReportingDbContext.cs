using Microsoft.EntityFrameworkCore;
using Reporting.Api.Domain;

namespace Reporting.Api.Data;

public sealed class ReportingDbContext(DbContextOptions<ReportingDbContext> options) : DbContext(options)
{
    public DbSet<FeedbackFact> Facts => Set<FeedbackFact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<FeedbackFact>();
        entity.ToTable("FeedbackFacts");
        entity.HasKey(x => x.FeedbackId);
        entity.HasIndex(x => x.StoreId);
        entity.HasIndex(x => x.VisitDate);
        entity.HasIndex(x => x.Category);
        entity.Property(x => x.Category).HasMaxLength(32);
        entity.Property(x => x.Status).HasMaxLength(32);
    }
}
