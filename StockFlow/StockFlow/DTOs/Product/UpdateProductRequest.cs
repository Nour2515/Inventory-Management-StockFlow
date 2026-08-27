namespace StockFlow.DTOs.Product
{
    public class UpdateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public bool IsActive { get; set; }
    }
}
