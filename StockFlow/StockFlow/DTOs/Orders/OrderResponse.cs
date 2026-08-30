using StockFlow.Models.Enums;

namespace StockFlow.DTOs.Orders
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<OrderItemResponse> Items { get; set; } = new();
    }
}
