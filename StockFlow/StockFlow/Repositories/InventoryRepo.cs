using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.IRepository;
using StockFlow.Models;

namespace StockFlow.Repositories
{
    public class InventoryRepo : GenericRepository<Inventory>, IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepo(AppDbContext context) : base(context)
        {
            _context = context;
        }
        //override
        public override async Task<IReadOnlyList<Inventory>> GetAllAsync()
        {
            return await _context.Inventories
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .ToListAsync();
        }
        public override async Task<Inventory?> GetByIdAsync(int id)
        {
            return await _context.Inventories .AsNoTracking().Include(i => i.Product).Include(i => i.Warehouse).FirstOrDefaultAsync(i=>i.Id == id);
        }

        public async Task<Inventory> GetByProductAndWarehouseAsync(int productId, int warehouseId)
        {
            return await _context.Inventories.Include(i=>i.Warehouse).Include(i=>i.Product).FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId);
        }

        public async Task<List<Inventory>> GetByProductIdAsync(int productId)
        {
            return await _context.Inventories.Include(i=>i.Warehouse).Include(i=>i.Product).Where(i => i.ProductId == productId).ToListAsync();
        }

        public async Task<List<Inventory>> GetByWarehouseIdAsync(int warehouseId)
        {
            return await _context.Inventories.AsNoTracking().Include(i => i.Product).Include(i => i.Warehouse).Where(i => i.WarehouseId == warehouseId).ToListAsync();
        }
    }
}
