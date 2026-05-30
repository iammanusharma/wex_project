namespace WEX.Infrastructure.ExternalServices.Treasury;

/// <summary>
/// Strongly-typed configuration for the Treasury Reporting Rates of Exchange API.
/// Bound from the "TreasuryApi" section in appsettings.json.
/// Using the Options pattern keeps config strongly-typed and testable.
/// </summary>
public sealed class TreasuryApiOptions
{
    public const string SectionName = "TreasuryApi";

    public string BaseUrl { get; init; } = string.Empty;
    public string ExchangeRateEndpoint { get; init; } = string.Empty;
    public int CacheDurationMinutes { get; init; } = 60;
    public int RetryCount { get; init; } = 3;
    public int RetryDelaySeconds { get; init; } = 2;
    public int TimeoutSeconds { get; init; } = 30;
}
