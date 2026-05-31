using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using WEX.Application.Interfaces;
using WEX.Domain.Interfaces;
using WEX.Infrastructure.Auth;
using WEX.Infrastructure.ExternalServices.Treasury;
using WEX.Infrastructure.Persistence;
using WEX.Infrastructure.Repositories;

namespace WEX.Infrastructure;

/// <summary>
/// Registers all Infrastructure layer services into the DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // --- Database ---
        // EF Core with Npgsql; migrations assembly set to Infrastructure
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName)));

        services.AddScoped<IPurchaseTransactionRepository, PurchaseTransactionRepository>();

        // --- Auth ---
        // Bind JWT options and register token + user services
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IUserStore, InMemoryUserStore>();

        // --- Caching ---
        // IMemoryCache used by TreasuryExchangeRateService to avoid repeated API calls
        services.AddMemoryCache();

        // --- Treasury API (Options + HttpClient + Polly) ---
        // Bind typed options from appsettings "TreasuryApi" section
        services.Configure<TreasuryApiOptions>(
            configuration.GetSection(TreasuryApiOptions.SectionName));

        var treasuryOptions = configuration
            .GetSection(TreasuryApiOptions.SectionName)
            .Get<TreasuryApiOptions>() ?? new TreasuryApiOptions();

        // Register as typed HttpClient — IHttpClientFactory manages connection pooling
        // Polly retry: exponential backoff (2s, 4s, 8s) on transient HTTP errors
        services.AddHttpClient<IExchangeRateService, TreasuryExchangeRateService>(client =>
        {
            client.BaseAddress = new Uri(treasuryOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(treasuryOptions.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddPolicyHandler(GetRetryPolicy(treasuryOptions.RetryCount, treasuryOptions.RetryDelaySeconds));

        return services;
    }

    /// <summary>
    /// Polly retry policy: retries on 5xx and network errors with exponential backoff.
    /// Exponential backoff (2^attempt * delay) prevents thundering-herd on API outages.
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount, int delaySeconds) =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount,
                attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt) * delaySeconds));
}

