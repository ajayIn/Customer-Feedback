using Feedback.Domain;

namespace Feedback.Application.Abstractions;

public sealed record FeedbackListFilter(
    Guid? CustomerId,
    Guid? StoreId,
    string? Category,
    string? Status,
    DateOnly? From,
    DateOnly? To);

public sealed record FeedbackPage(IReadOnlyList<FeedbackEntry> Items, int TotalCount);

public interface IFeedbackRepository
{
    Task AddAsync(FeedbackEntry entry, CancellationToken cancellationToken);
    Task<FeedbackEntry?> FindAsync(Guid id, CancellationToken cancellationToken);
    Task<FeedbackPage> ListAsync(FeedbackListFilter filter, int page, int pageSize, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
