using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WEX.Application.Features.Transactions.Commands.StorePurchaseTransaction;
using WEX.Domain.Entities;
using WEX.Domain.Interfaces;
using Xunit;

namespace WEX.Application.Tests.Features.Transactions;

public class StorePurchaseTransactionCommandHandlerTests
{
    private readonly IPurchaseTransactionRepository _repository;
    private readonly ILogger<StorePurchaseTransactionCommandHandler> _logger;
    private readonly StorePurchaseTransactionCommandHandler _handler;

    public StorePurchaseTransactionCommandHandlerTests()
    {
        _repository = Substitute.For<IPurchaseTransactionRepository>();
        _logger = Substitute.For<ILogger<StorePurchaseTransactionCommandHandler>>();
        _handler = new StorePurchaseTransactionCommandHandler(_repository, _logger);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewGuid()
    {
        // Arrange
        var command = new StorePurchaseTransactionCommand(
            Description: "Office supplies",
            TransactionDate: new DateOnly(2024, 6, 15),
            AmountUsd: 49.99m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(
            Arg.Is<PurchaseTransaction>(t =>
                t.Description == "Office supplies" &&
                t.TransactionDate == new DateOnly(2024, 6, 15) &&
                t.AmountUsd == 49.99m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsExactlyOnce()
    {
        // Arrange
        var command = new StorePurchaseTransactionCommand("Test", new DateOnly(2024, 1, 1), 10.00m);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<PurchaseTransaction>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsIdMatchingPersistedTransaction()
    {
        // Arrange
        Guid capturedId = Guid.Empty;
        await _repository.AddAsync(
            Arg.Do<PurchaseTransaction>(t => capturedId = t.Id),
            Arg.Any<CancellationToken>());

        var command = new StorePurchaseTransactionCommand("Test", new DateOnly(2024, 1, 1), 10.00m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(capturedId);
    }
}
