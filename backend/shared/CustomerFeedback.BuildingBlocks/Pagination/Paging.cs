namespace CustomerFeedback.BuildingBlocks.Pagination;

public static class Paging
{
    public static (int Page, int PageSize) Normalize(int? page, int? pageSize)
    {
        var p = page is null or < 1 ? 1 : page.Value;
        var size = pageSize is null or < 1 ? 20 : Math.Min(pageSize.Value, 100);
        return (p, size);
    }
}
