using MediatR;
using Microsoft.Extensions.Logging;
using WEX.Domain.Exceptions;
using WEX.Domain.Interfaces;

namespace WEX.Application.Features.Transactions.Queries.GetPurchaseInCurrency;

/// <summary>
/// Handles <see cref="GetPurchaseInCurrencyQuery"/>:
/// retrieves a transaction and converts its amount using the Treasury exchange rate
/// active at the time of the purchase.
/// </summary>
public sealed class GetPurchaseInCurrencyQueryHandler
    : IRequestHandler<GetPurchaseInCurrencyQuery, GetPurchaseInCurrencyResponse>
{
    private readonly IPurchaseTransactionRepository _repository;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly ILogger<GetPurchaseInCurrencyQueryHandler> _logger;

    public GetPurchaseInCurrencyQueryHandler(
        IPurchaseTransactionRepository repository,
        IExchangeRateService exchangeRateService,
        ILogger<GetPurchaseInCurrencyQueryHandler> logger)
    {
        _repository = repository;
        _exchangeRateService = exchangeRateService;
        _logger = logger;
    }

    public async Task<GetPurchaseInCurrencyResponse> Handle(
        GetPurchaseInCurrencyQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving transaction {TransactionId} in currency {TargetCurrency}",
            request.TransactionId,
            request.TargetCurrency);

        var transaction = await _repository.GetByIdAsync(request.TransactionId, cancellationToken)
            ?? throw new TransactionNotFoundException(request.TransactionId);

        var currency = request.TargetCurrency.ToUpperInvariant();

        var exchangeRate = await _exchangeRateService.GetRateAsync(
            currency, transaction.TransactionDate, cancellationToken);

        if (exchangeRate is null)
        {
            _logger.LogWarning(
                "No exchange rate found for {Currency} within 6 months of {PurchaseDate}. TransactionId: {TransactionId}",
                currency, transaction.TransactionDate, request.TransactionId);

            throw new CurrencyConversionUnavailableException(currency, transaction.TransactionDate);
        }

        var convertedAmount = Math.Round(
            transaction.AmountUsd * exchangeRate.Value, 2, MidpointRounding.AwayFromZero);

        _logger.LogInformation(
            "Conversion complete. TransactionId: {TransactionId}, Rate: {ExchangeRate}, Converted: {ConvertedAmount} {Currency}",
            request.TransactionId, exchangeRate.Value, convertedAmount, currency);

        return new GetPurchaseInCurrencyResponse(
            transaction.Id,
            transaction.Description,
            transaction.TransactionDate,
            transaction.AmountUsd,
            currency,
            exchangeRate.Value,
            convertedAmount);
    }
}
