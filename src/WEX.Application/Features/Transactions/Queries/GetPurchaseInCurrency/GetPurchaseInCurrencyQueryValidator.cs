using FluentValidation;

namespace WEX.Application.Features.Transactions.Queries.GetPurchaseInCurrency;

/// <summary>
/// Validates inputs for <see cref="GetPurchaseInCurrencyQuery"/>.
/// </summary>
public sealed class GetPurchaseInCurrencyQueryValidator
    : AbstractValidator<GetPurchaseInCurrencyQuery>
{
    public GetPurchaseInCurrencyQueryValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty()
                .WithMessage("Transaction ID is required.");

        RuleFor(x => x.TargetCurrency)
            .NotEmpty()
                .WithMessage("Target currency is required.")
            .Length(3)
                .WithMessage("Target currency must be a 3-letter ISO 4217 code.")
            .Matches("^[A-Za-z]{3}$")
                .WithMessage("Target currency must contain only letters.");
    }
}
