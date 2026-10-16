namespace CustomerFeedback.BuildingBlocks.Messaging;

public sealed record FeedbackSubmitted(
    Guid FeedbackId,
    Guid StoreId,
    Guid? CustomerId,
    DateOnly VisitDate,
    int Overall,
    int Cleanliness,
    int Staff,
    int WaitTime,
    int ProductAvailability,
    int NpsScore,
    string Category,
    string Status,
    DateTimeOffset CreatedAt);
