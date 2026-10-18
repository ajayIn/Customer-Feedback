namespace Feedback.Domain;

public sealed class FeedbackRuleException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}

public sealed class FeedbackEntry
{
    public static readonly IReadOnlySet<string> Categories =
        new HashSet<string>(StringComparer.Ordinal) { "Service", "Product", "Facilities", "Other" };

    public static readonly IReadOnlySet<string> Statuses =
        new HashSet<string>(StringComparer.Ordinal) { "New", "Reviewed", "Actioned" };

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

    public static FeedbackEntry Create(
        Guid id,
        Guid storeId,
        Guid? customerId,
        DateOnly visitDate,
        int overall,
        int cleanliness,
        int staff,
        int waitTime,
        int productAvailability,
        int npsScore,
        string category,
        string? comments,
        string? contactEmail,
        bool isAnonymous,
        DateTimeOffset now) => new()
    {
        Id = id,
        StoreId = storeId,
        CustomerId = customerId,
        VisitDate = visitDate,
        Overall = overall,
        Cleanliness = cleanliness,
        Staff = staff,
        WaitTime = waitTime,
        ProductAvailability = productAvailability,
        NpsScore = npsScore,
        Category = category,
        Comments = comments,
        ContactEmail = contactEmail,
        IsAnonymous = isAnonymous,
        Status = "New",
        CreatedAt = now,
        UpdatedAt = now
    };

    public static void EnsureCategory(string category)
    {
        if (!Categories.Contains(category))
        {
            throw new FeedbackRuleException("category", "Category must be Service, Product, Facilities, or Other.");
        }
    }

    public static void EnsureStatus(string status)
    {
        if (!Statuses.Contains(status))
        {
            throw new FeedbackRuleException("status", "Status must be New, Reviewed, or Actioned.");
        }
    }

    public void ChangeCategory(string category, DateTimeOffset now)
    {
        EnsureCategory(category);
        Category = category;
        UpdatedAt = now;
    }

    public void ChangeStatus(string status, DateTimeOffset now)
    {
        EnsureStatus(status);
        Status = status;
        UpdatedAt = now;
    }
}
