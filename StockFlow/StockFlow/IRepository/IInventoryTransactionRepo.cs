using StockFlow.Models;
using StockFlow.Models.Enums;

namespace StockFlow.IRepository
{
    public interface IInventoryTransactionRepo : IGenericRepository<InventoryTransaction>
    {
        Task<List<InventoryTransaction>> GetByWarehouseandproductIdAsync(int productId, int warehouseId ); 
        Task<List<InventoryTransaction>> GetByProductIdAsync(int productId);
        Task<List<InventoryTransaction>> GetByWarehouseIdAsync(int warehouseId);
        Task<List<InventoryTransaction>> GetByTypeAsync(InventoryTransactionType type);
    }
}
