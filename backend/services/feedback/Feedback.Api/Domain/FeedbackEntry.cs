namespace Feedback.Api.Domain;

public sealed class FeedbackEntry
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateOnly VisitDate { get; set; }
    public int Overall { get; set; }
    public int Cleanliness { get; set; }
    public int Staff { get; set; }
    public int WaitTime { get; set; }
    public int ProductAvailability { get; set; }
    public int NpsScore { get; set; }
    public string Category { get; set; } = "Other";
    public string? Comments { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsAnonymous { get; set; }
    public string Status { get; set; } = "New";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
