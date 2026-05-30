using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WEX.Application.Features.Transactions.Queries.GetPurchaseInCurrency;
using WEX.Domain.Entities;
using WEX.Domain.Exceptions;
using WEX.Domain.Interfaces;
using Xunit;

namespace WEX.Application.Tests.Features.Transactions.Queries;

/// <summary>
/// Unit tests for <see cref="GetPurchaseInCurrencyQueryHandler"/>.
/// All external dependencies (repository, exchange rate service) are mocked
/// so tests are fast and deterministic — no DB or HTTP calls.
/// </summary>
public sealed class GetPurchaseInCurrencyQueryHandlerTests
{
    private readonly IPurchaseTransactionRepository _repository;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly GetPurchaseInCurrencyQueryHandler _handler;

    // A reusable valid transaction for tests
    private static readonly PurchaseTransaction SampleTransaction =
        PurchaseTransaction.Create("Office supplies", new DateOnly(2024, 6, 15), 100.00m);

    public GetPurchaseInCurrencyQueryHandlerTests()
    {
        _repository = Substitute.For<IPurchaseTransactionRepository>();
        _exchangeRateService = Substitute.For<IExchangeRateService>();
        var logger = Substitute.For<ILogger<GetPurchaseInCurrencyQueryHandler>>();
        _handler = new GetPurchaseInCurrencyQueryHandler(_repository, _exchangeRateService, logger);
    }

    // ---------------------------------------------------------------------------
    // Happy path
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Handle_ValidQuery_ReturnsConvertedResponse()
    {
        // Arrange
        _repository.GetByIdAsync(SampleTransaction.Id, Arg.Any<CancellationToken>())
            .Returns(SampleTransaction);
        _exchangeRateService.GetRateAsync("EUR", SampleTransaction.TransactionDate, Arg.Any<CancellationToken>())
            .Returns(1.08m);

        var query = new GetPurchaseInCurrencyQuery(SampleTransaction.Id, "EUR");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(SampleTransaction.Id);
        result.TargetCurrency.Should().Be("EUR");
        result.ExchangeRate.Should().Be(1.08m);
        result.ConvertedAmount.Should().Be(108.00m); // 100.00 * 1.08
    }

    [Fact]
    public async Task Handle_ValidQuery_ConvertsAmountRoundedToTwoDecimalPlaces()
    {
        // Arrange — rate that produces a long decimal result
        _repository.GetByIdAsync(SampleTransaction.Id, Arg.Any<CancellationToken>())
            .Returns(SampleTransaction);
        _exchangeRateService.GetRateAsync("GBP", SampleTransaction.TransactionDate, Arg.Any<CancellationToken>())
            .Returns(0.7853m);

        var query = new GetPurchaseInCurrencyQuery(SampleTransaction.Id, "GBP");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert — 100.00 * 0.7853 = 78.53 (already 2dp)
        result.ConvertedAmount.Should().Be(78.53m);
    }

    [Fact]
    public async Task Handle_ValidQuery_NormalisesTargetCurrencyToUppercase()
    {
        // Arrange — lowercase currency code passed in
        _repository.GetByIdAsync(SampleTransaction.Id, Arg.Any<CancellationToken>())
            .Returns(SampleTransaction);
        _exchangeRateService.GetRateAsync("EUR", SampleTransaction.TransactionDate, Arg.Any<CancellationToken>())
            .Returns(1.08m);

        var query = new GetPurchaseInCurrencyQuery(SampleTransaction.Id, "eur");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert — stored and returned as uppercase
        result.TargetCurrency.Should().Be("EUR");
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsOriginalAmountUnchanged()
    {
        // Arrange
        _repository.GetByIdAsync(SampleTransaction.Id, Arg.Any<CancellationToken>())
            .Returns(SampleTransaction);
        _exchangeRateService.GetRateAsync("CAD", SampleTransaction.TransactionDate, Arg.Any<CancellationToken>())
            .Returns(1.36m);

        var query = new GetPurchaseInCurrencyQuery(SampleTransaction.Id, "CAD");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert — original USD amount is preserved in response
        result.OriginalAmountUsd.Should().Be(SampleTransaction.AmountUsd);
    }

    // ---------------------------------------------------------------------------
    // Transaction not found
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Handle_TransactionNotFound_ThrowsTransactionNotFoundException()
    {
        // Arrange — repository returns null (no match)
        var unknownId = Guid.NewGuid();
        _repository.GetByIdAsync(unknownId, Arg.Any<CancellationToken>())
            .Returns((PurchaseTransaction?)null);

        var query = new GetPurchaseInCurrencyQuery(unknownId, "EUR");

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert — domain exception, not a generic one
        await act.Should().ThrowAsync<TransactionNotFoundException>();
    }

    // ---------------------------------------------------------------------------
    // Exchange rate unavailable
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Handle_NoRateWithinSixMonths_ThrowsCurrencyConversionUnavailableException()
    {
        // Arrange — transaction exists but no rate found (service returns null)
        _repository.GetByIdAsync(SampleTransaction.Id, Arg.Any<CancellationToken>())
            .Returns(SampleTransaction);
        _exchangeRateService.GetRateAsync("EUR", SampleTransaction.TransactionDate, Arg.Any<CancellationToken>())
            .Returns((decimal?)null);

        var query = new GetPurchaseInCurrencyQuery(SampleTransaction.Id, "EUR");

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert — 422 domain exception
        await act.Should().ThrowAsync<CurrencyConversionUnavailableException>();
    }

    [Fact]
    public async Task Handle_NoRate_CallsExchangeServiceWithCorrectCurrencyAndDate()
    {
        // Arrange
        _repository.GetByIdAsync(SampleTransaction.Id, Arg.Any<CancellationToken>())
            .Returns(SampleTransaction);
        _exchangeRateService.GetRateAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(1.08m);

        var query = new GetPurchaseInCurrencyQuery(SampleTransaction.Id, "JPY");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert — service was called with normalised currency and correct purchase date
        await _exchangeRateService.Received(1).GetRateAsync(
            "JPY",
            SampleTransaction.TransactionDate,
            Arg.Any<CancellationToken>());
    }
}
