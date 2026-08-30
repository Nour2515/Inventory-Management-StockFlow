using StockFlow.Models.Enums;

namespace StockFlow.DTOs.Orders
{
    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }

    }
}
