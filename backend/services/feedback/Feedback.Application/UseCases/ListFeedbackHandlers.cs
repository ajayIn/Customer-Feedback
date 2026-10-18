using CustomerFeedback.BuildingBlocks.Pagination;
using Feedback.Application.Abstractions;
using Feedback.Application.Contracts;
using Feedback.Application.Mapping;

namespace Feedback.Application.UseCases;

public sealed class ListMyFeedbackHandler(IFeedbackRepository repository)
{
    public async Task<PagedResult<FeedbackDto>> Handle(Guid customerId, int? page, int? pageSize, CancellationToken cancellationToken)
    {
        var (p, size) = Paging.Normalize(page, pageSize);
        var result = await repository.ListAsync(
            new FeedbackListFilter(customerId, null, null, null, null, null),
            p,
            size,
            cancellationToken);
        return new PagedResult<FeedbackDto>(result.Items.Select(x => x.ToDto()).ToList(), p, size, result.TotalCount);
    }
}

public sealed class ListFeedbackHandler(IFeedbackRepository repository)
{
    public async Task<PagedResult<FeedbackDto>> Handle(
        int? page,
        int? pageSize,
        Guid? storeId,
        string? category,
        string? status,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken)
    {
        var (p, size) = Paging.Normalize(page, pageSize);
        var result = await repository.ListAsync(
            new FeedbackListFilter(null, storeId, category, status, from, to),
            p,
            size,
            cancellationToken);
        return new PagedResult<FeedbackDto>(result.Items.Select(x => x.ToDto()).ToList(), p, size, result.TotalCount);
    }
}
