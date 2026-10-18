namespace Reporting.Application.Contracts;

public sealed record CategoryBreakdown(string Category, int Count, double AverageOverall);

public sealed record TrendPoint(string Period, int Count, double AverageOverall, double AverageNps);

public sealed record StatusCounts(int New, int Reviewed, int Actioned);

public sealed record ReportSummary(
    int TotalCount,
    double AverageOverall,
    double AverageNps,
    IReadOnlyList<CategoryBreakdown> ByCategory,
    IReadOnlyList<TrendPoint> Trend,
    StatusCounts StatusCounts);
