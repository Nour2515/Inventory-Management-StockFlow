using StockFlow.DTOs.Product;

namespace StockFlow.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponse> CreateAsync(CreateProductRequest request);

        Task<IEnumerable<ProductResponse>> GetAllAsync();

        Task<ProductResponse?> GetByIdAsync(int id);

        Task<ProductResponse> UpdateAsync(
            int id,
            UpdateProductRequest request);

        Task<string> DeactivateAsync(int id);
    }
}
