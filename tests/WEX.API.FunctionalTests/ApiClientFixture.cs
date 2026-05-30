using Microsoft.Extensions.Configuration;

namespace WEX.API.FunctionalTests;

/// <summary>
/// Provides a shared HttpClient configured against the running API.
/// Base URL is read from appsettings.FunctionalTests.json or the
/// FUNCTIONAL_TEST_BASE_URL environment variable (used in CI pipelines).
/// </summary>
public sealed class ApiClientFixture : IDisposable
{
    public HttpClient Client { get; }
    public string BaseUrl { get; }

    public ApiClientFixture()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.FunctionalTests.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        BaseUrl = Environment.GetEnvironmentVariable("FUNCTIONAL_TEST_BASE_URL")
            ?? config["FunctionalTests:BaseUrl"]
            ?? "http://localhost:5000";

        var timeout = int.TryParse(config["FunctionalTests:TimeoutSeconds"], out var t) ? t : 30;

        Client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(timeout)
        };
    }

    public void Dispose() => Client.Dispose();
}
