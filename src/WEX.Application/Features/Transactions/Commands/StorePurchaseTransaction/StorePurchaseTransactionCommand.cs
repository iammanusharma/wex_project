using MediatR;

namespace WEX.Application.Features.Transactions.Commands.StorePurchaseTransaction;

/// <summary>
/// Command to store a new purchase transaction. Returns the assigned unique identifier.
/// </summary>
public sealed record StorePurchaseTransactionCommand(
    string Description,
    DateOnly TransactionDate,
    decimal AmountUsd) : IRequest<Guid>;
