using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Models;

namespace StockFlow.Data.Configurations;

public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("StockReservations");

        builder.HasKey(stockReservation => stockReservation.Id);

        builder.Property(stockReservation => stockReservation.Quantity)
            .IsRequired();

        builder.Property(stockReservation => stockReservation.ExpiresAt)
            .IsRequired();

        builder.Property(stockReservation => stockReservation.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(stockReservation => stockReservation.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(stockReservation => stockReservation.Order)
            .WithMany(order => order.StockReservations)
            .HasForeignKey(stockReservation => stockReservation.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(stockReservation => stockReservation.Product)
            .WithMany(product => product.StockReservations)
            .HasForeignKey(stockReservation => stockReservation.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(stockReservation => stockReservation.Warehouse)
            .WithMany(warehouse => warehouse.StockReservations)
            .HasForeignKey(stockReservation => stockReservation.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
