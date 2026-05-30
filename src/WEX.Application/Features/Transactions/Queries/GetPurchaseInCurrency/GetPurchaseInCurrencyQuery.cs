using MediatR;

namespace WEX.Application.Features.Transactions.Queries.GetPurchaseInCurrency;

/// <summary>
/// Query to retrieve a stored purchase transaction converted to a target currency.
/// </summary>
public sealed record GetPurchaseInCurrencyQuery(
    Guid TransactionId,
    string TargetCurrency) : IRequest<GetPurchaseInCurrencyResponse>;
