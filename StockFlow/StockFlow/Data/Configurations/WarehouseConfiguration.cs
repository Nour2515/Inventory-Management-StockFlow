using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Models;

namespace StockFlow.Data.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.HasKey(warehouse => warehouse.Id);

        builder.Property(warehouse => warehouse.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(warehouse => warehouse.Location)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(warehouse => warehouse.IsActive)
            .IsRequired();

        builder.Property(warehouse => warehouse.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }
}
