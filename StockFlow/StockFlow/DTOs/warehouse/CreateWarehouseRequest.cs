using System.ComponentModel.DataAnnotations;

namespace StockFlow.DTOs.warehouse
{
    public class CreateWarehouseRequest
    {
        [Required]
        [StringLength(150)]
        public string name { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string location { get; set; } = string.Empty;
    }
}
