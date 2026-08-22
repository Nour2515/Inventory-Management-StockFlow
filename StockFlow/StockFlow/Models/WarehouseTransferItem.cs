namespace StockFlow.Models;

public class WarehouseTransferItem
{
    public int Id { get; set; }
    public int WarehouseTransferId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }

    public WarehouseTransfer WarehouseTransfer { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
