using StockFlow.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace StockFlow.DTOs.Orders
{
    public class UpdateOrderStatusRequest
    {
        [EnumDataType(typeof(OrderStatus))]
        public OrderStatus Status { get; set; }
    }
}
