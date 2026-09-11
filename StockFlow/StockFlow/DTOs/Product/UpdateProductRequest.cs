using System.ComponentModel.DataAnnotations;

namespace StockFlow.DTOs.Product
{
    public class UpdateProductRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(64)]
        public string SKU { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than zero.")]
        public int CategoryId { get; set; }

        public bool IsActive { get; set; }
    }
}
