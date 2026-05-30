namespace WEX.Domain.ValueObjects;

/// <summary>
/// Represents a monetary amount in a specific currency, rounded to 2 decimal places.
/// </summary>
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency, nameof(currency));

        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        }

        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.Trim().ToUpperInvariant();
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
