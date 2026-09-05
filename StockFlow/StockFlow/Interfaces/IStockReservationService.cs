using StockFlow.DTOs.Reservation;
using StockFlow.Models;

namespace StockFlow.Interfaces
{
    public interface IStockReservationService
    {
        Task<IEnumerable<ReservationResponse>> GetAllReservations();
        Task<ReservationResponse> CreateAsync(CreateReservationRequest request,int userId);
        Task<ReservationResponse?> GetByIdAsync(int id);
        Task<IEnumerable<ReservationResponse>> GetByOrderIdAsync(int orderId);
        Task ReleaseAsync(int id, int userId);
        Task CancelAsync(int id, int userId);

        Task ExpireReservationsAsync();
    }
}
