using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.IRepository;
using StockFlow.Models;
using StockFlow.Models.Enums;

namespace StockFlow.Repositories
{
    public class InventoryTransactionRepo : GenericRepository<InventoryTransaction> , IInventoryTransactionRepo
    {
        private readonly AppDbContext _context;
        public InventoryTransactionRepo(AppDbContext context) : base(context)
        {
            _context = context;
        
        }
        public override async Task<IReadOnlyList<InventoryTransaction>> GetAllAsync()
        {
            return await _context.InventoryTransactions.Include(p => p.Product).Include(w => w.Warehouse).ToListAsync();
        }

        public override async Task<InventoryTransaction?> GetByIdAsync(int id)
        {
            return await _context.InventoryTransactions.Include(p => p.Product).Include(w => w.Warehouse).FirstOrDefaultAsync(i => i.Id == id);
        }
        public async Task<List<InventoryTransaction>> GetByProductIdAsync(int productId)
        {
            return await _context.InventoryTransactions
                .Where(t => t.ProductId == productId)
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        public async Task<List<InventoryTransaction>> GetByWarehouseIdAsync(int warehouseId)
        {
            return await _context.InventoryTransactions.
                Where(t => t.WarehouseId == warehouseId)
                .Include(t => t.Product)
                .Include(t => t.Warehouse).
                AsNoTracking().
                OrderByDescending(t => t.CreatedAt).
                ToListAsync();
        }
        public async Task<List<InventoryTransaction>> GetByWarehouseandproductIdAsync(int ProductId, int warehouseId) 
        { 
            return await _context.InventoryTransactions.
                Where(t => t.ProductId == ProductId && t.WarehouseId == warehouseId).
                Include(t => t.Product)
                .Include(t => t.Warehouse).
                AsNoTracking().
                OrderByDescending(t => t.CreatedAt).
                ToListAsync(); 
        }
        public async Task<List<InventoryTransaction>> GetByTypeAsync(InventoryTransactionType type) { 
            return await _context.InventoryTransactions.
                Where(t => t.Type == type).Include(t => t.Product)
                .Include(t => t.Warehouse).AsNoTracking().
                OrderByDescending(t => t.CreatedAt).
                ToListAsync();
        }

       

    
    }
}
