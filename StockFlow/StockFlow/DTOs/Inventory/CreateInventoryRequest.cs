using System.ComponentModel.DataAnnotations;

namespace StockFlow.DTOs.Inventory
{
    public class CreateInventoryRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than zero.")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "WarehouseId must be greater than zero.")]
        public int WarehouseId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "On hand quantity cannot be negative.")]
        public int onhandQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative.")]
        public int ReorderLevel { get; set; }
    }
}
