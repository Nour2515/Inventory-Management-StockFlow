using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Models;

namespace StockFlow.Data.Configurations;

public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions");

        builder.HasKey(inventoryTransaction => inventoryTransaction.Id);

        builder.Property(inventoryTransaction => inventoryTransaction.Type)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(inventoryTransaction => inventoryTransaction.Quantity)
            .IsRequired();

        builder.Property(inventoryTransaction => inventoryTransaction.ReferenceId)
            .HasMaxLength(100);

        builder.Property(inventoryTransaction => inventoryTransaction.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(inventoryTransaction => new
        {
            inventoryTransaction.ProductId,
            inventoryTransaction.WarehouseId,
            inventoryTransaction.CreatedAt
        });

        builder.HasOne(inventoryTransaction => inventoryTransaction.Product)
            .WithMany(product => product.InventoryTransactions)
            .HasForeignKey(inventoryTransaction => inventoryTransaction.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(inventoryTransaction => inventoryTransaction.Warehouse)
            .WithMany(warehouse => warehouse.InventoryTransactions)
            .HasForeignKey(inventoryTransaction => inventoryTransaction.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(inventoryTransaction => inventoryTransaction.CreatedByUser)
            .WithMany(user => user.InventoryTransactions)
            .HasForeignKey(inventoryTransaction => inventoryTransaction.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
