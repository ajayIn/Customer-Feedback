using FluentValidation;
using Feedback.Api.Contracts;

namespace Feedback.Api.Validation;

public sealed class CreateFeedbackRequestValidator : AbstractValidator<CreateFeedbackRequest>
{
    private static readonly string[] Categories = ["Service", "Product", "Facilities", "Other"];

    public CreateFeedbackRequestValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.VisitDate)
            .NotEmpty()
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Visit date cannot be in the future.");
        RuleFor(x => x.Overall).InclusiveBetween(1, 5);
        RuleFor(x => x.Cleanliness).InclusiveBetween(1, 5);
        RuleFor(x => x.Staff).InclusiveBetween(1, 5);
        RuleFor(x => x.WaitTime).InclusiveBetween(1, 5);
        RuleFor(x => x.ProductAvailability).InclusiveBetween(1, 5);
        RuleFor(x => x.NpsScore).InclusiveBetween(0, 10);
        RuleFor(x => x.Category).Must(c => Categories.Contains(c))
            .WithMessage("Category must be Service, Product, Facilities, or Other.");
        RuleFor(x => x.Comments).MaximumLength(2000);
        RuleFor(x => x.ContactEmail)
            .EmailAddress()
            .When(x => !x.IsAnonymous && !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.ContactEmail)
            .Empty()
            .When(x => x.IsAnonymous)
            .WithMessage("Contact email must be omitted when submitting anonymously.");
    }
}
