using CustomerFeedback.BuildingBlocks.Messaging;
using Reporting.Application.Abstractions;
using Reporting.Application.Contracts;
using Reporting.Domain;

namespace Reporting.Application.UseCases;

public sealed class GetReportSummaryHandler(IFeedbackFactStore store)
{
    public async Task<ReportSummary> Handle(Guid? storeId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        var rows = await store.QueryAsync(storeId, from, to, cancellationToken);
        var total = rows.Count;
        var averageOverall = total == 0 ? 0 : rows.Average(x => x.Overall);
        var averageNps = total == 0 ? 0 : rows.Average(x => x.NpsScore);
        var byCategory = rows
            .GroupBy(x => x.Category)
            .Select(g => new CategoryBreakdown(g.Key, g.Count(), g.Average(x => x.Overall)))
            .OrderBy(x => x.Category)
            .ToList();
        var trend = rows
            .GroupBy(x => x.VisitDate)
            .OrderBy(g => g.Key)
            .Select(g => new TrendPoint(
                g.Key.ToString("yyyy-MM-dd"),
                g.Count(),
                g.Average(x => x.Overall),
                g.Average(x => x.NpsScore)))
            .ToList();
        var statusCounts = new StatusCounts(
            rows.Count(x => x.Status == "New"),
            rows.Count(x => x.Status == "Reviewed"),
            rows.Count(x => x.Status == "Actioned"));

        return new ReportSummary(total, averageOverall, averageNps, byCategory, trend, statusCounts);
    }
}

public sealed class ProjectSubmittedFeedbackHandler(IFeedbackFactStore store)
{
    public async Task Handle(FeedbackSubmitted message, CancellationToken cancellationToken)
    {
        var existing = await store.FindAsync(message.FeedbackId, cancellationToken);
        if (existing is null)
        {
            store.Add(FeedbackFact.FromSubmitted(
                message.FeedbackId,
                message.StoreId,
                message.VisitDate,
                message.Overall,
                message.NpsScore,
                message.Category,
                message.Status,
                message.CreatedAt));
        }
        else
        {
            existing.ApplySubmitted(
                message.StoreId,
                message.VisitDate,
                message.Overall,
                message.NpsScore,
                message.Category,
                message.Status,
                message.CreatedAt);
        }

        await store.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ProjectUpdatedFeedbackHandler(IFeedbackFactStore store)
{
    public async Task Handle(FeedbackUpdated message, CancellationToken cancellationToken)
    {
        var existing = await store.FindAsync(message.FeedbackId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        existing.ApplyUpdated(message.Category, message.Status, message.UpdatedAt);
        await store.SaveChangesAsync(cancellationToken);
    }
}
