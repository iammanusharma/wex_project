using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WEX.API.IntegrationTests;
using Xunit;

namespace WEX.API.IntegrationTests.Controllers;

public class TransactionsControllerTests : IClassFixture<WexApiFactory>
{
    private readonly HttpClient _client;

    public TransactionsControllerTests(WexApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ----------------------------------------------------------------
    // Auth endpoint tests
    // ----------------------------------------------------------------

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        // Arrange
        var payload = new { username = "demo@wex.com", password = "WexDemo2024!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginDto>();
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.ExpiresIn.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        // Arrange
        var payload = new { username = "demo@wex.com", password = "WrongPassword!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_MissingPassword_Returns400()
    {
        // Arrange
        var payload = new { username = "demo@wex.com", password = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ----------------------------------------------------------------
    // Protected endpoint tests
    // ----------------------------------------------------------------

    [Fact]
    public async Task PostTransaction_WithoutToken_Returns401()
    {
        // Arrange
        var payload = new { description = "Test", transactionDate = "2024-01-15", amountUsd = 100.00m };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/transactions", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTransaction_WithoutToken_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/transactions/" + Guid.NewGuid());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostTransaction_WithValidToken_Returns201()
    {
        // Arrange
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            description = "Office supplies",
            transactionDate = "2024-01-15",
            amountUsd = 42.50m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/transactions", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<IdDto>();
        body!.Id.Should().NotBe(Guid.Empty);
    }

    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private async Task<string> GetTokenAsync()
    {
        var payload = new { username = "demo@wex.com", password = "WexDemo2024!" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);
        var body = await response.Content.ReadFromJsonAsync<LoginDto>();
        return body!.AccessToken;
    }

    private record LoginDto(string AccessToken, int ExpiresIn);
    private record IdDto(Guid Id);
}
