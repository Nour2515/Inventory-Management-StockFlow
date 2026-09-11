using System.ComponentModel.DataAnnotations;

namespace StockFlow.DTOs.Orders
{
    public class CreateOrderItemRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than zero.")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int quantity { get; set; }
    }
}
