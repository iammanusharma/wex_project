using WEX.Domain.Entities;

namespace WEX.Domain.Interfaces;

/// <summary>
/// Persistence contract for PurchaseTransaction.
/// Implemented in Infrastructure; consumed in Application.
/// </summary>
public interface IPurchaseTransactionRepository
{
    /// <summary>Persist a new purchase transaction.</summary>
    Task AddAsync(PurchaseTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>Retrieve a transaction by its unique ID, or null if not found.</summary>
    Task<PurchaseTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
