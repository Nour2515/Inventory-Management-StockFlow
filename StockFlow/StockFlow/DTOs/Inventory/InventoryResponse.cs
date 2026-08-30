namespace StockFlow.DTOs.Inventory
{
    public class InventoryResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int OnHandQuantity { get; set; }
        public int ReservedQuantity { get; set; } = 0;

        public int AvailableQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
