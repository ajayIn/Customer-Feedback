using Feedback.Domain;

namespace Feedback.Application.Abstractions;

public interface IFeedbackEventPublisher
{
    Task PublishSubmittedAsync(FeedbackEntry entry, CancellationToken cancellationToken);
    Task PublishUpdatedAsync(FeedbackEntry entry, CancellationToken cancellationToken);
}
