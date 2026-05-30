using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEX.Domain.Entities;

namespace WEX.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core entity type configuration for <see cref="PurchaseTransaction"/>.
/// </summary>
public sealed class PurchaseTransactionConfiguration
    : IEntityTypeConfiguration<PurchaseTransaction>
{
    public void Configure(EntityTypeBuilder<PurchaseTransaction> builder)
    {
        builder.ToTable("purchase_transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // ID assigned by domain, not DB

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(PurchaseTransaction.MaxDescriptionLength)
            .IsRequired();

        builder.Property(t => t.TransactionDate)
            .HasColumnName("transaction_date")
            .IsRequired();

        builder.Property(t => t.AmountUsd)
            .HasColumnName("amount_usd")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
