using System.Text.Json.Serialization;

namespace WEX.Infrastructure.ExternalServices.Treasury;

/// <summary>
/// Internal DTOs that match the Treasury Reporting Rates of Exchange API response shape.
/// Kept internal to Infrastructure — no other layer should depend on external API contracts.
/// </summary>
internal sealed class TreasuryRatesResponse
{
    [JsonPropertyName("data")]
    public List<TreasuryRateRecord> Data { get; init; } = [];
}

internal sealed class TreasuryRateRecord
{
    /// <summary>Combined country + currency description, e.g. "Euro Zone-Euro"</summary>
    [JsonPropertyName("country_currency_desc")]
    public string CountryCurrencyDesc { get; init; } = string.Empty;

    /// <summary>Currency name, e.g. "Euro"</summary>
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    /// <summary>Country name</summary>
    [JsonPropertyName("country")]
    public string Country { get; init; } = string.Empty;

    /// <summary>Exchange rate as a string from the API, e.g. "1.234"</summary>
    [JsonPropertyName("exchange_rate")]
    public string ExchangeRate { get; init; } = string.Empty;

    /// <summary>Date the rate was recorded, format "YYYY-MM-DD"</summary>
    [JsonPropertyName("record_date")]
    public string RecordDate { get; init; } = string.Empty;
}
