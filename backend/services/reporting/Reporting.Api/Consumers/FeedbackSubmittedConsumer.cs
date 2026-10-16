using CustomerFeedback.BuildingBlocks.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Reporting.Api.Data;
using Reporting.Api.Domain;

namespace Reporting.Api.Consumers;

public sealed class FeedbackSubmittedConsumer(ReportingDbContext db) : IConsumer<FeedbackSubmitted>
{
    public async Task Consume(ConsumeContext<FeedbackSubmitted> context)
    {
        var message = context.Message;
        var existing = await db.Facts.FirstOrDefaultAsync(x => x.FeedbackId == message.FeedbackId, context.CancellationToken);
        if (existing is null)
        {
            db.Facts.Add(new FeedbackFact
            {
                FeedbackId = message.FeedbackId,
                StoreId = message.StoreId,
                VisitDate = message.VisitDate,
                Overall = message.Overall,
                NpsScore = message.NpsScore,
                Category = message.Category,
                Status = message.Status,
                CreatedAt = message.CreatedAt,
                UpdatedAt = message.CreatedAt
            });
        }
        else
        {
            existing.StoreId = message.StoreId;
            existing.VisitDate = message.VisitDate;
            existing.Overall = message.Overall;
            existing.NpsScore = message.NpsScore;
            existing.Category = message.Category;
            existing.Status = message.Status;
            existing.UpdatedAt = message.CreatedAt;
        }

        await db.SaveChangesAsync(context.CancellationToken);
    }
}
