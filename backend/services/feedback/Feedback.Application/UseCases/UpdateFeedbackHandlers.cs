using CustomerFeedback.BuildingBlocks.Errors;
using Feedback.Application.Abstractions;
using Feedback.Application.Contracts;
using Feedback.Application.Mapping;
using Feedback.Domain;

namespace Feedback.Application.UseCases;

public sealed class UpdateFeedbackCategoryHandler(IFeedbackRepository repository, IFeedbackEventPublisher publisher)
{
    public async Task<FeedbackDto> Handle(Guid id, PatchCategoryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            FeedbackEntry.EnsureCategory(request.Category);
        }
        catch (FeedbackRuleException exception)
        {
            throw FeedbackRules.ToValidation(exception);
        }

        var entry = await repository.FindAsync(id, cancellationToken)
            ?? throw AppException.NotFound("Feedback was not found.");
        entry.ChangeCategory(request.Category, DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        await publisher.PublishUpdatedAsync(entry, cancellationToken);
        return entry.ToDto();
    }
}

public sealed class UpdateFeedbackStatusHandler(IFeedbackRepository repository, IFeedbackEventPublisher publisher)
{
    public async Task<FeedbackDto> Handle(Guid id, PatchStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            FeedbackEntry.EnsureStatus(request.Status);
        }
        catch (FeedbackRuleException exception)
        {
            throw FeedbackRules.ToValidation(exception);
        }

        var entry = await repository.FindAsync(id, cancellationToken)
            ?? throw AppException.NotFound("Feedback was not found.");
        entry.ChangeStatus(request.Status, DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        await publisher.PublishUpdatedAsync(entry, cancellationToken);
        return entry.ToDto();
    }
}
