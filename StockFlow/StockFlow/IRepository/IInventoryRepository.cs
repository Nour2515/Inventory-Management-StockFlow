using StockFlow.Models;

namespace StockFlow.IRepository
{
    public interface IInventoryRepository : IGenericRepository<Inventory>
    {
     
        Task<List<Inventory>> GetByProductIdAsync(int productId);

        Task<List<Inventory>> GetByWarehouseIdAsync(int warehouseId);
        Task<Inventory> GetByProductAndWarehouseAsync(int productId, int warehouseId);

    }
}
