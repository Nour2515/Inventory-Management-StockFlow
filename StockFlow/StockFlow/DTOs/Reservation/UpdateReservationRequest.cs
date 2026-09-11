using StockFlow.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace StockFlow.DTOs.Reservation
{
    public class UpdateReservationRequest
    {
        [EnumDataType(typeof(ReservationStatus))]
        public ReservationStatus Status { get; set; }
    }
}
