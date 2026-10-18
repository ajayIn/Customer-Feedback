namespace Reporting.Domain;

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

    public static FeedbackFact FromSubmitted(
        Guid feedbackId,
        Guid storeId,
        DateOnly visitDate,
        int overall,
        int npsScore,
        string category,
        string status,
        DateTimeOffset createdAt) => new()
    {
        FeedbackId = feedbackId,
        StoreId = storeId,
        VisitDate = visitDate,
        Overall = overall,
        NpsScore = npsScore,
        Category = category,
        Status = status,
        CreatedAt = createdAt,
        UpdatedAt = createdAt
    };

    public void ApplySubmitted(
        Guid storeId,
        DateOnly visitDate,
        int overall,
        int npsScore,
        string category,
        string status,
        DateTimeOffset updatedAt)
    {
        StoreId = storeId;
        VisitDate = visitDate;
        Overall = overall;
        NpsScore = npsScore;
        Category = category;
        Status = status;
        UpdatedAt = updatedAt;
    }

    public void ApplyUpdated(string category, string status, DateTimeOffset updatedAt)
    {
        Category = category;
        Status = status;
        UpdatedAt = updatedAt;
    }
}
