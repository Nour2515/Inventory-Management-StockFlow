using System.ComponentModel.DataAnnotations;

namespace StockFlow.DTOs.Reservation
{
    public class CreateReservationRequest
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int Quantity { get; set; }
    }
}
