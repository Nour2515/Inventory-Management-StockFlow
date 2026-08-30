namespace StockFlow.DTOs.Inventory
{
    public class StockAvailabilityResponse
    {
        public int productId { get; set; }
        public int warehouseId { get; set; }
        public int requstedQuantity { get; set; }
        public int availableQuantity { get; set; }
        public bool isAvailable { get; set; }
    }
}
