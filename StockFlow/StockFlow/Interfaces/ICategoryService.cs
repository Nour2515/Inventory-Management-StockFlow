using StockFlow.DTOs.Catagory;

namespace StockFlow.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);

        Task<IEnumerable<CategoryResponse>> GetAllAsync();

        Task<CategoryResponse?> GetByIdAsync(int id);

        Task<CategoryResponse> UpdateAsync(
            int id,
            UpdateCategoryRequest request);

        Task DeleteAsync(int id);
    }
}
