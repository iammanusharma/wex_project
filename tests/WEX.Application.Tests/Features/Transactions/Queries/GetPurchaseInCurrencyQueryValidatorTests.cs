using FluentAssertions;
using WEX.Application.Features.Transactions.Queries.GetPurchaseInCurrency;
using Xunit;

namespace WEX.Application.Tests.Features.Transactions.Queries;

/// <summary>
/// Unit tests for <see cref="GetPurchaseInCurrencyQueryValidator"/>.
/// Validates all boundary conditions for the query inputs.
/// </summary>
public sealed class GetPurchaseInCurrencyQueryValidatorTests
{
    private readonly GetPurchaseInCurrencyQueryValidator _validator = new();

    [Fact]
    public void Validate_ValidQuery_PassesValidation()
    {
        var query = new GetPurchaseInCurrencyQuery(Guid.NewGuid(), "EUR");
        var result = _validator.Validate(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyTransactionId_FailsValidation()
    {
        var query = new GetPurchaseInCurrencyQuery(Guid.Empty, "EUR");
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "TransactionId");
    }

    [Fact]
    public void Validate_EmptyCurrency_FailsValidation()
    {
        var query = new GetPurchaseInCurrencyQuery(Guid.NewGuid(), "");
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TargetCurrency");
    }

    [Theory]
    [InlineData("EU")]     // too short
    [InlineData("EURO")]   // too long
    [InlineData("12E")]    // contains digits
    public void Validate_InvalidCurrencyCode_FailsValidation(string currency)
    {
        var query = new GetPurchaseInCurrencyQuery(Guid.NewGuid(), currency);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("CAD")]
    [InlineData("JPY")]
    public void Validate_ValidIsoCurrencyCode_PassesValidation(string currency)
    {
        var query = new GetPurchaseInCurrencyQuery(Guid.NewGuid(), currency);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeTrue();
    }
}
