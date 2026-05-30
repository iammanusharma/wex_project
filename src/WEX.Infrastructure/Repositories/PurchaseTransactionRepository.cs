using Microsoft.EntityFrameworkCore;
using WEX.Domain.Entities;
using WEX.Domain.Interfaces;
using WEX.Infrastructure.Persistence;

namespace WEX.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IPurchaseTransactionRepository"/>.
/// </summary>
public sealed class PurchaseTransactionRepository : IPurchaseTransactionRepository
{
    private readonly AppDbContext _dbContext;

    public PurchaseTransactionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PurchaseTransaction transaction, CancellationToken cancellationToken = default)
    {
        await _dbContext.PurchaseTransactions.AddAsync(transaction, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PurchaseTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PurchaseTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }
}
