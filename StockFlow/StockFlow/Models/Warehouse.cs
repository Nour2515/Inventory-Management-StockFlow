namespace StockFlow.Models;

public class Warehouse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<StockReservation> StockReservations { get; set; } = new List<StockReservation>();
    public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    public ICollection<WarehouseTransfer> OutgoingTransfers { get; set; } = new List<WarehouseTransfer>();
    public ICollection<WarehouseTransfer> IncomingTransfers { get; set; } = new List<WarehouseTransfer>();
}
