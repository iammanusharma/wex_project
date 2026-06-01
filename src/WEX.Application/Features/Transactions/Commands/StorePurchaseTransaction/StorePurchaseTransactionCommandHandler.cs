using MediatR;
using Microsoft.Extensions.Logging;
using WEX.Domain.Entities;
using WEX.Domain.Interfaces;

namespace WEX.Application.Features.Transactions.Commands.StorePurchaseTransaction;

/// <summary>
/// Handles <see cref="StorePurchaseTransactionCommand"/>:
/// creates and persists a new purchase transaction, returning its unique ID.
/// </summary>
public sealed class StorePurchaseTransactionCommandHandler
    : IRequestHandler<StorePurchaseTransactionCommand, Guid>
{
    private readonly IPurchaseTransactionRepository _repository;
    private readonly ILogger<StorePurchaseTransactionCommandHandler> _logger;

    public StorePurchaseTransactionCommandHandler(
        IPurchaseTransactionRepository repository,
        ILogger<StorePurchaseTransactionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Guid> Handle(
        StorePurchaseTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var roundedAmount = decimal.Round(request.AmountUsd, 2, MidpointRounding.AwayFromZero);

        _logger.LogInformation(
            "Storing purchase transaction. Description: {Description}, Date: {TransactionDate}, Amount: {AmountUsd} USD (rounded from {OriginalAmount})",
            request.Description,
            request.TransactionDate,
            roundedAmount,
            request.AmountUsd);

        var transaction = PurchaseTransaction.Create(
            request.Description,
            request.TransactionDate,
            roundedAmount);

        await _repository.AddAsync(transaction, cancellationToken);

        _logger.LogInformation(
            "Purchase transaction stored successfully. TransactionId: {TransactionId}",
            transaction.Id);

        return transaction.Id;
    }
}
