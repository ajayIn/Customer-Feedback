namespace Reporting.Api.Domain;

public sealed class FeedbackFact
{
    public Guid FeedbackId { get; set; }
    public Guid StoreId { get; set; }
    public DateOnly VisitDate { get; set; }
    public int Overall { get; set; }
    public int NpsScore { get; set; }
    public string Category { get; set; } = "Other";
    public string Status { get; set; } = "New";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
