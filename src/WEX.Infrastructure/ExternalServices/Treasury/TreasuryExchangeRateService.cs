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
        string currency,
        DateOnly purchaseDate,
        CancellationToken cancellationToken)
    {
        // The requirement states: use rate ≤ purchaseDate within the last 6 months
        var cutoffDate = purchaseDate.AddMonths(-LookbackMonths);

        // Treasury API query:
        // - filter[currency][eq]=<currency>          — match exact currency name
        // - filter[record_date][lte]=<purchaseDate>  — on or before purchase date
        // - filter[record_date][gte]=<cutoffDate>    — within 6 months
        // - sort=-record_date                         — most recent first
        // - page[size]=1                              — we only need the closest rate
        var url = $"{_options.ExchangeRateEndpoint}" +
                  $"?filter[currency][eq]={Uri.EscapeDataString(currency)}" +
                  $"&filter[record_date][lte]={purchaseDate:yyyy-MM-dd}" +
                  $"&filter[record_date][gte]={cutoffDate:yyyy-MM-dd}" +
                  $"&sort=-record_date" +
                  $"&page[size]=1";

        _logger.LogInformation(
            "Fetching exchange rate from Treasury API. Currency: {Currency}, PurchaseDate: {PurchaseDate}",
            currency, purchaseDate);

        try
        {
            var response = await _httpClient.GetFromJsonAsync<TreasuryRatesResponse>(
                url, cancellationToken);

            var record = response?.Data.FirstOrDefault();

            if (record is null)
            {
                _logger.LogWarning(
                    "No exchange rate returned from Treasury API. Currency: {Currency}, PurchaseDate: {PurchaseDate}",
                    currency, purchaseDate);
                return null;
            }

            if (!decimal.TryParse(record.ExchangeRate, out var rate))
            {
                _logger.LogError(
                    "Treasury API returned unparseable exchange rate '{RateString}'. Currency: {Currency}",
                    record.ExchangeRate, currency);
                return null;
            }

            _logger.LogInformation(
                "Exchange rate retrieved. Currency: {Currency}, Rate: {Rate}, RecordDate: {RecordDate}",
                currency, rate, record.RecordDate);

            return rate;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "HTTP error fetching exchange rate. Currency: {Currency}, PurchaseDate: {PurchaseDate}",
                currency, purchaseDate);
            throw;
        }
    }
}
