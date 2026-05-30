namespace WEX.Domain.Interfaces;

/// <summary>
/// Contract for retrieving exchange rates from the Treasury Reporting Rates of Exchange API.
/// Implemented in Infrastructure; consumed in Application.
/// </summary>
public interface IExchangeRateService
{
    /// <summary>
    /// Returns the exchange rate for the given currency that is less than or equal to
    /// <paramref name="purchaseDate"/> and within the last 6 months,
    /// or null if no qualifying rate exists.
    /// </summary>
    Task<decimal?> GetRateAsync(
        string currency,
        DateOnly purchaseDate,
        CancellationToken cancellationToken = default);
}
