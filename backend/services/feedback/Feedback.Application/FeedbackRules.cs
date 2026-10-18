using CustomerFeedback.BuildingBlocks.Errors;
using Feedback.Domain;

namespace Feedback.Application;

internal static class FeedbackRules
{
    public static AppException ToValidation(FeedbackRuleException exception) =>
        AppException.Validation(new Dictionary<string, string[]>
        {
            [exception.Field] = [exception.Message]
        });
}
