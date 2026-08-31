using StockFlow.Models.Enums;

namespace StockFlow.DTOs.Reservation
{
    public class ReservationResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty; 
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty; 
        public int Quantity { get; set; }
        public DateTime ExpiresAt { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
