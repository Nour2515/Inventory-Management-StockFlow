using StockFlow.Models.Enums;

namespace StockFlow.Models;

//for history 
public class InventoryTransaction
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public InventoryTransactionType Type { get; set; }
    public int Quantity { get; set; }
    public string? ReferenceId { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
}
