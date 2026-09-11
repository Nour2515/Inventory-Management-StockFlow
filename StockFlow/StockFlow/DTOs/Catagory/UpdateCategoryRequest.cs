using System.ComponentModel.DataAnnotations;

namespace StockFlow.DTOs.Catagory
{
    public class UpdateCategoryRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
