using System.ComponentModel.DataAnnotations;

namespace StockFlow.DTOs.Orders
{
    public class CreateOrderRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "Order must contain at least one item.")]
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }
}
