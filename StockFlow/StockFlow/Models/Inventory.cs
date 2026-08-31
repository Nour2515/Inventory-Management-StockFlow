using System.ComponentModel.DataAnnotations;

namespace StockFlow.Models;

public class Inventory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int OnHandQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public DateTime UpdatedAt { get; set; }
    //for concurrency
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;

}
