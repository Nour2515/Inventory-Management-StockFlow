using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.IRepository;
using StockFlow.Models;

namespace StockFlow.Repositories
{
    public class CategoryRepo : GenericRepository<Category>, ICategoryRepo
    {
        private readonly AppDbContext _context;

        public CategoryRepo(AppDbContext context): base(context) 
        { _context = context; }

        public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .AnyAsync(
                    c => c.Name == name,
                    cancellationToken);
        }
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories
                .AnyAsync(c => c.Id == id);
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.Products
                .AnyAsync(
                    p => p.CategoryId == categoryId);

        }
    }
}