using StockFlow.Models.Enums;

namespace StockFlow.Models;

public class StockReservation
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int Quantity { get; set; }
    public DateTime ExpiresAt { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
}
