using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using CustomerFeedback.BuildingBlocks.Messaging;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Reporting.Application.Contracts;

namespace Reporting.Api.Tests;

public sealed class ReportingApiTests
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    [Fact]
    public async Task FeedbackSubmitted_updates_summary()
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("UseInMemoryDatabase", "true");
            builder.UseSetting("MassTransit:UseInMemory", "true");
            builder.UseSetting("Jwt:SigningKey", SigningKey);
        });

        var feedbackId = Guid.NewGuid();
        var storeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        using (var scope = factory.Services.CreateScope())
        {
            var publish = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            await publish.Publish(new FeedbackSubmitted(
                feedbackId,
                storeId,
                null,
                DateOnly.FromDateTime(DateTime.UtcNow),
                5, 5, 5, 4, 4,
                10,
                "Service",
                "New",
                DateTimeOffset.UtcNow));
        }

        await Task.Delay(300);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateManagerToken());

        var summary = await client.GetFromJsonAsync<ReportSummary>("/api/reports/summary?storeId=" + storeId);
        Assert.NotNull(summary);
        Assert.Equal(1, summary.TotalCount);
        Assert.Equal(5, summary.AverageOverall);
    }

    private static string CreateManagerToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var token = new JwtSecurityToken(
            issuer: "customer-feedback",
            audience: "customer-feedback",
            claims:
            [
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("role", "StoreManager")
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
