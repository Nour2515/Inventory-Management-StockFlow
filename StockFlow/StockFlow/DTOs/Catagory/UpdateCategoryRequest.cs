namespace StockFlow.DTOs.Catagory
{
    public class UpdateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
