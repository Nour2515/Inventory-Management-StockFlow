using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Models;

namespace StockFlow.Data.Configurations;

public class WarehouseTransferItemConfiguration : IEntityTypeConfiguration<WarehouseTransferItem>
{
    public void Configure(EntityTypeBuilder<WarehouseTransferItem> builder)
    {
        builder.ToTable("WarehouseTransferItems");

        builder.HasKey(warehouseTransferItem => warehouseTransferItem.Id);

        builder.Property(warehouseTransferItem => warehouseTransferItem.Quantity)
            .IsRequired();

        builder.HasOne(warehouseTransferItem => warehouseTransferItem.WarehouseTransfer)
            .WithMany(warehouseTransfer => warehouseTransfer.WarehouseTransferItems)
            .HasForeignKey(warehouseTransferItem => warehouseTransferItem.WarehouseTransferId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(warehouseTransferItem => warehouseTransferItem.Product)
            .WithMany(product => product.WarehouseTransferItems)
            .HasForeignKey(warehouseTransferItem => warehouseTransferItem.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
