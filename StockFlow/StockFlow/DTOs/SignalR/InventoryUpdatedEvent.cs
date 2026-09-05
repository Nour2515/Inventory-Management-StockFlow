namespace StockFlow.DTOs.SignalR
{
    public class InventoryUpdatedEvent
    {
        public int InventoryId { get; set; }

        public int ProductId { get; set; }

        public int WarehouseId { get; set; }

        public int OnHandQuantity { get; set; }

        public int ReservedQuantity { get; set; }

        public int AvailableQuantity { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
