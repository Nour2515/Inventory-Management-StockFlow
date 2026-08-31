using StockFlow.Models.Enums;

namespace StockFlow.DTOs.Transaction
{
    public class InventoryTransactionResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty; 
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty; 
        public int Quantity { get; set; }
        public InventoryTransactionType Type { get; set; }
        public string? Reference { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
