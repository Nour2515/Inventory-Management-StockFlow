using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.IRepository;
using StockFlow.Models;
using StockFlow.Models.Enums;

namespace StockFlow.Repositories
{
    public class StockReservationRepo:GenericRepository<StockReservation> , IStockReservationRepository
    {
        private readonly AppDbContext _context; 
        public StockReservationRepo(AppDbContext context) : base(context) 
        { 
            _context = context;
        }

        public  Task<List<StockReservation>> GetAllAsync()
        {
            return _context.StockReservations.Include(o => o.Order).Include(w => w.Warehouse).Include(p => p.Product).ThenInclude(I => I.Inventories).ToListAsync();
        }
        public async Task<StockReservation?> GetByIdWithDetailsAsync(int id) 
        { 
            return await _context.StockReservations.Include(r => r.Order).
                Include(r => r.Product).
                Include(r => r.Warehouse).
                FirstOrDefaultAsync(r => r.Id == id); 
        }
        public async Task<List<StockReservation>> GetByOrderIdAsync(int orderId)
        { return await _context.StockReservations.
                Where(r => r.OrderId == orderId).
                Include(r => r.Product).
                Include(r => r.Warehouse).
                AsNoTracking().
                ToListAsync();
        }
        public async Task<List<StockReservation>> GetActiveByProductAndWarehouseAsync(int productId, int warehouseId) 
        { return await _context.StockReservations.
                Where(r => r.ProductId == productId && r.WarehouseId == warehouseId && r.Status == ReservationStatus.Active)
                .ToListAsync(); 
        }
        public async Task<List<StockReservation>> GetActiveByOrderIdAsync(int orderId)
        { 
            return await _context.StockReservations.
                Where(r => r.OrderId == orderId && r.Status == ReservationStatus.Active).
                Include(r => r.Product).
                Include(r => r.Warehouse).
                ToListAsync();
        }
    }
}
