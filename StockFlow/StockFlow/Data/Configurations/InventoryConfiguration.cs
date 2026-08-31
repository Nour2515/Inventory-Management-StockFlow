using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Models;
using System.Reflection.Emit;

namespace StockFlow.Data.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("Inventories");

        builder.HasKey(inventory => inventory.Id);

        builder.Property(inventory => inventory.OnHandQuantity)
            .IsRequired();

        builder.Property(inventory => inventory.ReservedQuantity)
            .IsRequired();

        builder.Property(inventory => inventory.ReorderLevel)
            .IsRequired();

        builder.Property(inventory => inventory.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(i => i.RowVersion)
            .IsRowVersion().
            IsConcurrencyToken();

        builder.HasIndex(inventory => new { inventory.ProductId, inventory.WarehouseId })
            .IsUnique();

        builder.HasOne(inventory => inventory.Product)
            .WithMany(product => product.Inventories)
            .HasForeignKey(inventory => inventory.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(inventory => inventory.Warehouse)
            .WithMany(warehouse => warehouse.Inventories)
            .HasForeignKey(inventory => inventory.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
