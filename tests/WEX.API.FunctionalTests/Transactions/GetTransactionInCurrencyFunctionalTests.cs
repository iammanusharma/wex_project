using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace WEX.API.FunctionalTests.Transactions;

/// <summary>
/// Functional tests for GET /api/v1/transactions/{id}?currency={code}.
/// Runs against a LIVE running API + PostgreSQL.
/// Note: These tests first create a transaction then retrieve it with conversion —
/// this mirrors the real user journey end-to-end.
/// </summary>
[Collection("FunctionalTests")]
public sealed class GetTransactionInCurrencyFunctionalTests : IClassFixture<ApiClientFixture>
{
    private readonly HttpClient _client;

    public GetTransactionInCurrencyFunctionalTests(ApiClientFixture fixture)
    {
        _client = fixture.Client;
    }

    // ---------------------------------------------------------------------------
    // Happy path — requires real Treasury API to be reachable
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Get_ExistingTransaction_Returns200WithConvertedAmount()
    {
        // Arrange — store a transaction first
        var id = await StoreTransactionAsync("Functional GET test", "2024-06-15", 100.00m);

        // Act — retrieve with currency conversion (Euro is a stable, commonly available rate)
        var response = await _client.GetAsync($"/api/v1/transactions/{id}?currency=EUR");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_ExistingTransaction_ResponseContainsAllRequiredFields()
    {
        // Arrange
        var id = await StoreTransactionAsync("Functional fields test", "2024-03-20", 250.00m);

        // Act
        var response = await _client.GetAsync($"/api/v1/transactions/{id}?currency=EUR");
        var body = await response.Content.ReadFromJsonAsync<TransactionCurrencyResponse>();

        // Assert — all required fields from requirement are present
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();
        body!.Id.Should().Be(id);
        body.Description.Should().Be("Functional fields test");
        body.OriginalAmountUsd.Should().Be(250.00m);
        body.TargetCurrency.Should().Be("EUR");
        body.ExchangeRate.Should().BeGreaterThan(0);
        body.ConvertedAmount.Should().BeGreaterThan(0);
    }

    // ---------------------------------------------------------------------------
    // Validation failures
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Get_InvalidCurrencyCode_Returns400()
    {
        var id = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/v1/transactions/{id}?currency=INVALID");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_MissingCurrencyCode_Returns400()
    {
        var id = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/v1/transactions/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_NonExistentTransactionId_Returns404()
    {
        var unknownId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/v1/transactions/{unknownId}?currency=EUR");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    private async Task<Guid> StoreTransactionAsync(string description, string date, decimal amount)
    {
        var request = new { description, transactionDate = date, amountUsd = amount };
        var response = await _client.PostAsJsonAsync("/api/v1/transactions", request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<TransactionCreatedResponse>();
        return body!.Id;
    }

    private sealed record TransactionCreatedResponse(Guid Id);

    private sealed record TransactionCurrencyResponse(
        Guid Id,
        string Description,
        DateOnly TransactionDate,
        decimal OriginalAmountUsd,
        string TargetCurrency,
        decimal ExchangeRate,
        decimal ConvertedAmount);
}
