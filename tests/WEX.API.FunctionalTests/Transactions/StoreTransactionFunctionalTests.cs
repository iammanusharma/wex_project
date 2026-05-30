using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace WEX.API.FunctionalTests.Transactions;

/// <summary>
/// Functional tests for POST /api/v1/transactions.
/// These tests run against a LIVE, fully deployed API instance.
/// Set FUNCTIONAL_TEST_BASE_URL env var to target Dev/UAT/Prod.
/// Requires: API running + PostgreSQL available.
/// </summary>
[Collection("FunctionalTests")]
public sealed class StoreTransactionFunctionalTests : IClassFixture<ApiClientFixture>
{
    private readonly HttpClient _client;

    public StoreTransactionFunctionalTests(ApiClientFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Post_ValidTransaction_Returns201Created()
    {
        var request = new
        {
            description = "Functional test - office supplies",
            transactionDate = "2024-06-15",
            amountUsd = 49.99m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/transactions", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Post_ValidTransaction_ReturnsNonEmptyId()
    {
        var request = new
        {
            description = "Functional test - conference fee",
            transactionDate = "2024-07-01",
            amountUsd = 350.00m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/transactions", request);
        var body = await response.Content.ReadFromJsonAsync<TransactionCreatedResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Post_ValidTransaction_ReturnsLocationHeader()
    {
        var request = new
        {
            description = "Functional test - hotel",
            transactionDate = "2024-08-20",
            amountUsd = 199.00m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/transactions", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Post_EmptyDescription_Returns400()
    {
        var request = new
        {
            description = "",
            transactionDate = "2024-06-15",
            amountUsd = 49.99m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/transactions", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_DescriptionOver50Chars_Returns400()
    {
        var request = new
        {
            description = new string('X', 51),
            transactionDate = "2024-06-15",
            amountUsd = 49.99m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/transactions", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_NegativeAmount_Returns400()
    {
        var request = new
        {
            description = "Negative amount",
            transactionDate = "2024-06-15",
            amountUsd = -1.00m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/transactions", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed record TransactionCreatedResponse(Guid Id);
}
