namespace WEX.Domain.Exceptions;

/// <summary>
/// Thrown when no exchange rate can be found within 6 months of the purchase date
/// for the requested currency. Maps to HTTP 422 Unprocessable Entity.
/// </summary>
public sealed class CurrencyConversionUnavailableException : Exception
{
    public string Currency { get; }
    public DateOnly PurchaseDate { get; }

    public CurrencyConversionUnavailableException(string currency, DateOnly purchaseDate)
        : base($"No exchange rate available for '{currency}' within 6 months of {purchaseDate:yyyy-MM-dd}. " +
               $"The purchase cannot be converted to the target currency.")
    {
        Currency = currency;
        PurchaseDate = purchaseDate;
    }
}
