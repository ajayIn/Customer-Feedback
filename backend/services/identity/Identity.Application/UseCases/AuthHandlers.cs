using CustomerFeedback.BuildingBlocks.Errors;
using CustomerFeedback.BuildingBlocks.Validation;
using FluentValidation;
using Identity.Application.Abstractions;
using Identity.Application.Contracts;

namespace Identity.Application.UseCases;

public sealed class RegisterUserHandler(IValidator<RegisterRequest> validator, IUserAccounts accounts)
{
    public async Task<UserProfile> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        await RequestValidator.EnsureValidAsync(request, validator, cancellationToken);
        if (await accounts.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw AppException.Conflict("An account with that email already exists.");
        }

        var created = await accounts.CreateCustomerAsync(request.Email, request.Password, request.DisplayName.Trim(), cancellationToken);
        if (created is CreateCustomerResult.Invalid invalid)
        {
            throw AppException.Validation(new Dictionary<string, string[]>(invalid.Errors));
        }

        var account = ((CreateCustomerResult.Created)created).Account;
        return new UserProfile(account.Id, account.Email, account.DisplayName, account.Roles);
    }
}

public sealed class LoginHandler(
    IValidator<LoginRequest> validator,
    IUserAccounts accounts,
    ITokenIssuer tokens)
{
    public async Task<TokenResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        await RequestValidator.EnsureValidAsync(request, validator, cancellationToken);
        var account = await accounts.AuthenticateAsync(request.Email, request.Password, cancellationToken)
            ?? throw AppException.Unauthorized("Invalid email or password.");
        return await tokens.IssueAsync(account.Id, cancellationToken);
    }
}

public sealed class RefreshTokenHandler(ITokenIssuer tokens)
{
    public async Task<TokenResponse> Handle(RefreshRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw AppException.Unauthorized("Refresh token is required.");
        }

        var userId = await tokens.RotateAsync(request.RefreshToken, cancellationToken)
            ?? throw AppException.Unauthorized("Refresh token is invalid or expired.");
        return await tokens.IssueAsync(userId, cancellationToken);
    }
}

public sealed class GetCurrentUserHandler(IUserAccounts accounts)
{
    public async Task<UserProfile> Handle(Guid userId, CancellationToken cancellationToken)
    {
        var account = await accounts.FindByIdAsync(userId, cancellationToken)
            ?? throw AppException.Unauthorized();
        return new UserProfile(account.Id, account.Email, account.DisplayName, account.Roles);
    }
}
