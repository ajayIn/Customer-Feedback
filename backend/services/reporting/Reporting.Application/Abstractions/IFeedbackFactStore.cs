using Reporting.Domain;

namespace Reporting.Application.Abstractions;

public interface IFeedbackFactStore
{
    Task<FeedbackFact?> FindAsync(Guid feedbackId, CancellationToken cancellationToken);
    void Add(FeedbackFact fact);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<FeedbackFact>> QueryAsync(Guid? storeId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken);
}
