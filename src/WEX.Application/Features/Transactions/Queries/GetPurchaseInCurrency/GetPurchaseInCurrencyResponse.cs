namespace WEX.Application.Features.Transactions.Queries.GetPurchaseInCurrency;

/// <summary>
/// Response DTO for a purchase transaction retrieved with currency conversion applied.
/// </summary>
public sealed record GetPurchaseInCurrencyResponse(
    Guid Id,
    string Description,
    DateOnly TransactionDate,
    decimal OriginalAmountUsd,
    string TargetCurrency,
    decimal ExchangeRate,
    decimal ConvertedAmount);
