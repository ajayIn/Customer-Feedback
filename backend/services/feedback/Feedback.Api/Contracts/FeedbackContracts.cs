namespace Feedback.Api.Contracts;

public sealed record CreateFeedbackRequest(
    Guid StoreId,
    DateOnly VisitDate,
    int Overall,
    int Cleanliness,
    int Staff,
    int WaitTime,
    int ProductAvailability,
    int NpsScore,
    string Category,
    string? Comments,
    string? ContactEmail,
    bool IsAnonymous);

public sealed record PatchCategoryRequest(string Category);

public sealed record PatchStatusRequest(string Status);

public sealed record FeedbackDto(
    Guid Id,
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
    string? Comments,
    string? ContactEmail,
    bool IsAnonymous,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
