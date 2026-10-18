namespace Identity.Application.Contracts;

public sealed record RegisterRequest(string Email, string Password, string DisplayName);

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshRequest(string RefreshToken);

public sealed record UserProfile(Guid Id, string Email, string DisplayName, IReadOnlyList<string> Roles);

public sealed record TokenResponse(string AccessToken, string RefreshToken, int ExpiresIn, UserProfile User);
