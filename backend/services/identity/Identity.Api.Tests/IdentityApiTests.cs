using System.Net;
using System.Net.Http.Json;
using Identity.Application.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Identity.Api.Tests;

public sealed class IdentityApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IdentityApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("UseInMemoryDatabase", "true");
            builder.UseSetting("Jwt:SigningKey", "test-signing-key-at-least-32-chars-long");
        }).CreateClient();
    }

    [Fact]
    public async Task Register_and_login_returns_tokens()
    {
        var email = $"user{Guid.NewGuid():N}@test.local";
        var register = await _client.PostAsJsonAsync("/api/identity/register", new RegisterRequest(email, "Password1!", "Test User"));
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/identity/login", new LoginRequest(email, "Password1!"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var tokens = await login.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(tokens);
        Assert.False(string.IsNullOrWhiteSpace(tokens.AccessToken));
        Assert.Contains("Customer", tokens.User.Roles);
    }
}
