namespace StockFlow.DTOs.Orders
{
    public class CreateOrderItemRequest
    {
        public int ProductId { get; set; }
        public int quantity { get; set; }

    }
}
