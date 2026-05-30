using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WEX.Domain.Interfaces;
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
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName)));

        services.AddScoped<IPurchaseTransactionRepository, PurchaseTransactionRepository>();

        services.AddMemoryCache();

        return services;
    }
}
