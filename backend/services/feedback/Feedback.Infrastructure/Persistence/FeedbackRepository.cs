using Feedback.Application.Abstractions;
using Feedback.Domain;
using Microsoft.EntityFrameworkCore;

namespace Feedback.Infrastructure.Persistence;

public sealed class FeedbackRepository(FeedbackDbContext db) : IFeedbackRepository
{
    public Task AddAsync(FeedbackEntry entry, CancellationToken cancellationToken)
    {
        db.Feedback.Add(entry);
        return Task.CompletedTask;
    }

    public Task<FeedbackEntry?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        db.Feedback.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<FeedbackPage> ListAsync(FeedbackListFilter filter, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = db.Feedback.AsNoTracking().AsQueryable();
        if (filter.CustomerId is not null)
        {
            query = query.Where(x => x.CustomerId == filter.CustomerId);
        }

        if (filter.StoreId is not null)
        {
            query = query.Where(x => x.StoreId == filter.StoreId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(x => x.Category == filter.Category);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(x => x.Status == filter.Status);
        }

        if (filter.From is not null)
        {
            query = query.Where(x => x.VisitDate >= filter.From);
        }

        if (filter.To is not null)
        {
            query = query.Where(x => x.VisitDate <= filter.To);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new FeedbackPage(items, total);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
