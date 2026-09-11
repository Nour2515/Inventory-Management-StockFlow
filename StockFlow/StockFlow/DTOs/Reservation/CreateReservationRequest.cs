using System.ComponentModel.DataAnnotations;

namespace StockFlow.DTOs.Reservation
{
    public class CreateReservationRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "OrderId must be greater than zero.")]
        public int OrderId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than zero.")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "WarehouseId must be greater than zero.")]
        public int WarehouseId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }
    }
}
