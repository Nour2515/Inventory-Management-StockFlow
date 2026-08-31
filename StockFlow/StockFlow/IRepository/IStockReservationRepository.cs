using StockFlow.Models;

namespace StockFlow.IRepository
{
    public interface IStockReservationRepository : IGenericRepository<StockReservation>
    {
        Task<StockReservation?> GetByIdWithDetailsAsync(int id); 
        Task<List<StockReservation>> GetByOrderIdAsync(int orderId); 
        Task<List<StockReservation>> GetActiveByProductAndWarehouseAsync(int productId, int warehouseId);
        Task<List<StockReservation>> GetActiveByOrderIdAsync(int orderId);
    }
}
