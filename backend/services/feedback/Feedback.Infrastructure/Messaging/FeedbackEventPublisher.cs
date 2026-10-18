using CustomerFeedback.BuildingBlocks.Messaging;
using Feedback.Application.Abstractions;
using Feedback.Domain;
using MassTransit;

namespace Feedback.Infrastructure.Messaging;

public sealed class FeedbackEventPublisher(IPublishEndpoint publish) : IFeedbackEventPublisher
{
    public Task PublishSubmittedAsync(FeedbackEntry entry, CancellationToken cancellationToken) =>
        publish.Publish(new FeedbackSubmitted(
            entry.Id,
            entry.StoreId,
            entry.CustomerId,
            entry.VisitDate,
            entry.Overall,
            entry.Cleanliness,
            entry.Staff,
            entry.WaitTime,
            entry.ProductAvailability,
            entry.NpsScore,
            entry.Category,
            entry.Status,
            entry.CreatedAt), cancellationToken);

    public Task PublishUpdatedAsync(FeedbackEntry entry, CancellationToken cancellationToken) =>
        publish.Publish(new FeedbackUpdated(entry.Id, entry.Category, entry.Status, entry.UpdatedAt), cancellationToken);
}
