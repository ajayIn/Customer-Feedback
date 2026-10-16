using FluentValidation;

namespace CustomerFeedback.BuildingBlocks.Validation;

public static class RequestValidator
{
    public static async Task EnsureValidAsync<T>(T instance, IValidator<T> validator, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
