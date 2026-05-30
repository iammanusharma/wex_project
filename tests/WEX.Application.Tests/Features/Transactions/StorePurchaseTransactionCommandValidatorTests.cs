using FluentAssertions;
using FluentValidation;
using WEX.Application.Features.Transactions.Commands.StorePurchaseTransaction;
using Xunit;

namespace WEX.Application.Tests.Features.Transactions;

public class StorePurchaseTransactionCommandValidatorTests
{
    private readonly StorePurchaseTransactionCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new StorePurchaseTransactionCommand("Office supplies", new DateOnly(2024, 6, 1), 49.99m);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Validate_EmptyDescription_FailsWithRequiredMessage(string description)
    {
        // Arrange
        var command = new StorePurchaseTransactionCommand(description, new DateOnly(2024, 1, 1), 10.00m);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_DescriptionExceeds50Chars_FailsWithLengthMessage()
    {
        // Arrange
        var command = new StorePurchaseTransactionCommand(
            new string('A', 51), new DateOnly(2024, 1, 1), 10.00m);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description" && e.ErrorMessage.Contains("50"));
    }

    [Fact]
    public void Validate_DescriptionExactly50Chars_PassesValidation()
    {
        // Arrange
        var command = new StorePurchaseTransactionCommand(
            new string('A', 50), new DateOnly(2024, 1, 1), 10.00m);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Validate_NonPositiveAmount_FailsWithPositiveMessage(decimal amount)
    {
        // Arrange
        var command = new StorePurchaseTransactionCommand("Test", new DateOnly(2024, 1, 1), amount);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AmountUsd" && e.ErrorMessage.Contains("positive"));
    }

    [Fact]
    public void Validate_AmountWithMoreThan2DecimalPlaces_FailsWithRoundingMessage()
    {
        // Arrange
        var command = new StorePurchaseTransactionCommand("Test", new DateOnly(2024, 1, 1), 10.001m);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AmountUsd" && e.ErrorMessage.Contains("cent"));
    }
}
