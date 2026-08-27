using StockFlow.DTOs.Catagory;
using StockFlow.Interfaces;
using StockFlow.IRepository;
using StockFlow.Models;
using System.Collections;

namespace StockFlow.Services
{
    public class CategoryServices : ICategoryService
    {
        private readonly ICategoryRepo _categoryRepository;
        public CategoryServices(ICategoryRepo categoryRepository)
        {
            _categoryRepository = categoryRepository;

        }
        public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
        {
            var exists = await _categoryRepository.ExistsByNameAsync(request.Name);
            if (exists)
                throw new Exception("Category name already exists.");
            Category c = new Category
            {
                Name = request.Name,
                Description = request.Description
            };
            await _categoryRepository.AddAsync(c);
            await _categoryRepository.SaveChangesAsync();
            return new CategoryResponse { Id = c.Id, Name = c.Name, Description = c.Description };
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            });
        }

        public async Task<CategoryResponse?> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return null;
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new Exception("Category not found.");

            category.Name = request.Name;
            category.Description = request.Description;

            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();

            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }
        //important 
        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new Exception("Category not found.");
            var hasProducts = await _categoryRepository.HasProductsAsync(id);
            if (hasProducts)
                throw new Exception("Cannot delete category because it contains products");
            _categoryRepository.Delete(category);
            await _categoryRepository.SaveChangesAsync();
        }


    }
}