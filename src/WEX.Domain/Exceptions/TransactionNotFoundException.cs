namespace WEX.Domain.Exceptions;

/// <summary>
/// Thrown when a requested purchase transaction does not exist.
/// Maps to HTTP 404 Not Found.
/// </summary>
public sealed class TransactionNotFoundException : Exception
{
    public Guid TransactionId { get; }

    public TransactionNotFoundException(Guid transactionId)
        : base($"Purchase transaction with ID '{transactionId}' was not found.")
    {
        TransactionId = transactionId;
    }
}
