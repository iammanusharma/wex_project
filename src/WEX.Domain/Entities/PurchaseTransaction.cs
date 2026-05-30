namespace WEX.Domain.Entities;

/// <summary>
/// Represents a purchase transaction stored in the system.
/// Enforces domain invariants at construction time.
/// </summary>
public sealed class PurchaseTransaction
{
    public const int MaxDescriptionLength = 50;

    public Guid Id { get; private set; }
    public string Description { get; private set; }
    public DateOnly TransactionDate { get; private set; }
    public decimal AmountUsd { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PurchaseTransaction() { }  // EF Core constructor

    /// <summary>
    /// Factory method — the only way to create a valid PurchaseTransaction.
    /// </summary>
    public static PurchaseTransaction Create(
        string description,
        DateOnly transactionDate,
        decimal amountUsd)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        if (description.Length > MaxDescriptionLength)
        {
            throw new ArgumentException(
                $"Description must not exceed {MaxDescriptionLength} characters.", nameof(description));
        }

        if (amountUsd <= 0)
        {
            throw new ArgumentException("Purchase amount must be a positive value.", nameof(amountUsd));
        }

        return new PurchaseTransaction
        {
            Id = Guid.NewGuid(),
            Description = description.Trim(),
            TransactionDate = transactionDate,
            AmountUsd = Math.Round(amountUsd, 2, MidpointRounding.AwayFromZero),
            CreatedAt = DateTime.UtcNow
        };
    }
}
