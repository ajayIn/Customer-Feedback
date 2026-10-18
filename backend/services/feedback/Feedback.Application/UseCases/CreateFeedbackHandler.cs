using CustomerFeedback.BuildingBlocks.Text;
using CustomerFeedback.BuildingBlocks.Validation;
using Feedback.Application.Abstractions;
using Feedback.Application.Contracts;
using Feedback.Application.Mapping;
using Feedback.Domain;
using FluentValidation;

namespace Feedback.Application.UseCases;

public sealed class CreateFeedbackHandler(
    IValidator<CreateFeedbackRequest> validator,
    IFeedbackRepository repository,
    IFeedbackEventPublisher publisher)
{
    public async Task<FeedbackDto> Handle(CreateFeedbackRequest request, Guid customerId, CancellationToken cancellationToken)
    {
        await RequestValidator.EnsureValidAsync(request, validator, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entry = FeedbackEntry.Create(
            Guid.NewGuid(),
            request.StoreId,
            request.IsAnonymous ? null : customerId,
            request.VisitDate,
            request.Overall,
            request.Cleanliness,
            request.Staff,
            request.WaitTime,
            request.ProductAvailability,
            request.NpsScore,
            request.Category,
            CommentSanitizer.Sanitize(request.Comments),
            request.IsAnonymous ? null : request.ContactEmail,
            request.IsAnonymous,
            now);

        await repository.AddAsync(entry, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await publisher.PublishSubmittedAsync(entry, cancellationToken);
        return entry.ToDto();
    }
}
