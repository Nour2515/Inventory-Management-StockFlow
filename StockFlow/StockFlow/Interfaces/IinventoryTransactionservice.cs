using StockFlow.DTOs.Transaction;
using StockFlow.Models.Enums;

namespace StockFlow.Interfaces
{
    public interface IinventoryTransactionservice
    {
        Task<IEnumerable<InventoryTransactionResponse>> GetAllAsync();
        Task<InventoryTransactionResponse?> GetByIdAsync(int id);
        Task<IEnumerable<InventoryTransactionResponse>> GetByProductIdAsync(int productId);
        Task<IEnumerable<InventoryTransactionResponse>> GetByWarehouseIdAsync(int warehouseId);
        Task<IEnumerable<InventoryTransactionResponse>> GetByProductAndWarehouseAsync(int productId, int warehouseId);
        Task<IEnumerable<InventoryTransactionResponse>> GetByTypeAsync(InventoryTransactionType type);
        Task<InventoryTransactionResponse> ProcessTransactionAsync(CreateInventoryTransactionRequest request);
    }
}
