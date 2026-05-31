using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WEX.Infrastructure.Persistence;

namespace WEX.API.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory that replaces PostgreSQL with in-memory DB.
/// Test JWT config and users are provided via appsettings.Testing.json.
/// </summary>
public class WexApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace real PostgreSQL DbContext with in-memory for tests
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("WexTestDb_" + Guid.NewGuid()));
        });
    }
}
