
using StockFlow.DTOs.warehouse;

namespace StockFlow.Interfaces
{
    public interface IWarehouseService
    {
        Task<WarehouseResponse> CreateAsync(CreateWarehouseRequest request);

        Task<IEnumerable<WarehouseResponse>> GetAllAsync();

        Task<WarehouseResponse?> GetByIdAsync(int id);

        Task<WarehouseResponse> UpdateAsync(
            int id,
            UpdateWarehouseRequest request);

        Task<string> Deactivate(int id);
    }
}
