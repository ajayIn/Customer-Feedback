using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Feedback.Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;

namespace Feedback.Api.Tests;

public sealed class FeedbackApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    [Fact]
    public async Task Customer_can_submit_feedback()
    {
        var client = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("UseInMemoryDatabase", "true");
            builder.UseSetting("MassTransit:UseInMemory", "true");
            builder.UseSetting("Jwt:SigningKey", SigningKey);
        }).CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateToken("Customer"));

        var storeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var response = await client.PostAsJsonAsync("/api/feedback", new CreateFeedbackRequest(
            storeId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            5, 4, 5, 3, 4,
            9,
            "Service",
            "Great visit",
            "customer@test.local",
            false));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<FeedbackDto>();
        Assert.NotNull(body);
        Assert.Equal("New", body.Status);
    }

    private static string CreateToken(string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var token = new JwtSecurityToken(
            issuer: "customer-feedback",
            audience: "customer-feedback",
            claims:
            [
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("role", role)
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
