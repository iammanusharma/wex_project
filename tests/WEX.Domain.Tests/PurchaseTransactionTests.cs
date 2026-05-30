using FluentAssertions;
using WEX.Domain.Entities;
using WEX.Domain.Exceptions;

namespace WEX.Domain.Tests;

public class PurchaseTransactionTests
{
    [Fact]
    public void Create_ValidInputs_ReturnsTransactionWithCorrectValues()
    {
        // Arrange
        var description = "Office supplies";
        var date = new DateOnly(2024, 6, 15);
        var amount = 99.999m;

        // Act
        var transaction = PurchaseTransaction.Create(description, date, amount);

        // Assert
        transaction.Id.Should().NotBeEmpty();
        transaction.Description.Should().Be(description);
        transaction.TransactionDate.Should().Be(date);
        transaction.AmountUsd.Should().Be(100.00m); // rounded to nearest cent
        transaction.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_DescriptionExceeds50Chars_ThrowsArgumentException()
    {
        // Arrange
        var longDescription = new string('A', 51);

        // Act
        var act = () => PurchaseTransaction.Create(longDescription, DateOnly.FromDateTime(DateTime.Today), 10m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*50 characters*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespaceDescription_ThrowsArgumentException(string? description)
    {
        // Act
        var act = () => PurchaseTransaction.Create(description!, DateOnly.FromDateTime(DateTime.Today), 10m);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Create_NonPositiveAmount_ThrowsArgumentException(decimal amount)
    {
        // Act
        var act = () => PurchaseTransaction.Create("Valid desc", DateOnly.FromDateTime(DateTime.Today), amount);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*positive*");
    }

    [Theory]
    [InlineData(10.004, 10.00)]   // round down
    [InlineData(10.005, 10.01)]   // round up (AwayFromZero)
    [InlineData(10.999, 11.00)]
    public void Create_AmountWithExtraDecimals_IsRoundedToNearestCent(decimal input, decimal expected)
    {
        // Act
        var transaction = PurchaseTransaction.Create("Test", DateOnly.FromDateTime(DateTime.Today), input);

        // Assert
        transaction.AmountUsd.Should().Be(expected);
    }

    [Fact]
    public void Create_DescriptionExactly50Chars_Succeeds()
    {
        // Arrange
        var description = new string('A', 50);

        // Act
        var act = () => PurchaseTransaction.Create(description, DateOnly.FromDateTime(DateTime.Today), 1m);

        // Assert
        act.Should().NotThrow();
    }
}
