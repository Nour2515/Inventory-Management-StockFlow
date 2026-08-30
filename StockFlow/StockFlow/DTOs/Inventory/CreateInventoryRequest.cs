namespace StockFlow.DTOs.Inventory
{
    public class CreateInventoryRequest
    {
        public int ProductId { get; set; }

        public int WarehouseId { get; set; }

        public int onhandQuantity { get; set; }

        public int ReorderLevel { get; set; }

    }
}
