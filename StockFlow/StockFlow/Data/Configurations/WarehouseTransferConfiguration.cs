using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Models;

namespace StockFlow.Data.Configurations;

public class WarehouseTransferConfiguration : IEntityTypeConfiguration<WarehouseTransfer>
{
    public void Configure(EntityTypeBuilder<WarehouseTransfer> builder)
    {
        builder.ToTable("WarehouseTransfers");

        builder.HasKey(warehouseTransfer => warehouseTransfer.Id);

        builder.Property(warehouseTransfer => warehouseTransfer.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(warehouseTransfer => warehouseTransfer.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(warehouseTransfer => warehouseTransfer.CompletedAt);

        builder.HasOne(warehouseTransfer => warehouseTransfer.FromWarehouse)
            .WithMany(warehouse => warehouse.OutgoingTransfers)
            .HasForeignKey(warehouseTransfer => warehouseTransfer.FromWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(warehouseTransfer => warehouseTransfer.ToWarehouse)
            .WithMany(warehouse => warehouse.IncomingTransfers)
            .HasForeignKey(warehouseTransfer => warehouseTransfer.ToWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(warehouseTransfer => warehouseTransfer.CreatedByUser)
            .WithMany(user => user.WarehouseTransfers)
            .HasForeignKey(warehouseTransfer => warehouseTransfer.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
