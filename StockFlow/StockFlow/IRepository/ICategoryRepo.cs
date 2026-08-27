using StockFlow.Models;

namespace StockFlow.IRepository
{
    public interface ICategoryRepo : IGenericRepository<Category>
    {
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<bool> HasProductsAsync(int categoryId);
        Task<bool> ExistsAsync(int id);


    }
}
