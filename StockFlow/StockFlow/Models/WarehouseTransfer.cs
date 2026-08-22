using StockFlow.Models.Enums;

namespace StockFlow.Models;

public class WarehouseTransfer
{
    public int Id { get; set; }
    public int FromWarehouseId { get; set; }
    public int ToWarehouseId { get; set; }
    public WarehouseTransferStatus Status { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Warehouse FromWarehouse { get; set; } = null!;
    public Warehouse ToWarehouse { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<WarehouseTransferItem> WarehouseTransferItems { get; set; } = new List<WarehouseTransferItem>();
}
