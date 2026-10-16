namespace CustomerFeedback.BuildingBlocks.Messaging;

public sealed record FeedbackUpdated(
    Guid FeedbackId,
    string Category,
    string Status,
    DateTimeOffset UpdatedAt);
