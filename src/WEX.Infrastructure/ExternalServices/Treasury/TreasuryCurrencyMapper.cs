namespace WEX.Infrastructure.ExternalServices.Treasury;

/// <summary>
/// Maps ISO 4217 currency codes to the Treasury Reporting Rates of Exchange
/// "country_currency_desc" field values (e.g. EUR → "Euro Zone-Euro").
///
/// Why needed: The Treasury API does not accept ISO codes — it uses descriptive names.
/// This mapper is the single place to maintain that translation.
/// </summary>
internal static class TreasuryCurrencyMapper
{
    // Source: https://fiscaldata.treasury.gov/api/v1/accounting/od/rates_of_exchange
    // Verify exact strings by querying: ?fields=country_currency_desc&page[size]=200
    private static readonly Dictionary<string, string> IsoToTreasury =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["EUR"] = "Euro Zone-Euro",
            ["GBP"] = "United Kingdom-Pound",
            ["CAD"] = "Canada-Dollar",
            ["AUD"] = "Australia-Dollar",
            ["NZD"] = "New Zealand-Dollar",
            ["JPY"] = "Japan-Yen",
            ["CHF"] = "Switzerland-Franc",
            ["CNY"] = "China-Renminbi",
            ["HKD"] = "Hong Kong-Dollar",
            ["SGD"] = "Singapore-Dollar",
            ["MXN"] = "Mexico-Peso",
            ["BRL"] = "Brazil-Real",
            ["INR"] = "India-Rupee",
            ["KRW"] = "South Korea-Won",
            ["SEK"] = "Sweden-Krona",
            ["NOK"] = "Norway-Krone",
            ["DKK"] = "Denmark-Krone",
            ["ZAR"] = "South Africa-Rand",
            ["THB"] = "Thailand-Baht",
            ["MYR"] = "Malaysia-Ringgit",
            ["PHP"] = "Philippines-Peso",
            ["IDR"] = "Indonesia-Rupiah",
            ["TWD"] = "Taiwan-Dollar",
            ["SAR"] = "Saudi Arabia-Riyal",
            ["AED"] = "United Arab Emirates-Dirham",
            ["TRY"] = "Turkey-Lira",
            ["PLN"] = "Poland-Zloty",
            ["CZK"] = "Czech Republic-Koruna",
            ["HUF"] = "Hungary-Forint",
            ["ILS"] = "Israel-Shekel",
            ["CLP"] = "Chile-Peso",
            ["COP"] = "Colombia-Peso",
            ["PEN"] = "Peru-Sol",
            ["VND"] = "Vietnam-Dong",
            ["PKR"] = "Pakistan-Rupee",
            ["EGP"] = "Egypt-Pound",
            ["NGN"] = "Nigeria-Naira",
            ["KES"] = "Kenya-Shilling",
        };

    /// <summary>
    /// Returns the Treasury country_currency_desc for the given ISO 4217 code,
    /// or <c>null</c> if the currency is not in the supported set.
    /// </summary>
    public static string? GetTreasuryName(string isoCode) =>
        IsoToTreasury.TryGetValue(isoCode, out var name) ? name : null;

    /// <summary>Returns all supported ISO codes.</summary>
    public static IReadOnlyCollection<string> SupportedCodes => IsoToTreasury.Keys.ToList();
}
