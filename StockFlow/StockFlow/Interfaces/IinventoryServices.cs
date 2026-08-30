using StockFlow.DTOs.Inventory;

namespace StockFlow.Interfaces
{
    public interface IinventoryServices
    {
        Task<InventoryResponse> CreateAsync(CreateInventoryRequest request);

        Task<IEnumerable<InventoryResponse>> GetAllAsync();

        Task<InventoryResponse?> GetByIdAsync(int id);

        Task<InventoryResponse?> UpdateAsync(
            int id,
            UpdateInventoryRequest request);

        Task DeleteAsync(int id);

        Task<StockAvailabilityResponse> CheckAvailabilityAsync(int productId, int warehouseId, int quantity);
        Task<List<InventoryResponse>> GetByWarehouseIdAsync(int warehouseId);
        Task<List<InventoryResponse>> GetByproductIdAsync(int ProductId);


    }
}
