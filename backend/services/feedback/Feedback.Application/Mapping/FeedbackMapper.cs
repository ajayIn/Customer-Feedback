using Feedback.Application.Contracts;
using Feedback.Domain;

namespace Feedback.Application.Mapping;

public static class FeedbackMapper
{
    public static FeedbackDto ToDto(this FeedbackEntry entry) => new(
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
        entry.Comments,
        entry.ContactEmail,
        entry.IsAnonymous,
        entry.Status,
        entry.CreatedAt,
        entry.UpdatedAt);
}
