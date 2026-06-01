using FluentValidation;
using WEX.Domain.Entities;

namespace WEX.Application.Features.Transactions.Commands.StorePurchaseTransaction;

/// <summary>
/// Validates all inputs for <see cref="StorePurchaseTransactionCommand"/> before the handler runs.
/// </summary>
public sealed class StorePurchaseTransactionCommandValidator
    : AbstractValidator<StorePurchaseTransactionCommand>
{
    public StorePurchaseTransactionCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
                .WithMessage("Description is required.")
            .MaximumLength(PurchaseTransaction.MaxDescriptionLength)
                .WithMessage($"Description must not exceed {PurchaseTransaction.MaxDescriptionLength} characters.");

        RuleFor(x => x.TransactionDate)
            .NotEmpty()
                .WithMessage("Transaction date is required.");

        RuleFor(x => x.AmountUsd)
            .GreaterThan(0)
                .WithMessage("Purchase amount must be a positive value.");
    }
}
