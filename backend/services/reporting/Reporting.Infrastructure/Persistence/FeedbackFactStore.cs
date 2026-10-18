using Microsoft.EntityFrameworkCore;
using Reporting.Application.Abstractions;
using Reporting.Domain;

namespace Reporting.Infrastructure.Persistence;

public sealed class FeedbackFactStore(ReportingDbContext db) : IFeedbackFactStore
{
    public Task<FeedbackFact?> FindAsync(Guid feedbackId, CancellationToken cancellationToken) =>
        db.Facts.FirstOrDefaultAsync(x => x.FeedbackId == feedbackId, cancellationToken);

    public void Add(FeedbackFact fact) => db.Facts.Add(fact);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);

    public async Task<IReadOnlyList<FeedbackFact>> QueryAsync(Guid? storeId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        var query = db.Facts.AsNoTracking().AsQueryable();
        if (storeId is not null)
        {
            query = query.Where(x => x.StoreId == storeId);
        }

        if (from is not null)
        {
            query = query.Where(x => x.VisitDate >= from);
        }

        if (to is not null)
        {
            query = query.Where(x => x.VisitDate <= to);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
