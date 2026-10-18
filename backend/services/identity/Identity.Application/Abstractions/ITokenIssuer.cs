using Identity.Application.Contracts;

namespace Identity.Application.Abstractions;

public interface ITokenIssuer
{
    Task<TokenResponse> IssueAsync(Guid userId, CancellationToken cancellationToken);
    Task<Guid?> RotateAsync(string refreshToken, CancellationToken cancellationToken);
}
