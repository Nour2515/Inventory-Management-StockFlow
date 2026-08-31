using StockFlow.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace StockFlow.DTOs.Reservation
{
    public class UpdateReservationRequest
    {
        public ReservationStatus Status { get; set; }
    }
}
