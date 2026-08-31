using StockFlow.DTOs.Reservation;

namespace StockFlow.Interfaces
{
    public interface IStockReservationService
    {
        Task<IEnumerable<ReservationResponse>> GetAllReservations();
        Task<ReservationResponse> CreateAsync(CreateReservationRequest request);
        Task<ReservationResponse?> GetByIdAsync(int id);
        Task<IEnumerable<ReservationResponse>> GetByOrderIdAsync(int orderId);
        Task ReleaseAsync(int id); Task CancelAsync(int id);
    }
}
