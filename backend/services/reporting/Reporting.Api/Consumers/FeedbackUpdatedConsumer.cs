using CustomerFeedback.BuildingBlocks.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Reporting.Api.Data;

namespace Reporting.Api.Consumers;

public sealed class FeedbackUpdatedConsumer(ReportingDbContext db) : IConsumer<FeedbackUpdated>
{
    public async Task Consume(ConsumeContext<FeedbackUpdated> context)
    {
        var message = context.Message;
        var existing = await db.Facts.FirstOrDefaultAsync(x => x.FeedbackId == message.FeedbackId, context.CancellationToken);
        if (existing is null)
        {
            return;
        }

        existing.Category = message.Category;
        existing.Status = message.Status;
        existing.UpdatedAt = message.UpdatedAt;
        await db.SaveChangesAsync(context.CancellationToken);
    }
}
