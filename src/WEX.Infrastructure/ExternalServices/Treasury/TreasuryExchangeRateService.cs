using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using WEX.Domain.Interfaces;

namespace WEX.Infrastructure.ExternalServices.Treasury;

/// <summary>
/// Retrieves exchange rates from the US Treasury Reporting Rates of Exchange API.
///
/// Design decisions:
/// - IMemoryCache: avoids repeated calls for the same currency/date pair (60-min TTL by default)
/// - Polly retry: handles transient HTTP failures with exponential backoff
/// - 6-month window: per requirement, no rate found within 6 months → returns null
/// - Returns null (not throws) so the handler decides how to respond
/// </summary>
public sealed class TreasuryExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TreasuryExchangeRateService> _logger;
    private readonly TreasuryApiOptions _options;

    // The Treasury API uses currency names not ISO codes (e.g. "Euro", "Canadian Dollar")
    // We query by currency name — the API supports filtering via query params
    private const int LookbackMonths = 6;

    public TreasuryExchangeRateService(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<TreasuryApiOptions> options,
        ILogger<TreasuryExchangeRateService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<decimal?> GetRateAsync(
        string currency,
        DateOnly purchaseDate,
        CancellationToken cancellationToken = default)
    {
        // Cache key = currency + date so we don't re-fetch the same combination
        var cacheKey = $"exchange_rate_{currency}_{purchaseDate:yyyy-MM-dd}";

        if (_cache.TryGetValue(cacheKey, out decimal? cachedRate))
        {
            _logger.LogDebug(
                "Cache hit for exchange rate. Currency: {Currency}, Date: {PurchaseDate}",
                currency, purchaseDate);
            return cachedRate;
        }

        var rate = await FetchRateFromApiAsync(currency, purchaseDate, cancellationToken);

        // Cache both hits (rate found) and misses (null) to avoid hammering the API
        _cache.Set(cacheKey, rate, TimeSpan.FromMinutes(_options.CacheDurationMinutes));

        return rate;
    }

    private async Task<decimal?> FetchRateFromApiAsync(
        string isoCode,
        DateOnly purchaseDate,
        CancellationToken cancellationToken)
    {
        // Map ISO code (e.g. "EUR") → Treasury name (e.g. "Euro Zone-Euro")
        var treasuryName = TreasuryCurrencyMapper.GetTreasuryName(isoCode);
        if (treasuryName is null)
        {
            _logger.LogWarning(
                "Currency {IsoCode} is not in the supported Treasury currency list",
                isoCode);
            return null;
        }

        // The requirement states: use rate ≤ purchaseDate within the last 6 months
        var cutoffDate = purchaseDate.AddMonths(-LookbackMonths);

        // Treasury API filter syntax: filter=field:op:value,field:op:value
        // Commas separate multiple conditions (AND logic)
        var url = $"{_options.ExchangeRateEndpoint}" +
                  $"?filter=country_currency_desc:eq:{Uri.EscapeDataString(treasuryName)}" +
                  $",record_date:lte:{purchaseDate:yyyy-MM-dd}" +
                  $",record_date:gte:{cutoffDate:yyyy-MM-dd}" +
                  $"&sort=-record_date" +
                  $"&page[size]=1";

        _logger.LogInformation(
            "Fetching exchange rate from Treasury API. Currency: {IsoCode} ({TreasuryName}), PurchaseDate: {PurchaseDate}",
            isoCode, treasuryName, purchaseDate);

        try
        {
            var httpResponse = await _httpClient.GetAsync(url, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Treasury API returned {StatusCode}. Currency: {IsoCode}, URL: {Url}, Body: {Body}",
                    (int)httpResponse.StatusCode, isoCode, url, body);
                return null;
            }

            var response = await httpResponse.Content
                .ReadFromJsonAsync<TreasuryRatesResponse>(cancellationToken: cancellationToken);

            var record = response?.Data.FirstOrDefault();

            if (record is null)
            {
                _logger.LogWarning(
                    "No exchange rate returned from Treasury API. Currency: {IsoCode}, PurchaseDate: {PurchaseDate}",
                    isoCode, purchaseDate);
                return null;
            }

            if (!decimal.TryParse(
                    record.ExchangeRate,
                    System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var rate))
            {
                _logger.LogError(
                    "Treasury API returned unparseable exchange rate '{RateString}'. Currency: {IsoCode}",
                    record.ExchangeRate, isoCode);
                return null;
            }

            _logger.LogInformation(
                "Exchange rate retrieved. Currency: {IsoCode}, Rate: {Rate}, RecordDate: {RecordDate}",
                isoCode, rate, record.RecordDate);

            return rate;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "HTTP error fetching exchange rate. Currency: {IsoCode}, PurchaseDate: {PurchaseDate}",
                isoCode, purchaseDate);
            return null; // treat network errors as unavailable rather than crashing
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex,
                "Failed to deserialize Treasury API response. Currency: {IsoCode}, URL: {Url}",
                isoCode, url);
            return null;
        }
    }
}
