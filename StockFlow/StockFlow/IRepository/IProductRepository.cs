using StockFlow.Models;

namespace StockFlow.IRepository
{
    public interface IProductRepository: IGenericRepository<Product>
    {
        Task<Product?> GetBySKUAsync(string sku);
        Task<IReadOnlyList<Product>> GetAllWithCategoryAsync();
        Task<Product?> GetByIdWithCategoryAsync(int id);
        Task<bool> ExistsBySKUAsync(string sku);
        Task<bool> ExistsAsync(int id);

    }
}
