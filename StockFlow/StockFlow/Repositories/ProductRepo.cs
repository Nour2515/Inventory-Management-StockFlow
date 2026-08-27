using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.IRepository;
using StockFlow.Models;

namespace StockFlow.Repositories
{
    public class ProductRepo : GenericRepository<Product>, IProductRepository
    {

        private readonly AppDbContext _context;

        public ProductRepo(AppDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<Product?> GetBySKUAsync(string sku)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.SKU == sku);
        }
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products
                .AnyAsync(p => p.Id == id);
        }

        public async Task<bool> ExistsBySKUAsync(
            string sku)
        {
            return await _context.Products
                .AnyAsync(
                    p => p.SKU == sku);
        }

        public async Task<IReadOnlyList<Product>> GetAllWithCategoryAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Product?> GetByIdWithCategoryAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}