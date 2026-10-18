namespace Identity.Application.Abstractions;

public sealed record UserAccount(Guid Id, string Email, string DisplayName, IReadOnlyList<string> Roles);

public abstract record CreateCustomerResult
{
    public sealed record Created(UserAccount Account) : CreateCustomerResult;
    public sealed record Invalid(IReadOnlyDictionary<string, string[]> Errors) : CreateCustomerResult;
}

public interface IUserAccounts
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task<CreateCustomerResult> CreateCustomerAsync(string email, string password, string displayName, CancellationToken cancellationToken);
    Task<UserAccount?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken);
    Task<UserAccount?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
}
