using System.ComponentModel.DataAnnotations;

namespace StockFlow.DTOs.Inventory
{
    public class UpdateInventoryRequest
    {
        [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative.")]
        public int ReorderLevel { get; set; }
    }
}
